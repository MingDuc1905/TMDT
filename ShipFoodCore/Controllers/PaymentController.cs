// ============================================================
// 💳 PaymentController — Xử lý thanh toán | Payment Processing
// ============================================================
// Ý nghĩa: Controller trung tâm xử lý tất cả giao dịch thanh toán
// Chức năng: ProcessPayment (COD/VNPAY), VNPAY IPN callback, Bank Webhook,
//            nạp/rút ví, E-Invoice generation, SignalR broadcast,
//            atomic transaction, multi-restaurant orders, idempotency lock
// KEYWORDS: payment, thanh toán, VNPAY, COD, bank transfer, webhook,
//           process payment, invoice, batch order
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   Trigger: POST /Payment/ProcessPayment (từ Cart/Checkout)
//   Calls →: BaseController (GetCart, SetCart, GetCurrentUser, CheckLogin)
//            VnpayService (CreatePaymentUrl, VerifySignature)
//            EDeliveryService (GenerateEInvoice sau thanh toán thành công)
//            Chats Hub (IHubContext — newOrder, paymentConfirmed, orderStatusChanged)
//            BankHelper (GetVietQrBinCode)
//            FastShipConstants (SHIP_FEE)
//            Models: tbDonHang, tbChiTietDonHang, tbThongTinDatHang, tbBienTheMonAn
//   Called by ←: Cart/Checkout.cshtml (AJAX POST)
//                 BankWebhook (POST từ SePay/Casso/PayOS)
//                 VNPAY (IPN callback + Return URL redirect)
//   Flow: Checkout → ProcessPayment → 1. Validate → 2. Idempotency Lock
//        → 3. Re-read giá DB → 4. Atomic transaction (multi-restaurant loop)
//        → 5. SignalR newOrder → Restaurant nhận → 6. Xóa cart
//        → 7. VNPAY URL (nếu chọn VNPAY) / COD success ngay
//   BankWebhook: SePay/Casso → parse memo → FASTSHIP{id} / FASTSHIPNAP{uid}
//        → xác thực token FixedTimeEquals → update DB → SignalR broadcast
//   VNPAY IPN: POST callback → verify HMAC SHA512 → update trạng thái
//        → GenerateEInvoice → SignalR paymentConfirmed + newOrder
// ============================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ShipFood.Hubs;
using ShipFood.Helpers;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Controllers;

public class PaymentController : BaseController
{
    private readonly ILogger<PaymentController> _logger;
    private readonly IHubContext<Chats> _hubContext;
    private readonly VnpayService _vnpayService;
    private readonly IConfiguration _configuration;
    private readonly EDeliveryService _eDelivery;

    public PaymentController(dbFoodyEntities context, ILogger<PaymentController> logger, IHubContext<Chats> hubContext,
        VnpayService vnpayService, IConfiguration configuration, EDeliveryService eDelivery)
    {
        db = context;
        _logger = logger;
        _hubContext = hubContext;
        _vnpayService = vnpayService;
        _configuration = configuration;
        _eDelivery = eDelivery;
    }

    // ─── Bank transfer config (giữ lại cho BankWebhook backward compatibility) ───
    private string BankId => _configuration["BANK_ID"] ?? "970436";
    private string BankVietQrBinCode => BankHelper.GetVietQrBinCode(BankId);
    private string BankAccountNo => _configuration["BANK_ACCOUNT_NO"] ?? "1234567890";
    private string BankAccountName => _configuration["BANK_ACCOUNT_NAME"] ?? "FASTSHIP CO., LTD";
    private string BankWebhookToken => _configuration["BANK_WEBHOOK_TOKEN"] ?? "";

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> ProcessPayment(int? mattdh, string? hoten, string? quan, string? diachicuthe,
        string? diachiadd, string? SDT, string? note, int pttt, string testResult = "success", int? makhuyenmai = null)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập để tiếp tục" });

        if (testResult != "success" && testResult != "failure")
            return Json(new { success = false, message = "Tham số không hợp lệ" });

        if (mattdh == null)
        {
            if (string.IsNullOrWhiteSpace(hoten) || hoten.Length < 2 || hoten.Length > 100)
                return Json(new { success = false, message = "Họ tên phải từ 2-100 ký tự" });
            if (string.IsNullOrWhiteSpace(SDT) || !System.Text.RegularExpressions.Regex.IsMatch(SDT, @"^0[1-9][0-9]{8,9}$"))
                return Json(new { success = false, message = "Số điện thoại không hợp lệ" });
            if (string.IsNullOrWhiteSpace(diachicuthe) || diachicuthe.Length < 5)
                return Json(new { success = false, message = "Địa chỉ cụ thể quá ngắn" });
        }

        var cart = GetCart();
        if (cart == null || cart.items.Count == 0)
            return Json(new { success = false, message = "Giỏ hàng trống." });

        // ═══ MULTI-RESTAURANT: Lấy danh sách quán từ cart items ═══
        var restaurantIds = cart.items
            .Where(i => i.maquanan.HasValue)
            .Select(i => i.maquanan!.Value)
            .Distinct()
            .ToList();

        if (restaurantIds.Count == 0)
            return Json(new { success = false, message = "Giỏ hàng không có thông tin quán ăn. Vui lòng thêm món lại." });

        if (testResult == "failure")
        {
            var failures = new[] {
                "Thẻ của bạn đã hết hạn.",
                "Số dư tài khoản không đủ.",
                "Giao dịch bị từ chối do ngân hàng.",
                "Mã xác thực OTP không hợp lệ.",
                "Phiên thanh toán đã hết hạn."
            };
            var msg = failures[new System.Random().Next(failures.Length)];
            return Json(new { success = false, message = msg, keepCart = true });
        }

