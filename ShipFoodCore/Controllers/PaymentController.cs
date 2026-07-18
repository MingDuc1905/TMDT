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
    private readonly MoMoService _moMoService;
    private readonly PayPalService _payPalService;
    private readonly IConfiguration _configuration;
    private readonly EDeliveryService _eDelivery;

    public PaymentController(dbFoodyEntities context, ILogger<PaymentController> logger, IHubContext<Chats> hubContext,
        MoMoService moMoService, PayPalService payPalService, IConfiguration configuration, EDeliveryService eDelivery)
    {
        db = context;
        _logger = logger;
        _hubContext = hubContext;
        _moMoService = moMoService;
        _payPalService = payPalService;
        _configuration = configuration;
        _eDelivery = eDelivery;
    }

    // ─── Bank transfer config (đọc từ env vars) ───
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

            // ═══ 1 PENDING ORDER PER USER: Không cho tạo đơn mới nếu đang có đơn "Chờ thanh toán" ═══
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

            // ═══ IDEMPOTENCY LOCK: Chống tạo đơn trùng trong 30 giây ═══
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

            // ═══ MULTI-DEVICE CHECK: Phát hiện giỏ hàng đã được xử lý trên thiết bị khác ═══
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

            // ═══ 3a: BẮT BUỘC re-read giá từ DB ═══
            decimal tongTienMon = 0;
            foreach (var item in cart.items)
            {
                var bt = db.tbBienTheMonAn.Find(item.mabienthe);
                if (bt?.giatien == null)
                {
                    return Json(new { success = false, message = $"Món '{item.tenmon}' không còn tồn tại hoặc đã thay đổi giá. Vui lòng tải lại giỏ hàng." });
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

            decimal tongCong = Math.Max(tongTienMon + phiShip - discountAmount, 0);

            bool isBankTransfer = IsBankTransferMethod(pttt) || pttt == 5 || IsPayPalMethod(pttt);
            bool isPayPal = IsPayPalMethod(pttt);
            var trangThaiBanDau = isBankTransfer ? "Chờ thanh toán" : "Đã đặt";

            var createdOrders = new List<int>();
            string? momoPayUrl = null;
            bool momoSuccess = false;
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
                _logger.LogInformation("Order #{OrderId} (Restaurant #{ResId}) placed by user {UserId}", dh.madh, resId, user!.userid);

                dh.phiship = resShipFee;

                if (!isBankTransfer && trangThaiBanDau != "Chờ thanh toán")
                {
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
            }

            // ─── MoMo Payment ───
            if (pttt == 5 && momoPayUrl == null && createdOrders.Any())
            {
                var firstDh = createdOrders.First();
                try
                {
                    int momoAmount = (int)totalAllOrders;
                    if (momoAmount <= 0) momoAmount = 1000;

                    var momoRequest = new MoMoCreatePaymentRequest
                    {
                        OrderId = $"FS{firstDh}_{DateTime.Now:yyyyMMddHHmmss}",
                        OrderInfo = $"Thanh toan don hang FastShip #{firstDh}",
                        Amount = momoAmount,
                        RedirectUrl = $"{Request.Scheme}://{Request.Host}/Cart/SuccessView?orderId={firstDh}",
                        IpnUrl = $"{Request.Scheme}://{Request.Host}/Payment/MoMoIpn",
                        RequestType = "captureWallet",
                        ExtraData = $"{firstDh}"
                    };

                    var momoResult = await _moMoService.CreatePaymentAsync(momoRequest);
                    if (momoResult.IsSuccess)
                    {
                        momoPayUrl = momoResult.PayUrl;
                        momoSuccess = true;
                        _logger.LogInformation("MoMo payment URL created for order #{OrderId}: {PayUrl}, amount={Amount}", firstDh, momoPayUrl, momoAmount);
                    }
                    else
                    {
                        _logger.LogWarning("MoMo payment failed for order #{OrderId}: {Message}", firstDh, momoResult.Message);
                    }
                }
                catch (Exception momoEx)
                {
                    _logger.LogError(momoEx, "MoMo payment creation failed for order #{OrderId}", firstDh);
                }

                // ponytail: Fix #1 — MoMo fail => xóa đơn, trả error
                if (!momoSuccess && createdOrders.Any())
                {
                    _logger.LogWarning("MoMo creation FAILED — deleting {Count} orders", createdOrders.Count);
                    try
                    {
                        var ordersToDelete = db.tbDonHang.Where(o => createdOrders.Contains(o.madh)).ToList();
                        var detailIds = db.tbChiTietDonHang.Where(c => createdOrders.Contains((int)c.madh)).ToList();
                        db.tbChiTietDonHang.RemoveRange(detailIds);
                        db.tbDonHang.RemoveRange(ordersToDelete);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception delEx) { _logger.LogError(delEx, "Failed to delete orders after MoMo failure"); }

                    return Json(new { success = false, message = "Không thể tạo thanh toán MoMo. Vui lòng thử lại sau.", keepCart = true });
                }
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

            // ponytail: Fix #4 — xoa cart cho ca bank transfer, MoMo, PayPal success
            if (!isBankTransfer && pttt != 5)
            {
                SetCart(new Cart());
            }
            else if (momoSuccess)
            {
                SetCart(new Cart());
            }
            else if (isBankTransfer)
            {
                // ponytail: Fix #4 — don da tao trong DB => xoa cart, user theo doi qua OrderTracking
                SetCart(new Cart());
            }

            // ─── PayPal ───
            if (isPayPal && createdOrders.Any())
            {
                var ppOrderId = createdOrders.First();
                try
                {
                    var returnUrl = $"{Request.Scheme}://{Request.Host}/Payment/CapturePayPalOrder?orderId={ppOrderId}";
                    var cancelUrl = $"{Request.Scheme}://{Request.Host}/Cart/OrderTracking?id={ppOrderId}";

                    var ppResult = await _payPalService.CreateOrderAsync(ppOrderId.ToString(), totalAllOrders, returnUrl, cancelUrl);
                    if (ppResult.Success)
                    {
                        HttpContext.Session.SetString($"paypal_order_{ppOrderId}", ppResult.PayPalOrderId ?? "");

                        _logger.LogInformation("PayPal order created: OrderId={OrderId}, PayPalOrderId={PayPalOrderId}",
                            ppOrderId, ppResult.PayPalOrderId);

                        return Json(new
                        {
                            success = true,
                            message = $"Đã tạo {createdOrders.Count} đơn hàng! Chuyển hướng đến PayPal...",
                            orderId = ppOrderId,
                            orderIds = createdOrders,
                            trangthai = "Chờ thanh toán",
                            paypalApprovalUrl = ppResult.ApproveLink
                        });
                    }
                    else
                    {
                        _logger.LogWarning("PayPal CreateOrder FAILED for order #{OrderId}: {Message}", ppOrderId, ppResult.Message);

                        // ponytail: Fix #1 — PayPal fail => xóa đơn
                        try
                        {
                            var ordersToDelete = db.tbDonHang.Where(o => createdOrders.Contains(o.madh)).ToList();
                            var detailIds = db.tbChiTietDonHang.Where(c => createdOrders.Contains((int)c.madh)).ToList();
                            db.tbChiTietDonHang.RemoveRange(detailIds);
                            db.tbDonHang.RemoveRange(ordersToDelete);
                            await db.SaveChangesAsync();
                        }
                        catch (Exception delEx) { _logger.LogError(delEx, "Failed to delete orders after PayPal failure"); }

                        return Json(new { success = false, message = "Không thể tạo thanh toán PayPal. Vui lòng thử lại sau.", keepCart = true });
                    }
                }
                catch (Exception ppEx)
                {
                    _logger.LogError(ppEx, "PayPal CreateOrder error for order #{OrderId}", ppOrderId);

                    // Delete orders on PayPal exception
                    try
                    {
                        var ordersToDelete = db.tbDonHang.Where(o => createdOrders.Contains(o.madh)).ToList();
                        var detailIds = db.tbChiTietDonHang.Where(c => createdOrders.Contains((int)c.madh)).ToList();
                        db.tbChiTietDonHang.RemoveRange(detailIds);
                        db.tbDonHang.RemoveRange(ordersToDelete);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception delEx) { _logger.LogError(delEx, "Failed to delete orders after PayPal exception"); }

                    return Json(new { success = false, message = "Lỗi kết nối PayPal. Vui lòng thử lại sau.", keepCart = true });
                }
            }

            // ─── Bank Transfer ───
            if (isBankTransfer && !isPayPal)
            {
                var memo = $"SEVQR FASTSHIP{firstOrderId}";
                var qrUrl = $"https://img.vietqr.io/image/{BankVietQrBinCode}-{BankAccountNo}-print.png?amount={(long)totalAllOrders}&addInfo={Uri.EscapeDataString(memo)}&accountName={Uri.EscapeDataString(BankAccountName)}";

                _logger.LogInformation("Bank transfer QR URL generated. Orders: {OrderIds}", string.Join(",", createdOrders));

                return Json(new
                {
                    success = true,
                    message = $"Đã tạo {createdOrders.Count} đơn hàng! Vui lòng quét mã QR để chuyển khoản.",
                    orderId = firstOrderId,
                    orderIds = createdOrders,
                    trangthai = "Chờ thanh toán",
                    paymentMethod = "bank",
                    qrCodeUrl = qrUrl,
                    bankInfo = new
                    {
                        bankId = BankId,
                        accountNo = BankAccountNo,
                        accountName = BankAccountName,
                        amount = (long)totalAllOrders,
                        memo = memo
                    }
                });
            }

            _logger.LogInformation("Orders created successfully: {OrderIds}", string.Join(",", createdOrders));

            return Json(new
            {
                success  = true,
                message  = $"Đặt hàng thành công! Đã tạo {createdOrders.Count} đơn hàng.",
                orderId  = firstOrderId,
                orderIds = createdOrders,
                trangthai = "Đang xử lý",
                momoPayUrl = momoPayUrl,
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

    // ─── MoMo IPN Callback ───
    [HttpPost]
    [AllowAnonymous]
    public async Task<JsonResult> MoMoIpn()
    {
        try
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var ipnParams = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(body);

            if (ipnParams == null)
                return Json(new { error = "Invalid IPN data" });

            if (!_moMoService.VerifyIpnSignature(ipnParams))
            {
                _logger.LogWarning("MoMo IPN signature verification failed");
                return Json(new { error = "Invalid signature" });
            }

            var resultCode = int.TryParse(ipnParams.GetValueOrDefault("resultCode"), out var code) ? code : -1;
            var orderId = ipnParams.GetValueOrDefault("orderId") ?? "";
            var transId = ipnParams.GetValueOrDefault("transId");

            _logger.LogInformation("MoMo IPN received: OrderId={OrderId}, ResultCode={ResultCode}, TransId={TransId}",
                orderId, resultCode, transId);

            if (orderId.StartsWith("FS"))
            {
                var parts = orderId.Split('_');
                if (parts.Length >= 2 && int.TryParse(parts[0].Substring(2), out var madh))
                {
                    var donHang = await db.tbDonHangs.FindAsync(madh);
                    if (donHang != null)
                    {
                        if (resultCode == 0)
                        {
                            donHang.trangthai = "Đã thanh toán";
                            donHang.ngaythanhtoan = DateTime.Now;
                            donHang.momo_trans_id = transId;
                            _logger.LogInformation("MoMo payment confirmed for order #{OrderId}, TransId={TransId}", madh, transId);

                            try { await _eDelivery.GenerateEInvoice(madh); }
                            catch (Exception edEx) { _logger.LogWarning(edEx, "E-Invoice generation failed for order #{OrderId}", madh); }

                            try { await _hubContext.Clients.Group($"order_{madh}").SendAsync("paymentConfirmed", madh, donHang.tongtien); } catch { }
                        }
                        else
                        {
                            _logger.LogWarning("MoMo payment failed for order #{OrderId}: ResultCode={ResultCode}", madh, resultCode);
                            donHang.trangthai = "Chờ thanh toán";
                            try { await _hubContext.Clients.Group($"order_{madh}").SendAsync("paymentFailed", madh, ipnParams.GetValueOrDefault("message", "Thanh toán thất bại")); } catch { }
                        }
                        await db.SaveChangesAsync();
                    }
                }
            }

            return Json(new { error = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MoMo IPN callback error");
            return Json(new { error = -1, message = ex.Message });
        }
    }

    private bool IsPayPalMethod(int pttt)
    {
        try
        {
            var method = db.tbLoaiHinhThanhToan.Find(pttt);
            if (method == null) return false;
            var name = (method.tenhinhthuc ?? "").ToLowerInvariant();
            return name.Contains("paypal");
        }
        catch { return false; }
    }

    private bool IsBankTransferMethod(int pttt)
    {
        try
        {
            var method = db.tbLoaiHinhThanhToan.Find(pttt);
            if (method == null) return false;
            var name = RemoveDiacritics((method.tenhinhthuc ?? "").ToLowerInvariant());
            return name.Contains("chuyen khoan") || name.Contains("ngan hang") || name.Contains("bank");
        }
        catch { return false; }
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(System.Text.NormalizationForm.FormC);
    }

    // ─── BANK WEBHOOK: Casso/SePay/PayOS — xác nhận chuyển khoản ───
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

                // ponytail: Fix #5 — N?u don da hoan tat => skip (kh?i trùng l?p)
                if (donHang.trangthai == "Đã đặt" || donHang.trangthai == "Đã xác nhận" || donHang.trangthai == "Đã thanh toán" || donHang.trangthai == "Hoàn thành")
                {
                    _logger.LogInformation("BankWebhook: Order #{OrderId} already processed (status: {Status})", madh, donHang.trangthai);
                    return Json(new { error = 0, message = "Already processed" });
                }

                // ponytail: Fix #5 — N?u don da bi auto-cancel nhung khach da chuy?n kho?n => kích ho?t l?i
                bool wasCancelled = donHang.trangthai == "Đã hủy";
                if (wasCancelled)
                {
                    _logger.LogWarning("BankWebhook: Order #{OrderId} was CANCELLED but payment received — reactivating", madh);
                }

                // Kiểm tra số tiền (cho phép sai số ±1000đ do phí ngân hàng)
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

                string logMsg = wasCancelled
                    ? $"BankWebhook: Order #{madh} REACTIVATED after auto-cancel — payment received"
                    : $"BankWebhook: Order #{madh} approved normally";
                _logger.LogInformation(logMsg);

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

    // ─── Verify Bank Transaction: User nhấn "Tôi đã chuyển khoản" — kiểm tra trạng thái ───
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

            // ponytail: ownership check — user ch? xem ???c don c?a mình
            var user = GetCurrentUser();
            var ttdh = await db.tbThongTinDatHangs.FindAsync(donHang.mattdh);
            if (ttdh?.userid != user?.userid)
                return Json(new { success = false, message = "Không có quyền kiểm tra đơn hàng này" });

            // ponytail: Fix #2 — Ki?m tra tr?ng thái th?c t? t? DB
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
    public IActionResult MoMoReturn(int? orderId)
    {
        if (orderId.HasValue)
        {
            TempData["OrderSuccess"] = $"Thanh toán MoMo thành công! Mã đơn hàng: #{orderId}";
            return RedirectToAction("ChiTietDonHang", "Cart", new { id = orderId });
        }
        return RedirectToAction("Index", "Cart");
    }

    // ═══════════════════════════════════════════════════════════════
    // PayPal Payment Integration
    // ═══════════════════════════════════════════════════════════════

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> CreatePayPalOrder(int orderId)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        try
        {
            var donHang = await db.tbDonHangs.FindAsync(orderId);
            if (donHang == null)
                return Json(new { success = false, message = "Đơn hàng không tồn tại" });

            if (donHang.trangthai != "Chờ thanh toán")
                return Json(new { success = false, message = $"Đơn hàng ở trạng thái '{donHang.trangthai}', không thể thanh toán PayPal" });

            var returnUrl = $"{Request.Scheme}://{Request.Host}/Payment/CapturePayPalOrder?orderId={orderId}";
            var cancelUrl = $"{Request.Scheme}://{Request.Host}/Cart/OrderTracking?id={orderId}";

            var result = await _payPalService.CreateOrderAsync(orderId.ToString(), donHang.tongtien ?? 0, returnUrl, cancelUrl);

            if (result.Success)
            {
                HttpContext.Session.SetString($"paypal_order_{orderId}", result.PayPalOrderId ?? "");
                _logger.LogInformation("PayPal order created: OrderId={OrderId}, PayPalOrderId={PayPalOrderId}, Amount={Amount}USD",
                    orderId, result.PayPalOrderId, Math.Round((donHang.tongtien ?? 0) / 25000m, 2));

                return Json(new { success = true, approveLink = result.ApproveLink, paypalOrderId = result.PayPalOrderId, message = "Chuyển hướng đến PayPal..." });
            }

            _logger.LogWarning("PayPal CreateOrder failed for OrderId={OrderId}: {Message}", orderId, result.Message);
            return Json(new { success = false, message = result.Message ?? "Không thể tạo thanh toán PayPal" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePayPalOrder error for OrderId={OrderId}", orderId);
            return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> CapturePayPalOrder(int orderId, string? token = null)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        try
        {
            var donHang = await db.tbDonHangs.FindAsync(orderId);
            if (donHang == null)
            {
                TempData["OrderError"] = "Đơn hàng không tồn tại";
                return RedirectToAction("OrderTracking", "Cart", new { id = orderId });
            }

            var paypalOrderId = HttpContext.Session.GetString($"paypal_order_{orderId}") ?? token ?? "";
            if (string.IsNullOrEmpty(paypalOrderId))
            {
                TempData["OrderError"] = "Không tìm thấy giao dịch PayPal";
                return RedirectToAction("OrderTracking", "Cart", new { id = orderId });
            }

            var result = await _payPalService.CaptureOrderAsync(paypalOrderId);

            if (result.Success)
            {
                donHang.trangthai = "Đã đặt";
                donHang.ngaythanhtoan = DateTime.Now;
                donHang.momo_trans_id = result.CaptureId;
                await db.SaveChangesAsync();
                HttpContext.Session.Remove($"paypal_order_{orderId}");

                _logger.LogInformation("PayPal capture success: OrderId={OrderId}, CaptureId={CaptureId}", orderId, result.CaptureId);

                try { await _hubContext.Clients.Group($"order_{orderId}").SendAsync("paymentConfirmed", orderId, donHang.tongtien); } catch { }
                try { await _hubContext.Clients.Group($"order_{orderId}").SendAsync("orderStatusChanged", orderId, "Đã đặt", DateTime.Now.ToString("HH:mm")); } catch { }
                if (donHang.maquan != null)
                {
                    try { await _hubContext.Clients.Group($"restaurant_{donHang.maquan}").SendAsync("newOrder", new { orderId = donHang.madh, status = "Đã đặt", time = DateTime.Now.ToString("HH:mm") }); } catch { }
                }

                TempData["OrderSuccess"] = $"Thanh toán PayPal thành công! Mã đơn hàng: #{orderId}";
                return RedirectToAction("OrderTracking", "Cart", new { id = orderId });
            }

            _logger.LogWarning("PayPal capture failed: OrderId={OrderId}, Status={Status}", orderId, result.Status);
            TempData["OrderError"] = result.Message ?? "Thanh toán PayPal thất bại";
            return RedirectToAction("OrderTracking", "Cart", new { id = orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CapturePayPalOrder error for OrderId={OrderId}", orderId);
            TempData["OrderError"] = $"Lỗi: {ex.Message}";
            return RedirectToAction("OrderTracking", "Cart", new { id = orderId });
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
