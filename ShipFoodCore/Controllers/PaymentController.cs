using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ShipFood.Hubs;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Controllers;

public class PaymentController : BaseController
{
    private readonly ILogger<PaymentController> _logger;
    private readonly IHubContext<Chats> _hubContext;
    private readonly MoMoService _moMoService;
    private readonly IConfiguration _configuration;
    private readonly EDeliveryService _eDelivery;

    public PaymentController(dbFoodyEntities context, ILogger<PaymentController> logger, IHubContext<Chats> hubContext,
        MoMoService moMoService, IConfiguration configuration, EDeliveryService eDelivery)
    {
        db = context;
        _logger = logger;
        _hubContext = hubContext;
        _moMoService = moMoService;
        _configuration = configuration;
        _eDelivery = eDelivery;
    }

    // ─── Bank transfer config (đọc từ env vars) ───
    private string BankId => _configuration["BANK_ID"] ?? "Vietcombank";
    private string BankAccountNo => _configuration["BANK_ACCOUNT_NO"] ?? "1234567890";
    private string BankAccountName => _configuration["BANK_ACCOUNT_NAME"] ?? "FASTSHIP CO., LTD";
    private string BankWebhookToken => _configuration["BANK_WEBHOOK_TOKEN"] ?? "";

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> ProcessPayment(int? mattdh, string? hoten, string? quan, string? diachicuthe,
        string? diachiadd, string? SDT, string? note, int pttt, string testResult, int? makhuyenmai = null)
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

        if (cart.maquanan == null)
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

            // ═══ IDEMPOTENCY LOCK: Chống tạo đơn trùng trong 30 giây ═══
            // Nếu user vừa tạo đơn thành công (qua bất kỳ thiết bị nào), chặn tạo thêm
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

            decimal phiShip = 15000;
            decimal discountAmount = 0;
            int? appliedCouponId = null;

            if (makhuyenmai != null)
            {
                var coupon = db.tbKhuyenMai.Find(makhuyenmai);
                if (coupon != null && (coupon.ngayketthuc == null || coupon.ngayketthuc >= DateTime.Now))
                {
                    int phanTram = coupon.phantramgiam ?? 0;
                    discountAmount = tongTienMon * phanTram / 100;
                    appliedCouponId = coupon.makm;
                }
            }

            decimal tongCong = Math.Max(tongTienMon + phiShip - discountAmount, 0);

            // ═══ Bank Transfer: đặt trạng thái 'Chờ thanh toán' thay vì 'Đã đặt' ═══
            bool isBankTransfer = IsBankTransferMethod(pttt);
            var trangThaiBanDau = isBankTransfer ? "Chờ thanh toán" : "Đã đặt";

            var dh = new tbDonHang
            {
                maquan    = cart.maquanan,
                mattdh    = ttdh.mattdh,
                ngaydathang = DateTime.Now,
                trangthai = trangThaiBanDau,
                tongtien  = tongCong,
                hinhthucthanhtoan = pttt,
                ghichu    = note,
                phiship   = phiShip,
                makhuyenmai = appliedCouponId
            };
            db.tbDonHang.Add(dh);
            db.SaveChanges();

            foreach (var i in cart.items)
            {
                db.tbChiTietDonHang.Add(new tbChiTietDonHang
                {
                    madh    = dh.madh,
                    mamon   = i.mabienthe,
                    soluong = i.soLuong,
                    dongia  = i.giatien
                });
            }
            db.SaveChanges();

            // ─── MoMo Payment: Nếu chọn thanh toán MoMo (mahttt=5), tạo payment request ───
            // ⚠️ Fix: KHÔNG xóa cart trước khi MoMo API hoàn tất
            // Nếu MoMo thất bại, user vẫn còn cart để thử lại
            string? momoPayUrl = null;
            if (pttt == 5) // mahttt=5 = MoMo (xem tbLoaiHinhThanhToan seed)
            {
                try
                {
                    var momoRequest = new MoMoCreatePaymentRequest
                    {
                        OrderId = $"FS{dh.madh}_{DateTime.Now:yyyyMMddHHmmss}",
                        OrderInfo = $"Thanh toan don hang FastShip #{dh.madh}",
                        Amount = (long)(tongCong * 1000), // MoMo tính theo VND (số nguyên)
                        RedirectUrl = $"{Request.Scheme}://{Request.Host}/Cart/SuccessView?orderId={dh.madh}",
                        IpnUrl = $"{Request.Scheme}://{Request.Host}/Payment/MoMoIpn",
                        RequestType = "captureWallet",
                        ExtraData = $"{dh.madh}"
                    };

                    var momoResult = await _moMoService.CreatePaymentAsync(momoRequest);
                    if (momoResult.IsSuccess)
                    {
                        momoPayUrl = momoResult.PayUrl;
                        _logger.LogInformation("MoMo payment URL created for order #{OrderId}: {PayUrl}", dh.madh, momoPayUrl);
                    }
                    else
                    {
                        _logger.LogWarning("MoMo payment failed for order #{OrderId}: {Message}", dh.madh, momoResult.Message);
                    }
                }
                catch (Exception momoEx)
                {
                    _logger.LogError(momoEx, "MoMo payment creation failed for order #{OrderId}", dh.madh);
                }
            }

