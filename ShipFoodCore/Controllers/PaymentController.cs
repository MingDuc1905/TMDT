using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipFood.Models;

namespace ShipFood.Controllers;

public class PaymentController : BaseController
{
    public PaymentController(dbFoodyEntities context)
    {
        db = context;
    }

    /// <summary>
    /// Xử lý thanh toán mô phỏng - nhận kết quả test (success/failure) từ AJAX
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult ProcessPayment(int? mattdh, string hoten, string quan, string diachicuthe,
        string diachiadd, string SDT, string note, int pttt, string testResult)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        // Validate testResult
        if (testResult != "success" && testResult != "failure")
            return Json(new { success = false, message = "Kết quả kiểm thử không hợp lệ" });

        var cart = GetCart();
        if (cart == null || cart.monAns.Count == 0)
            return Json(new { success = false, message = "Giỏ hàng trống" });

        // Mô phỏng thanh toán thất bại
        if (testResult == "failure")
        {
            return Json(new
            {
                success = false,
                message = "Thanh toán thất bại hoặc phiên giao dịch hết hạn. Vui lòng thử lại!",
                keepCart = true
            });
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
                    return Json(new { success = false, message = "Địa chỉ không hợp lệ" });
            }
            else
            {
                ttdh = new tbThongTinDatHang
                {
                    userid = user!.userid,
                    sdt = SDT ?? "",
                    diachi = $"{diachiadd}, {diachicuthe}, {quan}, TP Đà Nẵng",
                    toado = new NetTopologySuite.Geometries.Point(90, 90) { SRID = 4326 },
                    tennguoinhan = hoten ?? ""
                };
                db.tbThongTinDatHang.Add(ttdh);
                db.SaveChanges();
            }

            decimal tongTienMon = cart.monAns.Sum(m => (m.giatien ?? 0) * m.soLuong);
            decimal phiShip = 15000;

            var dh = new tbDonHang
            {
                maquan = cart.maquanan,
                mattdh = ttdh.mattdh,
                ngaydathang = DateTime.Now,
                trangthai = "Đang xử lý",
                tongtien = tongTienMon,
                hinhthucthanhtoan = pttt,
                ghichu = note,
                phiship = phiShip
            };
            db.tbDonHang.Add(dh);
            db.SaveChanges();

            foreach (var i in cart.monAns)
            {
                db.tbChiTietDonHang.Add(new tbChiTietDonHang
                {
                    madh = dh.madh,
                    mamon = i.mamon,
                    soluong = i.soLuong,
                    dongia = i.giatien
                });
            }
            db.SaveChanges();

            SetCart(new Cart());

            return Json(new
            {
                success = true,
                message = $"Đặt hàng thành công! Mã đơn hàng: #{dh.madh}",
                orderId = dh.madh,
                trangthai = "Đang xử lý"
            });
        }
        catch
        {
            return Json(new { success = false, message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." });
        }
    }
}
