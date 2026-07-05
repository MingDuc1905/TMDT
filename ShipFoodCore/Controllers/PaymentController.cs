using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ShipFood.Hubs;
using ShipFood.Models;

namespace ShipFood.Controllers;

public class PaymentController : BaseController
{
    private readonly ILogger<PaymentController> _logger;
    private readonly IHubContext<Chats> _hubContext;

    public PaymentController(dbFoodyEntities context, ILogger<PaymentController> logger, IHubContext<Chats> hubContext)
    {
        db = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Xử lý thanh toán mô phỏng - nhận kết quả test (success/failure) từ AJAX
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> ProcessPayment(int? mattdh, string? hoten, string? quan, string? diachicuthe,
        string? diachiadd, string? SDT, string? note, int pttt, string testResult, int? makhuyenmai = null)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập để tiếp tục" });

        // Validate testResult
        if (testResult != "success" && testResult != "failure")
            return Json(new { success = false, message = "Tham số không hợp lệ" });

        // Validate thông tin người nhận (chỉ khi KHÔNG dùng địa chỉ có sẵn)
        if (mattdh == null)
        {
            if (string.IsNullOrWhiteSpace(hoten) || hoten.Length < 2 || hoten.Length > 100)
                return Json(new { success = false, message = "Họ tên phải từ 2-100 ký tự" });
            if (string.IsNullOrWhiteSpace(SDT) || !System.Text.RegularExpressions.Regex.IsMatch(SDT, @"^0[1-9][0-9]{8,9}$"))
                return Json(new { success = false, message = "Số điện thoại không hợp lệ — phải là 10-11 số, bắt đầu bằng 0" });
            if (string.IsNullOrWhiteSpace(diachicuthe) || diachicuthe.Length < 5)
                return Json(new { success = false, message = "Địa chỉ cụ thể quá ngắn (ít nhất 5 ký tự)" });
        }

        var cart = GetCart();
        if (cart == null || cart.monAns.Count == 0)
            return Json(new { success = false, message = "Giỏ hàng trống. Vui lòng thêm món trước khi thanh toán." });

        // Mô phỏng thanh toán thất bại
        if (testResult == "failure")
        {
            var failures = new[] {
                "Thẻ của bạn đã hết hạn. Vui lòng kiểm tra lại thông tin thẻ.",
                "Số dư tài khoản không đủ để thực hiện giao dịch.",
                "Giao dịch bị từ chối do ngân hàng gặp sự cố. Vui lòng thử lại sau.",
                "Mã xác thực OTP không hợp lệ. Vui lòng thực hiện lại.",
                "Phiên thanh toán đã hết hạn. Vui lòng thử lại."
            };
            var msg = failures[new System.Random().Next(failures.Length)];
            return Json(new { success = false, message = msg, keepCart = true });
        }

        // Mô phỏng thanh toán thành công -> lưu đơn hàng
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

            decimal tongTienMon = cart.monAns.Sum(m => (m.giatien ?? 0) * m.soLuong);
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

            foreach (var i in cart.monAns)
            {
                db.tbChiTietDonHang.Add(new tbChiTietDonHang
                {
                    madh    = dh.madh,
                    mamon   = i.mamon,
                    soluong = i.soLuong,
                    dongia  = i.giatien
                });
            }
            db.SaveChanges();

            SetCart(new Cart());

            _logger.LogInformation("Order #{OrderId} placed by user {UserId}", dh.madh, user.userid);

            // ─── SignalR: Broadcast đơn hàng mới đến Quán ăn ───
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
            catch { /* SignalR broadcast không ảnh hưởng đến luồng chính */ }

            return Json(new
            {
                success  = true,
                message  = $"Đặt hàng thành công! Mã đơn hàng: #{dh.madh}",
                orderId  = dh.madh,
                trangthai = "Đang xử lý"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProcessPayment failed for user {User}", GetCurrentUser()?.userid);
            return Json(new { success = false, message = $"Lỗi hệ thống: {ex.Message}. Vui lòng thử lại." });
        }
    }
}