            _logger.LogInformation("Order #{OrderId} placed by user {UserId}", dh.madh, user.userid);

            // ─── 1h: Ghi nhận lịch sử sử dụng mã giảm giá ───
            // ⚠️ Non-blocking: nếu bảng chưa tồn tại hoặc lỗi ghi, payment vẫn thành công
            if (appliedCouponId != null)
            {
                try
                {
                    db.tbLichSuSuDungKhuyenMai.Add(new tbLichSuSuDungKhuyenMai
                    {
                        userid = user!.userid,
                        makm = appliedCouponId.Value,
                        ngaydung = DateTime.Now,
                        madh = dh.madh
                    });
                    db.SaveChanges();
                }
                catch (Exception couponEx)
                {
                    _logger.LogWarning(couponEx, "Failed to record coupon usage for order #{OrderId}, coupon #{CouponId}", dh.madh, appliedCouponId);
                    // Không throw — payment vẫn thành công
                }
            }

            // ─── Bank Transfer: KHÔNG xóa cart, trả về QR URL ───
            if (isBankTransfer)
            {
                // Tạo VietQR URL động
                var memo = $"FASTSHIP{dh.madh}";
                var qrUrl = $"https://img.vietqr.io/image/{BankId}-{BankAccountNo}-compact2.png?amount={(long)tongCong}&addInfo={Uri.EscapeDataString(memo)}&accountName={Uri.EscapeDataString(BankAccountName)}";

                _logger.LogInformation("Bank transfer QR URL generated for order #{OrderId}: {QrUrl}", dh.madh, qrUrl);

                // Không clear cart — user cần quay lại nếu muốn đặt thêm
                // ponytail: giữ cart để user dễ đặt lại nếu không muốn chờ QR

                return Json(new
                {
                    success = true,
                    message = $"Đơn hàng #{dh.madh} đang chờ thanh toán. Vui lòng quét mã QR để chuyển khoản.",
                    orderId = dh.madh,
                    trangthai = "Chờ thanh toán",
                    qrCodeUrl = qrUrl,
                    bankInfo = new
                    {
                        bankId = BankId,
                        accountNo = BankAccountNo,
                        accountName = BankAccountName,
                        amount = (long)tongCong,
                        memo = memo
                    }
                });
            }

            // ─── Chỉ xóa cart sau khi MoMo API đã được gọi (dù thành công hay thất bại) ───
            // Cart được clear sau khi MoMo attempt để nếu MoMo thất bại, user còn giỏ hàng để thử lại
            SetCart(new Cart());

            try
            {
                await _hubContext.Clients.Group($"restaurant_{cart.maquanan}").SendAsync("newOrder", new
                {
                    orderId = dh.madh,
                    customerName = hoten ?? "Khách",
                    totalAmount = tongCong,
                    status = "Đã đặt",
                    time = DateTime.Now.ToString("HH:mm")
                });
            }
            catch { }