        try
        {
            var user = GetCurrentUser();

            // ═══ FIX 1: Atomic transaction — đảm bảo order + details được save hoặc rollback cùng nhau ═══
            using var transaction = db.Database.BeginTransaction();

            // ═══ 1 PENDING ORDER PER USER ═══
            var pendingOrder = db.tbDonHang
                .Where(dh => dh.tbThongTinDatHang != null
                    && dh.tbThongTinDatHang.userid == user!.userid
                    && dh.trangthai == "Chờ thanh toán")
                .OrderByDescending(dh => dh.ngaydathang)
                .FirstOrDefault();
            if (pendingOrder != null)
            {
                return Json(new
                {
                    success = false,
                    message = $"Bạn đang có đơn hàng #{pendingOrder.madh} chờ thanh toán. Vui lòng thanh toán hoặc đợi đơn hết hạn (15 phút) trước khi tạo đơn mới.",
                    keepCart = false
                });
            }

            // ═══ IDEMPOTENCY LOCK ═══
            var recentOrderCount = db.tbDonHang
                .Where(dh => dh.tbThongTinDatHang != null
                    && dh.tbThongTinDatHang.userid == user!.userid
                    && dh.ngaydathang >= DateTime.Now.AddSeconds(-30))
                .Count();
            if (recentOrderCount > 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Bạn vừa đặt hàng thành công trong 30 giây qua. Vui lòng kiểm tra lịch sử đơn hàng hoặc thử lại sau.",
                    keepCart = false
                });
            }

            // ═══ MULTI-DEVICE CHECK ═══
            var recentMultiDeviceOrder = db.tbDonHang
                .Where(dh => dh.tbThongTinDatHang != null
                    && dh.tbThongTinDatHang.userid == user!.userid
                    && dh.ngaydathang >= DateTime.Now.AddMinutes(-5))
                .OrderByDescending(dh => dh.ngaydathang)
                .FirstOrDefault();
            if (recentMultiDeviceOrder != null && recentMultiDeviceOrder.madh > 0)
            {
                var staleCart = GetCart();
                if (staleCart != null && staleCart.items.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Giỏ hàng của bạn đã được xử lý trên thiết bị khác. Vui lòng tải lại trang để làm mới giỏ hàng.",
                        keepCart = false
                    });
                }
            }

            tbThongTinDatHang ttdh;

            if (mattdh != null)
            {
                ttdh = db.tbThongTinDatHang.Find(mattdh)!;
                if (ttdh == null)
                    return Json(new { success = false, message = "Địa chỉ đã lưu không còn tồn tại" });
                if (ttdh.userid != user!.userid)
                    return Json(new { success = false, message = "Không có quyền sử dụng địa chỉ này" });
            }
            else
            {
                ttdh = new tbThongTinDatHang
                {
                    userid = user!.userid,
                    sdt = SDT ?? "",
                    diachi = string.Join(", ", new[] { diachiadd, diachicuthe, quan, "TP. Hồ Chí Minh" }
                               .Where(s => !string.IsNullOrWhiteSpace(s))),
                    toado = null,
                    tennguoinhan = hoten ?? ""
                };
                db.tbThongTinDatHang.Add(ttdh);
                db.SaveChanges();
            }

            // ═══ Re-read giá từ DB ═══
            decimal tongTienMon = 0;
            foreach (var item in cart.items)
            {
                var bt = db.tbBienTheMonAn.Find(item.mabienthe);
                if (bt?.giatien == null)
                {
                    return Json(new { success = false, message = $"Món '{item.tenmon}' không còn tồn tại hoặc đã thay đổi giá. Vui lòng tải lại giỏ hàng." });
                }
                var monAn = db.tbMonAn.Find(item.mamon);
                if (monAn != null && monAn.conhang == false)
                {
                    return Json(new { success = false, message = $"Món '{item.tenmon}' đã hết hàng. Vui lòng xóa khỏi giỏ hàng trước khi thanh toán." });
                }
                item.giatien = bt.giatien;
                tongTienMon += (bt.giatien ?? 0) * item.soLuong;
            }

            decimal phiShip = ShipFood.Helpers.FastShipConstants.SHIP_FEE;
            decimal discountAmount = 0;
            int? appliedCouponId = null;

            if (makhuyenmai != null)
            {
                var coupon = db.tbKhuyenMai.Find(makhuyenmai);
                if (coupon != null && (coupon.ngayketthuc == null || coupon.ngayketthuc >= DateTime.Now))
                {
                    var loai = (coupon.loaikm ?? "").ToLowerInvariant();
                    if (loai == "free ship" || loai == "miễn phí ship" || loai == "miễn phí vận chuyển")
                    {
                        phiShip = 0;
                    }
                    else
                    {
                        int phanTram = coupon.phantramgiam ?? 0;
                        discountAmount = tongTienMon * phanTram / 100;
                    }
                    appliedCouponId = coupon.makm;
                }
            }

            // ═══ Xác định trạng thái ban đầu dựa trên phương thức thanh toán ═══
            // COD (Tiền mặt) → "Đã đặt"
            // VNPAY → "Chờ thanh toán" (chờ VNPAY IPN xác nhận)
            bool isVnpay = IsVnpayMethod(pttt);
            var trangThaiBanDau = isVnpay ? "Chờ thanh toán" : "Đã đặt";

            var createdOrders = new List<int>();
            decimal totalAllOrders = 0;
            bool shipFeeApplied = false;

            foreach (var resId in restaurantIds)
            {
                var resItems = cart.items.Where(i => i.maquanan == resId).ToList();
                if (resItems.Count == 0) continue;

                decimal resTongTienMon = 0;
                foreach (var item in resItems)
                {
                    var bt = db.tbBienTheMonAn.Find(item.mabienthe);
                    if (bt?.giatien == null)
                    {
                        return Json(new { success = false, message = $"Món '{item.tenmon}' không còn tồn tại hoặc đã thay đổi giá. Vui lòng tải lại giỏ hàng." });
                    }
                    var monAn = db.tbMonAn.Find(item.mamon);
                    if (monAn != null && monAn.conhang == false)
                    {
                        return Json(new { success = false, message = $"Món '{item.tenmon}' đã hết hàng. Vui lòng xóa khỏi giỏ hàng trước khi thanh toán." });
                    }
                    item.giatien = bt.giatien;
                    resTongTienMon += (bt.giatien ?? 0) * item.soLuong;
                }

                decimal resDiscount = (resId == restaurantIds.First()) ? discountAmount : 0;
                decimal resShipFee = shipFeeApplied ? 0 : phiShip;
                shipFeeApplied = true;
                decimal resTongCong = Math.Max(resTongTienMon + resShipFee - resDiscount, 0);
                totalAllOrders += resTongCong;

                var dh = new tbDonHang
                {
                    maquan    = resId,
                    mattdh    = ttdh.mattdh,
                    ngaydathang = DateTime.Now,
                    trangthai = trangThaiBanDau,
                    tongtien  = resTongCong,
                    hinhthucthanhtoan = pttt,
                    ghichu    = note,
                    phiship   = phiShip,
                    makhuyenmai = (resId == restaurantIds.First()) ? appliedCouponId : null
                };
                db.tbDonHang.Add(dh);
                db.SaveChanges();

                foreach (var item in resItems)
                {
                    db.tbChiTietDonHang.Add(new tbChiTietDonHang
                    {
                        madh    = dh.madh,
                        mamon   = item.mabienthe,
                        soluong = item.soLuong,
                        dongia  = item.giatien
                    });
                }
                db.SaveChanges();

                createdOrders.Add(dh.madh);
                // ═══ FIX 3: Diagnostic logging chi tiết để debug order không hiển thị ═══
                _logger.LogInformation("Order #{OrderId} created: maquan={ResId}, maKhach={Userid}, trangthai={Status}, "
                    + "ttdh={TtdhId}, restaurantIds=[{RestaurantIds}], createdOrderIds=[{CreatedIds}]",
                    dh.madh, resId, user!.userid, trangThaiBanDau,
                    ttdh.mattdh, string.Join(",", restaurantIds), string.Join(",", createdOrders.Concat(new[] { dh.madh })));

                dh.phiship = resShipFee;

                // ═══ SignalR: Luôn thông báo cho quán khi có đơn mới (k? c? VNPAY) ═══
                // Quán cần th?y d?n d? chu?n b?, dù dang ch? thanh toán
                try
                {
                    await _hubContext.Clients.Group($"restaurant_{resId}").SendAsync("newOrder", new
                    {
                        orderId = dh.madh,
                        customerName = hoten ?? "Khách",
                        totalAmount = resTongCong,
                        status = trangThaiBanDau,
                        time = DateTime.Now.ToString("HH:mm")
                    });
                }
                catch { }
            }

            // ─── Coupon usage ───
            if (appliedCouponId != null)
            {
                try
                {
                    db.tbLichSuSuDungKhuyenMai.Add(new tbLichSuSuDungKhuyenMai
                    {
                        userid = user!.userid,
                        makm = appliedCouponId.Value,
                        ngaydung = DateTime.Now,
                        madh = createdOrders.First()
                    });
                    db.SaveChanges();
                }
                catch (Exception couponEx) { _logger.LogWarning(couponEx, "Failed to record coupon usage for order #{OrderId}", createdOrders.FirstOrDefault()); }
            }

            var firstOrderId = createdOrders.FirstOrDefault();

            // ✅ Commit transaction — tất cả saves đều thành công
            transaction.Commit();

            // Xóa giỏ hàng sau khi đặt thành công
            SetCart(new Cart());

            // ─── VNPAY: Tạo URL thanh toán ───
            if (isVnpay && firstOrderId > 0)
            {
                try
                {
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                    var returnUrl = $"{Request.Scheme}://{Request.Host}/Payment/VnpayReturn";
                    var orderInfo = $"FastShip thanh toan don hang #{firstOrderId}";

                    var vnpayUrl = _vnpayService.CreatePaymentUrl(
                        firstOrderId,
                        (long)totalAllOrders,
                        orderInfo,
                        ipAddress,
                        returnUrl
                    );

                    if (!string.IsNullOrEmpty(vnpayUrl))
                    {
                        _logger.LogInformation("VNPAY payment URL created for order #{OrderId}", firstOrderId);
                        return Json(new
                        {
                            success = true,
                            message = $"Đã tạo {createdOrders.Count} đơn hàng! Chuyển hướng đến VNPAY...",
                            orderId = firstOrderId,
                            orderIds = createdOrders,
                            trangthai = "Chờ thanh toán",
                            paymentMethod = "vnpay",
                            vnpayUrl = vnpayUrl
                        });
                    }
                    else
                    {
                        _logger.LogError("VNPAY payment URL creation failed for order #{OrderId} — missing credentials", firstOrderId);
                        // Xóa đơn nếu VNPAY không được cấu hình
                        try
                        {
                            var ordersToDelete = db.tbDonHang.Where(o => createdOrders.Contains(o.madh)).ToList();
                            var detailIds = db.tbChiTietDonHang.Where(c => createdOrders.Contains((int)c.madh)).ToList();
                            db.tbChiTietDonHang.RemoveRange(detailIds);
                            db.tbDonHang.RemoveRange(ordersToDelete);
                            await db.SaveChangesAsync();
                        }
                        catch (Exception delEx) { _logger.LogError(delEx, "Failed to delete orders after VNPAY failure"); }
                        return Json(new { success = false, message = "Cổng thanh toán VNPAY chưa được cấu hình. Vui lòng thử phương thức thanh toán khác.", keepCart = true });
                    }
                }
                catch (Exception vnpayEx)
                {
                    _logger.LogError(vnpayEx, "VNPAY payment creation failed for order #{OrderId}", firstOrderId);
                    // Xóa đơn nếu VNPAY exception
                    try
                    {
                        var ordersToDelete = db.tbDonHang.Where(o => createdOrders.Contains(o.madh)).ToList();
                        var detailIds = db.tbChiTietDonHang.Where(c => createdOrders.Contains((int)c.madh)).ToList();
                        db.tbChiTietDonHang.RemoveRange(detailIds);
                        db.tbDonHang.RemoveRange(ordersToDelete);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception delEx) { _logger.LogError(delEx, "Failed to delete orders after VNPAY exception"); }
                    return Json(new { success = false, message = "Lỗi kết nối cổng thanh toán VNPAY. Vui lòng thử lại sau.", keepCart = true });
                }
            }

            // ─── COD Success ───
            _logger.LogInformation("Orders created successfully (COD): {OrderIds}", string.Join(",", createdOrders));
            return Json(new
            {
                success  = true,
                message  = $"Đặt hàng thành công! Đã tạo {createdOrders.Count} đơn hàng.",
                orderId  = firstOrderId,
                orderIds = createdOrders,
                trangthai = "Đã đặt",
                paymentMethod = "cod"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProcessPayment failed for user {User}", GetCurrentUser()?.userid);
            _logger.LogError(ex, "Payment failed. Inner: {InnerMessage}",
                (ex is DbUpdateException due && due.InnerException != null) ? due.InnerException.Message : ex.Message);

            var errorMessage = ex switch
            {
                DbUpdateException de when de.InnerException?.Message?.Contains("FK_") == true
                    => "Dữ liệu không hợp lệ: ràng buộc khóa ngoại bị vi phạm.",
                DbUpdateException de when de.InnerException?.Message?.Contains("UNIQUE") == true
                    => "Dữ liệu bị trùng lặp: đơn hàng này đã tồn tại.",
                DbUpdateException de when de.InnerException?.Message?.Contains("timeout") == true
                    => "Kết nối cơ sở dữ liệu bị timeout. Vui lòng thử lại.",
                DbUpdateException de when (de.InnerException?.Message?.Contains("doesn't exist") == true
                    || de.InnerException?.Message?.Contains("not exist") == true
                    || de.InnerException?.Message?.Contains("Unknown table") == true)
                    => "Lỗi cấu trúc cơ sở dữ liệu: bảng chưa được tạo. Vui lòng liên hệ quản trị viên.",
                OperationCanceledException _
                    => "Yêu cầu đã bị hủy do quá thời gian chờ. Vui lòng thử lại.",
                InvalidOperationException ioe when ioe.Message.Contains("session")
                    => "Phiên đặt hàng đã hết hạn. Vui lòng đăng nhập lại.",
                _ => $"Lỗi hệ thống: {(ex is DbUpdateException dbEx && dbEx.InnerException != null ? dbEx.InnerException.Message : ex.Message)}. Vui lòng thử lại hoặc liên hệ hỗ trợ."
            };

            return Json(new { success = false, message = errorMessage, keepCart = true });
        }
    }

    /// <summary>
    /// Kiểm tra phương thức thanh toán có phải VNPAY không
    /// </summary>
    private bool IsVnpayMethod(int pttt)
    {
        try
        {
            var method = db.tbLoaiHinhThanhToan.Find(pttt);
            if (method == null) return false;
            var name = (method.tenhinhthuc ?? "").ToLowerInvariant();
            return name.Contains("vnpay");
        }
        catch { return false; }
    }

    // ═══════════════════════════════════════════════════════════════
    // VNPAY Payment Integration
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// API: Tạo link thanh toán VNPAY (gọi từ frontend sau khi tạo đơn)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> CreateVnpayPayment(int orderId)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        try
        {
            var donHang = await db.tbDonHangs.FindAsync(orderId);
            if (donHang == null)
                return Json(new { success = false, message = "Đơn hàng không tồn tại" });

            // Kiểm tra quyền sở hữu
            var user = GetCurrentUser();
            var ttdh = await db.tbThongTinDatHangs.FindAsync(donHang.mattdh);
            if (ttdh?.userid != user?.userid)
                return Json(new { success = false, message = "Không có quyền thanh toán đơn hàng này" });

            if (donHang.trangthai != "Chờ thanh toán")
                return Json(new { success = false, message = $"Đơn hàng ở trạng thái '{donHang.trangthai}', không thể thanh toán VNPAY" });

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var returnUrl = $"{Request.Scheme}://{Request.Host}/Payment/VnpayReturn";
            var orderInfo = $"FastShip thanh toan don hang #{orderId}";
            var amount = (long)(donHang.tongtien ?? 0);

            var vnpayUrl = _vnpayService.CreatePaymentUrl(orderId, amount, orderInfo, ipAddress, returnUrl);

            if (string.IsNullOrEmpty(vnpayUrl))
            {
                _logger.LogError("VNPAY CreateVnpayPayment failed — credentials not configured");
                return Json(new { success = false, message = "Cổng thanh toán VNPAY chưa được cấu hình. Vui lòng liên hệ quản trị viên." });
            }

            _logger.LogInformation("VNPAY payment URL created for order #{OrderId}, amount={Amount}", orderId, amount);
            return Json(new { success = true, vnpayUrl = vnpayUrl, message = "Chuyển hướng đến VNPAY..." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateVnpayPayment failed for order #{OrderId}", orderId);
            return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
        }
    }

    /// <summary>
    /// API: VNPAY IPN Callback (VNPAY gọi POST để xác nhận kết quả thanh toán)
    /// Xác thực chữ ký, cập nhật trạng thái đơn hàng + SignalR broadcast
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<JsonResult> VnpayIPN()
    {
        try
        {
            // Đọc tất cả tham số từ query string (VNPAY gửi IPN qua GET/POST params)
            var vnpParams = new Dictionary<string, string>();
            foreach (var key in Request.Query.Keys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    vnpParams[key] = Request.Query[key].ToString() ?? "";
                }
            }

            // Nếu không có query params, thử đọc từ form body
            if (vnpParams.Count == 0 && Request.HasFormContentType)
            {
                foreach (var key in Request.Form.Keys)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        vnpParams[key] = Request.Form[key].ToString() ?? "";
                    }
                }
            }

            if (vnpParams.Count == 0)
            {
                _logger.LogWarning("VNPAY IPN received with no parameters");
                return Json(new { RspCode = "01", Message = "No params" });
            }

            // Lấy các tham số quan trọng
            vnpParams.TryGetValue("vnp_ResponseCode", out var responseCode);
            vnpParams.TryGetValue("vnp_TxnRef", out var txnRef);
            vnpParams.TryGetValue("vnp_TransactionNo", out var transactionNo);
            vnpParams.TryGetValue("vnp_Amount", out var vnpAmountStr);
            vnpParams.TryGetValue("vnp_OrderInfo", out var orderInfo);

            _logger.LogInformation("VNPAY IPN received: TxnRef={TxnRef}, ResponseCode={ResponseCode}, TransactionNo={TransactionNo}",
                txnRef, responseCode, transactionNo);

            // Xác thực chữ ký
            if (!_vnpayService.VerifySignature(vnpParams))
            {
                _logger.LogWarning("VNPAY IPN signature verification FAILED for TxnRef={TxnRef}", txnRef);
                return Json(new { RspCode = "97", Message = "Invalid signature" });
            }

            // Parse mã đơn hàng
            if (string.IsNullOrEmpty(txnRef) || !int.TryParse(txnRef, out var madh))
            {
                _logger.LogWarning("VNPAY IPN: Invalid TxnRef: {TxnRef}", txnRef);
                return Json(new { RspCode = "01", Message = "Invalid TxnRef" });
            }

            var donHang = await db.tbDonHangs.FindAsync(madh);
            if (donHang == null)
            {
                _logger.LogWarning("VNPAY IPN: Order #{OrderId} not found", madh);
                return Json(new { RspCode = "01", Message = "Order not found" });
            }

            // Kiểm tra trạng thái đơn hàng — nếu đã xử lý rồi thì không xử lý lại
            if (donHang.trangthai == "Đã đặt" || donHang.trangthai == "Đã xác nhận" || donHang.trangthai == "Hoàn thành")
            {
                _logger.LogInformation("VNPAY IPN: Order #{OrderId} already processed (status: {Status})", madh, donHang.trangthai);
                return Json(new { RspCode = "02", Message = "Order already confirmed" });
            }

            // Nếu đơn đã hủy do auto-cancel nhưng khách vẫn thanh toán → reactivate
            bool wasCancelled = donHang.trangthai == "Đã hủy";
            if (wasCancelled)
            {
                _logger.LogWarning("VNPAY IPN: Order #{OrderId} was CANCELLED but payment received — reactivating", madh);
            }

            // Xử lý kết quả thanh toán
            if (responseCode == "00")
            {
                // Thanh toán thành công
                donHang.trangthai = "Đã đặt";
                donHang.ngaythanhtoan = DateTime.Now;
                donHang.momo_trans_id = transactionNo; // Tái sử dụng cột momo_trans_id để lưu VNPAY transactionNo
                await db.SaveChangesAsync();

                _logger.LogInformation("VNPAY payment confirmed for order #{OrderId}, TransactionNo={TransactionNo}", madh, transactionNo);

                // Tạo e-invoice
                try { await _eDelivery.GenerateEInvoice(madh); }
                catch (Exception edEx) { _logger.LogWarning(edEx, "E-Invoice generation failed for order #{OrderId}", madh); }

                // SignalR: thông báo real-time
                try { await _hubContext.Clients.Group($"order_{madh}").SendAsync("paymentConfirmed", madh, donHang.tongtien); } catch { }
                try { await _hubContext.Clients.Group($"order_{madh}").SendAsync("orderStatusChanged", madh, "Đã đặt", DateTime.Now.ToString("HH:mm")); } catch { }

                if (donHang.maquan != null)
                {
                    try
                    {
                        await _hubContext.Clients.Group($"restaurant_{donHang.maquan}").SendAsync("newOrder", new
                        {
                            orderId = donHang.madh,
                            status = "Đã đặt",
                            time = DateTime.Now.ToString("HH:mm")
                        });
                    }
                    catch { }
                }

                return Json(new { RspCode = "00", Message = "Confirm Success" });
            }
            else
            {
                // Thanh toán thất bại
                _logger.LogWarning("VNPAY payment failed for order #{OrderId}: ResponseCode={ResponseCode}", madh, responseCode);

                if (!wasCancelled)
                {
                    donHang.trangthai = "Chờ thanh toán";
                    await db.SaveChangesAsync();
                }

                // SignalR: thông báo thất bại
                try { await _hubContext.Clients.Group($"order_{madh}").SendAsync("paymentFailed", madh, "Thanh toán VNPAY thất bại"); } catch { }

                return Json(new { RspCode = responseCode, Message = "Payment failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VNPAY IPN callback error");
            return Json(new { RspCode = "99", Message = "Internal error" });
        }
    }

    /// <summary>
    /// VNPAY Return URL — người dùng được redirect về đây sau khi thanh toán
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> VnpayReturn()
    {
        var vnpParams = new Dictionary<string, string>();
        foreach (var key in Request.Query.Keys)
        {
            if (!string.IsNullOrEmpty(key))
            {
                vnpParams[key] = Request.Query[key].ToString() ?? "";
            }
        }

        if (vnpParams.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        vnpParams.TryGetValue("vnp_ResponseCode", out var responseCode);
        vnpParams.TryGetValue("vnp_TxnRef", out var txnRef);

        // Xác thực chữ ký
        bool isValid = _vnpayService.VerifySignature(vnpParams);

        if (!isValid)
        {
            TempData["OrderError"] = "Chữ ký không hợp lệ. Vui lòng liên hệ hỗ trợ.";
            if (!string.IsNullOrEmpty(txnRef) && int.TryParse(txnRef, out var errId))
                return RedirectToAction("OrderTracking", "Cart", new { id = errId });
            return RedirectToAction("Index", "Cart");
        }

        if (responseCode == "00" && !string.IsNullOrEmpty(txnRef) && int.TryParse(txnRef, out var orderId))
        {
            TempData["OrderSuccess"] = $"Thanh toán VNPAY thành công! Mã đơn hàng: #{orderId}";
            return RedirectToAction("ChiTietDonHang", "Cart", new { id = orderId });
        }

        // Thanh toán thất bại
        var errorMsg = responseCode switch
        {
            "01" => "Giao dịch chưa được xác thực.",
            "02" => "Giao dịch bị từ chối.",
            "04" => "Giao dịch bị đóng băng.",
            "07" => "Trừ tiền tài khoản thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo).",
            "09" => "Giao dịch thất bại do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking.",
            "10" => "Giao dịch thất bại do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần.",
            "11" => "Giao dịch thất bại do: Đã hết hạn chờ thanh toán. Xin vui lòng thực hiện lại.",
            "12" => "Giao dịch thất bại do: Thẻ/Tài khoản của khách hàng bị khóa.",
            "13" => "Giao dịch thất bại do: Khách hàng nhập sai mật khẩu xác thực giao dịch (OTP).",
            "24" => "Giao dịch thất bại do: Khách hàng hủy giao dịch.",
            "51" => "Giao dịch thất bại do: Tài khoản của khách hàng không đủ số dư.",
            "65" => "Giao dịch thất bại do: Tài khoản của khách hàng đã vượt quá hạn mức giao dịch trong ngày.",
            "75" => "Ngân hàng thanh toán đang bảo trì.",
            "79" => "Giao dịch thất bại do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
            "99" => "Giao dịch thất bại do: Lỗi không xác định.",
            _ => $"Lỗi không xác định (Mã: {responseCode})"
        };

        TempData["OrderError"] = $"Thanh toán VNPAY thất bại: {errorMsg}";
        if (!string.IsNullOrEmpty(txnRef) && int.TryParse(txnRef, out var failId))
            return RedirectToAction("OrderTracking", "Cart", new { id = failId });
        return RedirectToAction("Index", "Cart");
    }

    // ═══════════════════════════════════════════════════════════════
    // VNPAY Wallet Deposit Return
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// VNPAY Return URL cho nạp tiền ví — xác thực và cộng tiền vào ví
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> VnpayWalletReturn()
    {
        var vnpParams = new Dictionary<string, string>();
        foreach (var key in Request.Query.Keys)
        {
            if (!string.IsNullOrEmpty(key))
            {
                vnpParams[key] = Request.Query[key].ToString() ?? "";
            }
        }

        if (vnpParams.Count == 0)
        {
            return RedirectToAction("Wallet", "Home");
        }

        vnpParams.TryGetValue("vnp_ResponseCode", out var responseCode);
        vnpParams.TryGetValue("vnp_TxnRef", out var txnRef);
        vnpParams.TryGetValue("vnp_Amount", out var amountStr);

        // Xác thực chữ ký
        bool isValid = _vnpayService.VerifySignature(vnpParams);

        if (!isValid)
        {
            _logger.LogWarning("VnpayWalletReturn: Invalid signature for TxnRef={TxnRef}", txnRef);
            TempData["WalletError"] = "Chữ ký không hợp lệ. Vui lòng liên hệ hỗ trợ.";
            return RedirectToAction("Wallet", "Home");
        }

        if (responseCode != "00")
        {
            var errorMsg = responseCode switch
            {
                "01" => "Giao dịch chưa được xác thực.",
                "02" => "Giao dịch bị từ chối.",
                "09" => "Thẻ chưa đăng ký InternetBanking.",
                "10" => "Xác thực thông tin sai quá 3 lần.",
                "11" => "Hết hạn chờ thanh toán.",
                "12" => "Thẻ/Tài khoản bị khóa.",
                "13" => "Sai mật khẩu OTP.",
                "24" => "Khách hàng hủy giao dịch.",
                "51" => "Tài khoản không đủ số dư.",
                "65" => "Vượt quá hạn mức giao dịch.",
                "75" => "Ngân hàng đang bảo trì.",
                _ => $"Lỗi không xác định (Mã: {responseCode})"
            };
            TempData["WalletError"] = $"Nạp tiền thất bại: {errorMsg}";
            _logger.LogWarning("VnpayWalletReturn: Payment failed for TxnRef={TxnRef}, Code={Code}", txnRef, responseCode);
            return RedirectToAction("Wallet", "Home");
        }

        // Thanh toán thành công — parse orderInfo để lấy userId
        vnpParams.TryGetValue("vnp_OrderInfo", out var orderInfo);

        try
        {
            int? userId = null;
            // Parse userId từ orderInfo: FASTSHIP_WALLET_NAP{userId}_{timestamp}
            if (!string.IsNullOrEmpty(orderInfo))
            {
                var match = System.Text.RegularExpressions.Regex.Match(orderInfo, @"FASTSHIP_WALLET_NAP([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    userId = int.Parse(match.Groups[1].Value);
                }
            }

            if (userId == null || userId <= 0)
            {
                _logger.LogWarning("VnpayWalletReturn: Could not parse userId from orderInfo={OrderInfo}", orderInfo);
                TempData["WalletError"] = "Không thể xác định tài khoản. Vui lòng liên hệ hỗ trợ.";
                return RedirectToAction("Wallet", "Home");
            }

            var user = await db.tbUsers.FindAsync(userId.Value);
            if (user == null)
            {
                _logger.LogWarning("VnpayWalletReturn: User #{UserId} not found", userId);
                TempData["WalletError"] = "Tài khoản không tồn tại.";
                return RedirectToAction("Wallet", "Home");
            }

            // Tính số tiền từ VNPAY (vnp_Amount gửi về * 100)
            long amountVnd = 0;
            if (!string.IsNullOrEmpty(amountStr) && long.TryParse(amountStr, out var vnpAmount))
            {
                amountVnd = vnpAmount / 100;
            }

            if (amountVnd <= 0)
            {
                _logger.LogWarning("VnpayWalletReturn: Invalid amount for user #{UserId}", userId);
                TempData["WalletError"] = "Số tiền không hợp lệ.";
                return RedirectToAction("Wallet", "Home");
            }

            // Cộng tiền vào ví
            user.vitien = (user.vitien ?? 0) + amountVnd;
            await db.SaveChangesAsync();

            // Ghi log deposit
            try
            {
                db.tbTinNhans.Add(new tbTinNhan
                {
                    noidung = $"VNPAY_DEPOSIT_SUCCESS|{txnRef}|{amountVnd}|{userId}",
                    makh = userId.Value
                });
                await db.SaveChangesAsync();
            }
            catch { }

            _logger.LogInformation("VnpayWalletReturn: Wallet deposit {Amount}đ for user #{UserId} via VNPAY (TxnRef={TxnRef})",
                amountVnd, userId, txnRef);

            TempData["WalletSuccess"] = $"Nạp tiền thành công! {amountVnd:N0}đ đã được cộng vào ví.";
            return RedirectToAction("Wallet", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VnpayWalletReturn: Error processing deposit for TxnRef={TxnRef}", txnRef);
            TempData["WalletError"] = "Lỗi xử lý nạp tiền. Vui lòng liên hệ hỗ trợ.";
            return RedirectToAction("Wallet", "Home");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Bank Webhook (giữ lại cho backward compatibility)
    // ═══════════════════════════════════════════════════════════════

    [HttpPost]
    [AllowAnonymous]
    [Route("Payment/BankWebhook")]
    public async Task<JsonResult> BankWebhook()
    {
        try
        {
            if (string.IsNullOrEmpty(BankWebhookToken))
            {
                _logger.LogError("BankWebhook: BANK_WEBHOOK_TOKEN not configured");
                return Json(new { error = "Server not configured" });
            }

            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? "";
            var token = authHeader.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token) || token.Length != BankWebhookToken.Length)
            {
                _logger.LogWarning("BankWebhook: Invalid token received (length mismatch)");
                return Json(new { error = "Unauthorized" });
            }

            if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(token),
                System.Text.Encoding.UTF8.GetBytes(BankWebhookToken)))
            {
                _logger.LogWarning("BankWebhook: Token mismatch (FixedTimeEquals)");
                return Json(new { error = "Unauthorized" });
            }

            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body);

            string? memo = null;
            long? amount = null;

            // Casso format
            if (json.TryGetProperty("data", out var data) && data.ValueKind == System.Text.Json.JsonValueKind.Array && data.GetArrayLength() > 0)
            {
                var first = data[0];
                if (first.TryGetProperty("description", out var desc)) memo = desc.GetString();
                if (first.TryGetProperty("amount", out var amt)) amount = (long)amt.GetDecimal();
            }

            // SePay format
            if (memo == null && json.TryGetProperty("content", out var sepayContent)) memo = sepayContent.GetString();
            if (memo == null && json.TryGetProperty("transferDesc", out var td)) memo = td.GetString();
            if (amount == null && json.TryGetProperty("transferAmount", out var ta)) amount = (long)ta.GetDecimal();
            if (json.TryGetProperty("gateway", out var gateway))
                _logger.LogInformation("BankWebhook: Gateway={Gateway}", gateway.GetString());

            // PayOS format
            if (amount == null && json.TryGetProperty("amount", out var payosAmt)) amount = (long)payosAmt.GetDecimal();
            if (memo == null && json.TryGetProperty("description", out var pd)) memo = pd.GetString();

            if (string.IsNullOrEmpty(memo))
            {
                _logger.LogWarning("BankWebhook: No memo found");
                return Json(new { error = "No memo" });
            }

            var orderMatch = System.Text.RegularExpressions.Regex.Match(memo, @"FASTSHIP(\d+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var depositMatch = System.Text.RegularExpressions.Regex.Match(memo, @"FASTSHIPNAP(\d+)_(\d+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (depositMatch.Success)
            {
                if (!int.TryParse(depositMatch.Groups[1].Value, out var depositUserId))
                    return Json(new { error = "Invalid deposit memo" });

                var user = await db.tbUsers.FindAsync(depositUserId);
                if (user == null) return Json(new { error = "User not found" });

                if (amount == null || amount.Value <= 0)
                    return Json(new { error = "Invalid amount" });

                user.vitien = (user.vitien ?? 0) + (decimal)amount.Value;
                await db.SaveChangesAsync();

                try
                {
                    db.tbTinNhans.Add(new tbTinNhan
                    {
                        noidung = $"DEPOSIT_SUCCESS|{memo}|{amount}|{depositUserId}",
                        makh = depositUserId
                    });
                    await db.SaveChangesAsync();
                }
                catch { }

                _logger.LogInformation("BankWebhook: Wallet deposit {Amount}đ for user #{UserId}", amount, depositUserId);
                try { await _hubContext.Clients.Group($"customer_{depositUserId}").SendAsync("walletDeposit", amount, user.vitien); } catch { }

                return Json(new { error = 0, message = "Deposit approved" });
            }
            else if (orderMatch.Success)
            {
                if (!int.TryParse(orderMatch.Groups[1].Value, out var madh))
                    return Json(new { error = "Invalid order memo" });

                var donHang = await db.tbDonHangs.FindAsync(madh);
                if (donHang == null) return Json(new { error = "Order not found" });

                // Skip nếu đã xử lý rồi
                if (donHang.trangthai == "Đã đặt" || donHang.trangthai == "Đã xác nhận" || donHang.trangthai == "Đã thanh toán" || donHang.trangthai == "Hoàn thành")
                {
                    _logger.LogInformation("BankWebhook: Order #{OrderId} already processed (status: {Status})", madh, donHang.trangthai);
                    return Json(new { error = 0, message = "Already processed" });
                }

                bool wasCancelled = donHang.trangthai == "Đã hủy";
                if (wasCancelled)
                {
                    _logger.LogWarning("BankWebhook: Order #{OrderId} was CANCELLED but payment received — reactivating", madh);
                }

                if (amount == null || Math.Abs(Convert.ToDecimal(amount.Value) - (donHang.tongtien ?? 0)) > 1000)
                {
                    _logger.LogWarning("BankWebhook: Amount mismatch for order #{OrderId}. Expected: {Expected}, Received: {Received}",
                        madh, donHang.tongtien, amount);
                    return Json(new { error = "Amount mismatch" });
                }

                donHang.trangthai = "Đã đặt";
                donHang.ngaythanhtoan = DateTime.Now;
                await db.SaveChangesAsync();

                try { await _eDelivery.GenerateEInvoice(madh); }
                catch (Exception edEx) { _logger.LogWarning(edEx, "E-Invoice generation failed for order #{OrderId}", madh); }

                _logger.LogInformation("BankWebhook: Order #{OrderId} approved via bank transfer (was cancelled: {WasCancelled})", madh, wasCancelled);

                try { await _hubContext.Clients.Group($"order_{madh}").SendAsync("paymentConfirmed", madh, donHang.tongtien); } catch { }
                try { await _hubContext.Clients.Group($"order_{madh}").SendAsync("orderStatusChanged", madh, "Đã đặt", DateTime.Now.ToString("HH:mm")); } catch { }

                if (donHang.maquan != null)
                {
                    try
                    {
                        await _hubContext.Clients.Group($"restaurant_{donHang.maquan}").SendAsync("newOrder", new
                        {
                            orderId = donHang.madh,
                            status = "Đã đặt",
                            time = DateTime.Now.ToString("HH:mm")
                        });
                    }
                    catch { }
                }

                return Json(new { error = 0, message = "Order approved" });
            }
            else
            {
                _logger.LogWarning("BankWebhook: Cannot parse any valid code from memo: {Memo}", memo);
                return Json(new { error = "Invalid memo format" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BankWebhook processing error");
            return Json(new { error = -1, message = ex.Message });
        }
    }

    // ─── Verify Bank Transaction ───
    [HttpGet]
    public async Task<JsonResult> VerifyBankTransaction(int madh)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        try
        {
            var donHang = await db.tbDonHangs.FindAsync(madh);
            if (donHang == null)
                return Json(new { success = false, message = "Đơn hàng không tồn tại" });

            var user = GetCurrentUser();
            var ttdh = await db.tbThongTinDatHangs.FindAsync(donHang.mattdh);
            if (ttdh?.userid != user?.userid)
                return Json(new { success = false, message = "Không có quyền kiểm tra đơn hàng này" });

            bool daThanhToan = donHang.trangthai == "Đã đặt" || donHang.trangthai == "Đã xác nhận" || donHang.trangthai == "Đã thanh toán" || donHang.trangthai == "Hoàn thành";
            if (daThanhToan)
            {
                return Json(new { success = true, message = $"✅ Đơn hàng #{madh} đã được thanh toán! (trạng thái: {donHang.trangthai})" });
            }

            if (donHang.trangthai == "Đã hủy")
            {
                return Json(new { success = false, message = $"❌ Đơn hàng #{madh} đã bị hủy do quá thời gian thanh toán. Vui lòng liên hệ admin nếu bạn đã chuyển khoản." });
            }

            return Json(new { success = false, message = "🕐 Hệ thống chưa nhận được xác nhận chuyển khoản. Vui lòng kiểm tra lại sau 1-2 phút." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VerifyBankTransaction failed for order #{OrderId}", madh);
            return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<JsonResult> CheckPaymentStatus(int madh)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        try
        {
            var donHang = await db.tbDonHangs.FindAsync(madh);
            if (donHang == null)
                return Json(new { success = false, message = "Đơn hàng không tồn tại" });

            var user = GetCurrentUser();
            var ttdh = await db.tbThongTinDatHangs.FindAsync(donHang.mattdh);
            if (ttdh?.userid != user?.userid)
                return Json(new { success = false, message = "Không có quyền kiểm tra đơn hàng này" });

            bool daThanhToan = donHang.trangthai == "Đã đặt" || donHang.trangthai == "Đã xác nhận" || donHang.trangthai == "Đã thanh toán"
                || donHang.trangthai == "Hoàn thành";
            return Json(new
            {
                success = daThanhToan,
                trangthai = donHang.trangthai,
                message = daThanhToan ? $"Đơn hàng đã được xác nhận (trạng thái: {donHang.trangthai})" : "Đơn hàng đang chờ thanh toán"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckPaymentStatus failed for order #{OrderId}", madh);
            return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
        }
    }

    [HttpGet]
    public JsonResult CheckOrderStatus(int orderId)
    {
        var roleCheck = CheckRoleJson("Khách hàng");
        if (roleCheck != null) return roleCheck;

        var order = db.tbDonHang.Find(orderId);
        if (order == null)
            return Json(new { success = false, message = "Đơn hàng không tồn tại" });

        var paid = order.trangthai.Equals("Đã thanh toán", StringComparison.OrdinalIgnoreCase) ||
                   order.trangthai.Equals("Đã đặt", StringComparison.OrdinalIgnoreCase);

        return Json(new {
            success = true,
            status  = paid ? "Paid" : order.trangthai,
            redirectUrl = paid ? Url.Action("ChiTietDonHang", "Cart", new { id = order.madh }) : null
        });
    }
}
