using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipFood.Models;

namespace ShipFood.Controllers;

public class CartController : BaseController
{
    public CartController(dbFoodyEntities context)
    {
        db = context;
    }

    [HttpGet]
    public ActionResult Index()
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");
        var maKMs = db.tbKhuyenMai.ToList();
        ViewBag.maKMs = maKMs;
        return View();
    }

    [HttpGet]
    public ActionResult Checkout()
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");
        var cart = GetCart();
        var user = GetCurrentUser();

        // Kiểm tra giỏ hàng trống
        if (cart == null || cart.monAns.Count == 0)
        {
            TempData["CartError"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm món trước khi thanh toán.";
            return RedirectToAction("Index");
        }

        ViewBag.phuongthuctt = db.tbLoaiHinhThanhToan.Where(t => !t.tenhinhthuc.Contains("Paypal")).ToList();
        ViewBag.diachicosan = db.tbThongTinDatHang.Where(tt => tt.userid == user!.userid).ToList();
        ViewBag.cart = cart;
        ViewBag.CouponList = db.tbKhuyenMai.Where(k => k.ngayketthuc == null || k.ngayketthuc >= DateTime.Now).Take(5).ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult CheckCoupon(string code, decimal tongTien)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Json(new { success = false, message = "Vui lòng nhập mã khuyến mãi" });

        var coupon = db.tbKhuyenMai
            .FirstOrDefault(k => k.tenkm.ToLower() == code.ToLower()
                && (k.ngayketthuc == null || k.ngayketthuc >= DateTime.Now));

        if (coupon == null)
            return Json(new { success = false, message = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn" });

        // Kiểm tra ngày bắt đầu
        if (coupon.ngaybatdau != null && coupon.ngaybatdau > DateTime.Now)
            return Json(new { success = false, message = "Mã khuyến mãi chưa đến ngày áp dụng" });

        // Tính tiền giảm
        int phanTramGiam = coupon.phantramgiam ?? 0;
        decimal discountAmount = tongTien * phanTramGiam / 100;

        return Json(new
        {
            success = true,
            message = $"Áp dụng mã '{coupon.tenkm}' thành công! Giảm {phanTramGiam}% ({discountAmount:N0} VNĐ)",
            couponId = coupon.makm,
            discountPercent = phanTramGiam,
            discountAmount = discountAmount
        });
    }

    [HttpPost]
    public ActionResult Checkout(string hoten, string quan, string diachicuthe, string diachiadd,
        string SDT, string note, int pttt, int? mattdh)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        // Save checkout info in TempData for SuccessView
        TempData["hoten"] = hoten;
        TempData["quan"] = quan;
        TempData["diachicuthe"] = diachicuthe;
        TempData["diachiadd"] = diachiadd;
        TempData["SDT"] = SDT;
        TempData["pttt"] = pttt.ToString();
        TempData["mattdh"] = mattdh?.ToString();
        TempData["note"] = note;

        // Both "Tiền mặt" and "Chuyển khoản" go directly to success
        return RedirectToAction("SuccessView");
    }

    [HttpGet]
    public ActionResult ThemMonAn(int maMonAn, int soLuong)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        // BUG-001/002/003: Chặn số lượng <= 0
        if (soLuong <= 0)
        {
            TempData["CartError"] = "Hãy chọn số lượng trước khi thêm";
            var monAnRef = db.tbMonAn.Find(maMonAn);
            if (monAnRef != null)
                return RedirectToAction("DetailRestaurant", "Home", new { id = monAnRef.maquanan });
            return RedirectToAction("Index", "Home");
        }

        var cart = GetCart() ?? new Cart();
        var monAn = db.tbMonAn.Find(maMonAn);

        if (cart.maquanan == null)
        {
            cart.maquanan = monAn!.maquanan;
        }
        else if (cart.maquanan != monAn!.maquanan)
        {
            cart = new Cart { maquanan = monAn.maquanan };
        }
        cart.themMon(monAn, soLuong);
        SetCart(cart);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult TangSoLuong(int maMonAn, int soLuong)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        // BUG-001/002/003: Chặn số lượng <= 0
        if (soLuong <= 0)
            soLuong = 1;

        var cart = GetCart() ?? new Cart();
        var monAn = db.tbMonAn.Find(maMonAn);

        if (cart.maquanan == null)
            cart.maquanan = monAn!.maquanan;
        else if (cart.maquanan != monAn!.maquanan)
            cart = new Cart();

        cart.themMon(monAn, soLuong);
        SetCart(cart);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult GiamSoLuong(int maMonAn)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        var cart = GetCart();
        if (cart != null)
        {
            // BUG-008: Kiểm tra nếu số lượng = 1 thì không giảm nữa (giữ nguyên >= 1)
            var existingItem = cart.monAns.FirstOrDefault(m => m.mamon == maMonAn);
            if (existingItem != null && existingItem.soLuong <= 1)
            {
                // Không giảm, giữ nguyên
                SetCart(cart);
                return RedirectToAction("Index");
            }
            cart.giamMon(maMonAn);
            SetCart(cart);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult XoaMon(int maMonAn)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        var cart = GetCart();
        if (cart != null)
        {
            cart.xoaMon(maMonAn);
            SetCart(cart);
        }
        return RedirectToAction("Index");
    }

    public ActionResult LichSuDatHang()
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        var user = GetCurrentUser();
        var donHangs = db.tbDonHang
            .Include(dh => dh.tbThongTinDatHang)
            .Where(dh => dh.tbThongTinDatHang!.userid == user!.userid)
            .OrderBy(dh => dh.ngaydathang)
            .ToList();
        donHangs.Reverse();
        ViewBag.donHangs = donHangs;
        return View();
    }

    public ActionResult ChiTietDonHang(int? id)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");
        if (id == null)
            return RedirectToAction("LichSuDatHang");
        ViewBag.donHang = db.tbDonHang
            .Include(d => d.tbChiTietDonHangs).ThenInclude(c => c.tbMonAn)
            .Include(d => d.tbThongTinDatHang)
            .Include(d => d.tbShipper).ThenInclude(s => s.tbUser)
            .Include(d => d.tbLoaiHinhThanhToan)
            .FirstOrDefault(d => d.madh == id);
        return View();
    }

    public ActionResult SuccessView()
    {
        var user = GetCurrentUser();
        if (user == null) return RedirectToAction("Login", "Home");
        
        var cart = GetCart();
        if (cart == null || cart.monAns.Count == 0) return RedirectToAction("Index", "Cart");

        string? hoten = TempData["hoten"] as string;
        string? quan = TempData["quan"] as string;
        string? diachicuthe = TempData["diachicuthe"] as string;
        string? diachiadd = TempData["diachiadd"] as string;
        string? SDT = TempData["SDT"] as string;
        int pttt = int.TryParse(TempData["pttt"] as string, out var p) ? p : 1;
        int? mattdh = int.TryParse(TempData["mattdh"] as string, out var m) ? m : null;
        string? note = TempData["note"] as string;

        // Validate thông tin bắt buộc
        if (string.IsNullOrWhiteSpace(hoten) || string.IsNullOrWhiteSpace(SDT))
        {
            TempData["CartError"] = "Vui lòng điền đầy đủ thông tin người nhận";
            return RedirectToAction("Checkout");
        }

        tbThongTinDatHang ttdh;
        if (mattdh != null)
        {
            ttdh = db.tbThongTinDatHang.Find(mattdh)!;
            if (ttdh == null)
            {
                TempData["CartError"] = "Địa chỉ không hợp lệ";
                return RedirectToAction("Checkout");
            }
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

        // Tính tổng tiền chính xác từ giỏ hàng
        decimal tongTienMon = cart.monAns.Sum(m => (m.giatien ?? 0) * m.soLuong);
        decimal phiShip = 15000;
        decimal tongCong = tongTienMon + phiShip;

        var dh = new tbDonHang
        {
            maquan = cart.maquanan,
            mattdh = ttdh.mattdh,
            ngaydathang = DateTime.Now, // Luôn dùng thời gian thực - không cho phép chọn ngày tương lai
            trangthai = "Đã đặt",
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

        // Xóa giỏ hàng sau khi đặt thành công
        SetCart(new Cart());

        TempData["OrderSuccess"] = "Đặt hàng thành công! Mã đơn hàng: #" + dh.madh;
        return View();
    }
}