            return Json(new
            {
                success  = true,
                message  = $"Đặt hàng thành công! Mã đơn hàng: #{dh.madh}",
                orderId  = dh.madh,
                trangthai = "Đang xử lý",
                momoPayUrl = momoPayUrl
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

    // ─── Helper: kiểm tra có phải bank transfer không ───
    private bool IsBankTransferMethod(int pttt)
    {
        // ponytail: Check trực tiếp tên phương thức từ DB — linh hoạt với mọi seed data
        try
        {
            var method = db.tbLoaiHinhThanhToan.Find(pttt);
            if (method == null) return false;
            var name = (method.tenhinhthuc ?? "").ToLowerInvariant();
            return name.Contains("chuyển khoản") || name.Contains("ngân hàng") || name.Contains("bank");
        }
        catch
        {
            return false;
        }
    }

    // ─── BANK WEBHOOK: Casso/SePay/PayOS tự động gọi khi có biến động số dư ───
    [HttpPost]
    [AllowAnonymous]
    public async Task<JsonResult> BankWebhook()
    {
        try
        {
            // ═══ Xác thực Secure Token từ Header ═══
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? "";
            var token = authHeader.Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(BankWebhookToken) && token != BankWebhookToken)
            {
                _logger.LogWarning("BankWebhook: Invalid token received");
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

            // SePay format: transferDesc, transferAmount
            if (memo == null && json.TryGetProperty("transferDesc", out var td)) memo = td.GetString();
            if (amount == null && json.TryGetProperty("transferAmount", out var ta)) amount = (long)ta.GetDecimal();

            // PayOS format: description, amount
            if (memo == null && json.TryGetProperty("description", out var pd)) memo = pd.GetString();
            if (amount == null && json.TryGetProperty("amount", out var pa)) amount = (long)pa.GetDecimal();

            if (string.IsNullOrEmpty(memo))
            {
                _logger.LogWarning("BankWebhook: No memo found in webhook data");
                return Json(new { error = "No memo" });
            }

            // ═══ Parse mã đơn hàng từ memo ═══
            // Format: FASTSHIP{madh}  (VD: FASTSHIP42)
            var match = System.Text.RegularExpressions.Regex.Match(memo, @"FASTSHIP(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!match.Success || !int.TryParse(match.Groups[1].Value, out var madh))
            {
                _logger.LogWarning("BankWebhook: Cannot parse order ID from memo: {Memo}", memo);
                return Json(new { error = "Invalid memo format" });
            }

            // ═══ Đối soát đơn hàng ═══
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

            // Kiểm tra số tiền (cho phép sai số ±1000đ do phí ngân hàng)
            if (amount == null || Math.Abs((decimal)(amount.Value - (long)(donHang.tongtien ?? 0))) > 1000)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "BankWebhook processing error");
            return Json(new { error = -1, message = ex.Message });
        }
    }

    // ─── VERIFY BANK TRANSACTION: Fallback khi webhook không đến ───
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
            // Load thông tin địa chỉ để kiểm tra quyền sở hữu
            var ttdh = await db.tbThongTinDatHangs.FindAsync(donHang.mattdh);
            if (ttdh?.userid != user?.userid)
                return Json(new { success = false, message = "Không có quyền kiểm tra đơn hàng này" });

            if (donHang.trangthai != "Chờ thanh toán")
            {
                return Json(new
                {
                    success = true,
                    alreadyConfirmed = true,
                    message = $"Đơn hàng đã được xác nhận (trạng thái: {donHang.trangthai})",
                    trangthai = donHang.trangthai
                });
            }

            // ponytail: Không có API tra cứu ngân hàng thật → kiểm tra thời gian chờ
            // Nếu đã qua 15 phút kể từ khi tạo đơn, cho phép user tự xác nhận
            if (donHang.ngaydathang != null && DateTime.Now - donHang.ngaydathang > TimeSpan.FromMinutes(15))
            {
                donHang.trangthai = "Đã đặt";
                donHang.ngaythanhtoan = DateTime.Now;
                await db.SaveChangesAsync();

                _logger.LogInformation("VerifyBankTransaction: Order #{OrderId} manually confirmed (15min fallback)", madh);

                try
                {
                    await _hubContext.Clients.Group($"order_{madh}").SendAsync("paymentConfirmed", madh, donHang.tongtien);
                    await _hubContext.Clients.Group($"order_{madh}").SendAsync("orderStatusChanged", madh, "Đã đặt", DateTime.Now.ToString("HH:mm"));
                }
                catch { }

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

                return Json(new { success = true, message = "✅ Đơn hàng đã được xác nhận!" });
            }

            var remainingSeconds = (int)(15 * 60 - (DateTime.Now - donHang.ngaydathang!.Value).TotalSeconds);
            return Json(new
            {
                success = false,
                message = $"Hệ thống đang chờ xác nhận từ ngân hàng. Vui lòng thử lại sau {Math.Max(remainingSeconds, 0)} giây nữa, hoặc đợi tự động duyệt trong vòng 15 phút."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VerifyBankTransaction failed for order #{OrderId}", madh);
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
}
