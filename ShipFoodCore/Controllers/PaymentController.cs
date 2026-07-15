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
            // Nếu session cart tồn tại nhưng DB đã có đơn hàng mới của user này trong 5 phút,
            // chứng tỏ thiết bị khác đã đặt hàng — không cho phép tạo thêm đơn từ session cart cũ
            var recentMultiDeviceOrder = db.tbDonHang
                .Where(dh => dh.tbThongTinDatHang != null
                    && dh.tbThongTinDatHang.userid == user!.userid
                    && dh.ngaydathang >= DateTime.Now.AddMinutes(-5))
                .OrderByDescending(dh => dh.ngaydathang)
                .FirstOrDefault();
            if (recentMultiDeviceOrder != null && recentMultiDeviceOrder.madh > 0)
            {
                // Kiểm tra nếu session cart vẫn tồn tại (thiết bị cũ chưa reload)
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
            // Không tin tưởng giatien từ Frontend/localStorage, phải truy vấn giá mới nhất từ tbBienTheMonAn
            decimal tongTienMon = 0;
            foreach (var item in cart.items)
            {
                var bt = db.tbBienTheMonAn.Find(item.mabienthe);
                if (bt?.giatien == null)
                {
                    return Json(new { success = false, message = $"Món '{item.tenmon}' không còn tồn tại hoặc đã thay đổi giá. Vui lòng tải lại giỏ hàng." });
                }
                // Ghi đè giá từ DB, không dùng giá frontend gửi lên
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
                    // ponytail: fix P9 — voucher lo?i "Mi?n phí ship" set phiShip = 0
                    // Ch? match exact loaikm, tránh false positive v?i 'ship' trong tên khác
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

            // ponytail: Fix Item 3 — MoMo (pttt==5) va PayPal cung la async payment => "Chờ thanh toán"
            bool isBankTransfer = IsBankTransferMethod(pttt) || pttt == 5 || IsPayPalMethod(pttt);
            bool isPayPal = IsPayPalMethod(pttt);
            var trangThaiBanDau = isBankTransfer ? "Chờ thanh toán" : "Đã đặt";

            // ═══ MULTI-RESTAURANT: Tạo đơn riêng cho từng quán ═══
            // Nếu chỉ có 1 quán, tạo 1 đơn như bình thường
            // Nếu nhiều quán, tạo N đơn riêng biệt, gộp chung vào 1 địa chỉ giao hàng
            var createdOrders = new List<int>();
            string? momoPayUrl = null;
            bool momoSuccess = false;
            decimal totalAllOrders = 0;

            // ponytail: Fix Item 5 — ship fee chi tinh 1 lan cho tong don, ko tinh N lan
            // Ship fee duoc cong vao don dau tien, cac don con lai co ship fee = 0
            bool shipFeeApplied = false;

            foreach (var resId in restaurantIds)
            {
                var resItems = cart.items.Where(i => i.maquanan == resId).ToList();
                if (resItems.Count == 0) continue;

                // Tính tiền cho từng quán riêng
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

                // Chỉ áp dụng discount + ship fee cho đơn đầu tiên
                decimal resDiscount = (resId == restaurantIds.First()) ? discountAmount : 0;
                // ponytail: Fix Item 5 — ship fee chi 1 lan, amount = tongCong dung totalAllOrders cho MoMo
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

                // ponytail: Fix Item 5 — dung resShipFee cho phiship field, ko dung phiShip goc
                dh.phiship = resShipFee;

                // ponytail: CH? broadcast newOrder cho restaurant n?u don da thanh toan (ko ph?i bank transfer)
                // Bank transfer: restaurant ch? nhan thong bao SAU KHI webhook xac nhan (trong BankWebhook)
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
            } // ← end foreach

            // ponytail: Fix Item 1 — MoMo payment SAU vong lap de dung totalAllOrders (da tinh du)
            // ponytail: Fix Item 18 — bo *1000, totalAllOrders da la VND, ko nhan them 1000
            if (pttt == 5 && momoPayUrl == null && createdOrders.Any())
            {
                var firstDh = createdOrders.First();
                try
                {
                    int momoAmount = (int)totalAllOrders;
                    if (momoAmount <= 0) momoAmount = 1000; // fallback
                    
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
            }

            // ─── Ghi nhận lịch sử sử dụng mã giảm giá ───
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
                catch (Exception couponEx)
                {
                    _logger.LogWarning(couponEx, "Failed to record coupon usage for order #{OrderId}", createdOrders.FirstOrDefault());
                }
            }

            var firstOrderId = createdOrders.FirstOrDefault();

            // ponytail: Fix Item 2 — xoa cart CHI KHI payment success (bank transfer hoac cash)
            // Khong xoa cart cho MoMo (async, chua chac da thanh toan)
            if (!isBankTransfer && pttt != 5)
            {
                SetCart(new Cart());
            }
            else if (momoSuccess)
            {
                // MoMo da tao payment URL — cung xoa cart (user duoc redirect sang MoMo)
                SetCart(new Cart());
            }

            // ─── PayPal: trả về approval link ───
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

                        _logger.LogInformation("PayPal order created via ProcessPayment: OrderId={OrderId}, PayPalOrderId={PayPalOrderId}",
                            ppOrderId, ppResult.PayPalOrderId);

                        // Khong xoa cart — PayPal la async, user can chua thanh toan

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
                        _logger.LogWarning("PayPal CreateOrder failed for order #{OrderId} from ProcessPayment: {Message}", ppOrderId, ppResult.Message);
                        // Fall through: tra ve success nhung ko co paypal URL (user co the thanh toan sau tu OrderTracking)
                    }
                }
                catch (Exception ppEx)
                {
                    _logger.LogError(ppEx, "PayPal CreateOrder error for order #{OrderId} from ProcessPayment", ppOrderId);
                }
            }

            // ─── Bank Transfer: trả về QR URL ───
            if (isBankTransfer && !isPayPal)
            {
                // ponytail: SePay format — "SEVQR FASTSHIP{OrderId}" (SePay yêu cầu prefix "SEVQR ")
                var memo = $"SEVQR FASTSHIP{firstOrderId}";
                var qrUrl = $"https://img.vietqr.io/image/{BankVietQrBinCode}-{BankAccountNo}-compact2.png?amount={(long)totalAllOrders}&addInfo={Uri.EscapeDataString(memo)}&accountName={Uri.EscapeDataString(BankAccountName)}";

                _logger.LogInformation("Bank transfer QR URL generated. Orders: {OrderIds}", string.Join(",", createdOrders));

                return Json(new
                {
                    success = true,
                    message = $"Đã tạo {createdOrders.Count} đơn hàng! Vui lòng quét mã QR để chuyển khoản.",
                    orderId = firstOrderId,
                    orderIds = createdOrders,
                    trangthai = "Chờ thanh toán",
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

            // Phân loại lỗi chi tiết — LUÔN log đầy đủ inner exception để debug
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

    // ─── MoMo IPN Callback (MoMo gọi khi có kết quả thanh toán) ───
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

            // Xác thực signature
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

            // Parse mã đơn hàng từ orderId (FS{madh}_...)
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
                            // Thanh toán thành công
                            donHang.trangthai = "Đã thanh toán";
                            donHang.ngaythanhtoan = DateTime.Now;
                            donHang.momo_trans_id = transId; // Lưu mã giao dịch MoMo để dùng cho Refund
                            _logger.LogInformation("MoMo payment confirmed for order #{OrderId}, TransId={TransId}", madh, transId);

                            // ─── E-Delivery: Auto-sinh E-Invoice ───
                            try { await _eDelivery.GenerateEInvoice(madh); }
                            catch (Exception edEx) { _logger.LogWarning(edEx, "E-Invoice generation failed for order #{OrderId}", madh); }

                            // SignalR broadcast đến khách hàng
                            try
                            {
                                await _hubContext.Clients.Group($"order_{madh}").SendAsync("paymentConfirmed", madh, donHang.tongtien);
                            }
                            catch { }
                        }
                        else
                        {
                            // Thanh toán thất bại
                            _logger.LogWarning("MoMo payment failed for order #{OrderId}: ResultCode={ResultCode}", madh, resultCode);
                            donHang.trangthai = "Chờ thanh toán";

                            try
                            {
                                await _hubContext.Clients.Group($"order_{madh}").SendAsync("paymentFailed", madh, ipnParams.GetValueOrDefault("message", "Thanh toán thất bại"));
                            }
                            catch { }
                        }
                        await db.SaveChangesAsync();
                    }
                }
            }

            // MoMo yêu cầu response OK để không gửi lại IPN
            return Json(new { error = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MoMo IPN callback error");
            return Json(new { error = -1, message = ex.Message });
        }
    }

    // ─── Helper: kiểm tra có phải PayPal không ───
    private bool IsPayPalMethod(int pttt)
    {
        try
        {
            var method = db.tbLoaiHinhThanhToan.Find(pttt);
            if (method == null) return false;
            var name = (method.tenhinhthuc ?? "").ToLowerInvariant();
            return name.Contains("paypal");
        }
        catch
        {
            return false;
        }
    }

    // ─── Helper: kiểm tra có phải bank transfer không ───
    private bool IsBankTransferMethod(int pttt)
    {
        // ponytail: Kiểm tra tên phương thức từ DB, bỏ dấu tiếng Việt để match linh hoạt
        try
        {
            var method = db.tbLoaiHinhThanhToan.Find(pttt);
            if (method == null) return false;
            var name = RemoveDiacritics((method.tenhinhthuc ?? "").ToLowerInvariant());
            return name.Contains("chuyen khoan") || name.Contains("ngan hang") || name.Contains("bank");
        }
        catch
        {
            return false;
        }
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(System.Text.NormalizationForm.FormC);
    }

    // ─── BANK WEBHOOK: Casso/SePay/PayOS tự động gọi khi có biến động số dư ───
    [HttpPost]
    [AllowAnonymous]
    [Route("Payment/BankWebhook")]
    public async Task<JsonResult> BankWebhook()
    {
        try
        {
            // ponytail: Fix Item 10 — REQUIRE token, ko cho bypass neu BANK_WEBHOOK_TOKEN chua set
            // Neu chua config token, tra ve 401 luon
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

            // ponytail: FixedTimeEquals chong timing attack
            if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(token),
                System.Text.Encoding.UTF8.GetBytes(BankWebhookToken)))
            {
                _logger.LogWarning("BankWebhook: Token mismatch (FixedTimeEquals)");
                return Json(new { error = "Unauthorized" });
            }

            // Đọc body
            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse JSON (linh hoạt với nhiều định dạng webhook)
            var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body);

            // Tìm nội dung chuyển khoản (memo/description) — hỗ trợ nhiều định dạng
            string? memo = null;
            long? amount = null;

            // Casso format: data[0].description, data[0].amount
            if (json.TryGetProperty("data", out var data) && data.ValueKind == System.Text.Json.JsonValueKind.Array && data.GetArrayLength() > 0)
            {
                var first = data[0];
                if (first.TryGetProperty("description", out var desc)) memo = desc.GetString();
                if (first.TryGetProperty("amount", out var amt)) amount = (long)amt.GetDecimal();
            }

            // SePay format: content (n?i dung CK), transferAmount, gateway
            // SePay g?i webhook v?i field "content" ch?a n?i dung chuy?n kho?n
            if (memo == null && json.TryGetProperty("content", out var sepayContent)) memo = sepayContent.GetString();
            if (memo == null && json.TryGetProperty("transferDesc", out var td)) memo = td.GetString();
            if (amount == null && json.TryGetProperty("transferAmount", out var ta)) amount = (long)ta.GetDecimal();

            // SePay gateway (tên ngân hàng) — log cho traceability
            if (json.TryGetProperty("gateway", out var gateway))
            {
                _logger.LogInformation("BankWebhook: Gateway={Gateway}", gateway.GetString());
            }

            // PayOS format: amount, description
            if (amount == null && json.TryGetProperty("amount", out var payosAmt)) amount = (long)payosAmt.GetDecimal();
            if (memo == null && json.TryGetProperty("description", out var pd)) memo = pd.GetString();

            if (string.IsNullOrEmpty(memo))
            {
                _logger.LogWarning("BankWebhook: No memo found in webhook data");
                return Json(new { error = "No memo" });
            }

            // ═══ Parse mã từ memo ═══
            // Format 1: SEVQR FASTSHIP{madh} — thanh toán đơn hàng (VD: SEVQR FASTSHIP42)
            // Format 2: SEVQR FASTSHIPNAP{UserId}_{Timestamp} — nạp tiền ví (VD: SEVQR FASTSHIPNAP1_20240715143000)
            var orderMatch = System.Text.RegularExpressions.Regex.Match(memo, @"FASTSHIP(\d+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var depositMatch = System.Text.RegularExpressions.Regex.Match(memo, @"FASTSHIPNAP(\d+)_(\d+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (depositMatch.Success)
            {
                // ═══ WALLET DEPOSIT: Nạp tiền vào ví ═══
                if (!int.TryParse(depositMatch.Groups[1].Value, out var depositUserId))
                {
                    _logger.LogWarning("BankWebhook: Cannot parse user ID from deposit memo: {Memo}", memo);
                    return Json(new { error = "Invalid deposit memo" });
                }

                var user = await db.tbUsers.FindAsync(depositUserId);
                if (user == null)
                {
                    _logger.LogWarning("BankWebhook: Deposit user #{UserId} not found", depositUserId);
                    return Json(new { error = "User not found" });
                }

                if (amount == null || amount.Value <= 0)
                {
                    _logger.LogWarning("BankWebhook: Invalid deposit amount: {Amount}", amount);
                    return Json(new { error = "Invalid amount" });
                }

                user.vitien = (user.vitien ?? 0) + (decimal)amount.Value;
                await db.SaveChangesAsync();

                // Ghi log
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

                _logger.LogInformation("BankWebhook: Wallet deposit {Amount}đ for user #{UserId} via bank transfer", amount, depositUserId);

                // SignalR broadcast đến user
                try
                {
                    await _hubContext.Clients.Group($"customer_{depositUserId}").SendAsync("walletDeposit", amount, user.vitien);
                }
                catch { }

                return Json(new { error = 0, message = "Deposit approved" });
            }
            else if (orderMatch.Success)
            {
                // ═══ ORDER PAYMENT: Thanh toán đơn hàng ═══
                if (!int.TryParse(orderMatch.Groups[1].Value, out var madh))
                {
                    _logger.LogWarning("BankWebhook: Cannot parse order ID from memo: {Memo}", memo);
                    return Json(new { error = "Invalid memo format" });
                }

                var donHang = await db.tbDonHangs.FindAsync(madh);
                if (donHang == null)
                {
                    _logger.LogWarning("BankWebhook: Order #{OrderId} not found", madh);
                    return Json(new { error = "Order not found" });
                }

                if (donHang.trangthai != "Chờ thanh toán")
                {
                    _logger.LogInformation("BankWebhook: Order #{OrderId} already processed (status: {Status})", madh, donHang.trangthai);
                    return Json(new { error = 0, message = "Already processed" });
                }

                // ponytail: Fix Item 25 — tranh lossy cast (long)decimal, dung Convert.ToDecimal
                // Fix Item 20 — kiem tra don hang da huy/hoan thanh thi ko xu ly
                if (donHang.trangthai == "Đã hủy" || donHang.trangthai == "Hoàn thành")
                {
                    _logger.LogInformation("BankWebhook: Order #{OrderId} already final (status: {Status})", madh, donHang.trangthai);
                    return Json(new { error = 0, message = "Already processed" });
                }

                // Kiểm tra số tiền (cho phép sai số ±1000đ do phí ngân hàng)
                if (amount == null || Math.Abs(Convert.ToDecimal(amount.Value) - (donHang.tongtien ?? 0)) > 1000)
                {
                    _logger.LogWarning("BankWebhook: Amount mismatch for order #{OrderId}. Expected: {Expected}, Received: {Received}",
                        madh, donHang.tongtien, amount);
                    return Json(new { error = "Amount mismatch" });
                }

                // ═══ Cập nhật trạng thái đơn hàng ═══
                donHang.trangthai = "Đã đặt";
                donHang.ngaythanhtoan = DateTime.Now;
                await db.SaveChangesAsync();

                // ─── E-Delivery: Auto-sinh E-Invoice khi bank transfer confirmed ───
                try { await _eDelivery.GenerateEInvoice(madh); }
                catch (Exception edEx) { _logger.LogWarning(edEx, "E-Invoice generation failed for order #{OrderId}", madh); }

                _logger.LogInformation("BankWebhook: Order #{OrderId} auto-approved via bank transfer", madh);

                // ═══ SignalR broadcast đến khách hàng ═══
                try
                {
                    await _hubContext.Clients.Group($"order_{madh}").SendAsync("paymentConfirmed", madh, donHang.tongtien);
                    await _hubContext.Clients.Group($"order_{madh}").SendAsync("orderStatusChanged", madh, "Đã đặt", DateTime.Now.ToString("HH:mm"));
                }
                catch { }

                // ═══ SignalR broadcast đến quán ăn ═══
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

    // ponytail: Da xoa 15-min self-confirm fallback — phai cho bank webhook thuc te
    // Nguoi dung khong the tu xac nhan chuyen khoan, dam bao tinh bao mat
    // Thay bang: kiem tra trang thai don hang hien tai
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

            // ponytail: Fix Item 20 — cancelled la final state, ko tra ve success
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


    // ─── MoMo Payment Return URL (sau khi user thanh toán xong) ───
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

    // ─── Create PayPal Order: T?o don PayPal, tr? v? approve link ───
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
                // L?u PayPal OrderId vào session d? dùng khi capture
                HttpContext.Session.SetString($"paypal_order_{orderId}", result.PayPalOrderId ?? "");

                _logger.LogInformation("PayPal order created: OrderId={OrderId}, PayPalOrderId={PayPalOrderId}, Amount={Amount}USD",
                    orderId, result.PayPalOrderId, Math.Round((donHang.tongtien ?? 0) / 25000m, 2));

                return Json(new
                {
                    success = true,
                    approveLink = result.ApproveLink,
                    paypalOrderId = result.PayPalOrderId,
                    message = "Chuyển hướng đến PayPal..."
                });
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

    // ─── Capture PayPal Order: Thu ti?n sau khi khách duy?t trên PayPal ───
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

            // L?y PayPal OrderId t? session ho?c t? query string token (PayPal g?i token param)
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

                // SignalR broadcast
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
}
