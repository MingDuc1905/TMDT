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

    public PaymentController(dbFoodyEntities context, ILogger<PaymentController> logger, IHubContext<Chats> hubContext, MoMoService moMoService)
    {
        db = context;
        _logger = logger;
        _hubContext = hubContext;
        _moMoService = moMoService;
    }

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

            decimal tongTienMon = cart.items.Sum(m => (m.giatien ?? 0) * m.soLuong);
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

            var dh = new tbDonHang
            {
                maquan    = cart.maquanan,
                mattdh    = ttdh.mattdh,
                ngaydathang = DateTime.Now,
                trangthai = "Đã đặt",
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

            SetCart(new Cart());

            // ─── MoMo Payment: Nếu chọn thanh toán MoMo (mahttt=5), tạo payment request ───
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

            // Phân loại lỗi chi tiết
            var errorMessage = ex switch
            {
                DbUpdateException due when due.InnerException?.Message?.Contains("FK_") == true
                    => "Dữ liệu không hợp lệ: ràng buộc khóa ngoại bị vi phạm.",
                DbUpdateException due when due.InnerException?.Message?.Contains("UNIQUE") == true
                    => "Dữ liệu bị trùng lặp: đơn hàng này đã tồn tại.",
                DbUpdateException due when due.InnerException?.Message?.Contains("timeout") == true
                    => "Kết nối cơ sở dữ liệu bị timeout. Vui lòng thử lại.",
                OperationCanceledException _
                    => "Yêu cầu đã bị hủy do quá thời gian chờ. Vui lòng thử lại.",
                InvalidOperationException ioe when ioe.Message.Contains("session")
                    => "Phiên đặt hàng đã hết hạn. Vui lòng đăng nhập lại.",
                _ => $"Lỗi hệ thống: {ex.Message}. Vui lòng thử lại."
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
