using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Controllers;

public class CartController : BaseController
{
    private readonly RecommendationService _recommendationService;

    public CartController(dbFoodyEntities context, RecommendationService recommendationService)
    {
        db = context;
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public async Task<ActionResult> Index()
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");
        var maKMs = db.tbKhuyenMai.ToList();
        ViewBag.maKMs = maKMs;

        // ─── Apriori: Gợi ý món mua kèm dựa trên giỏ hàng hiện tại ───
        var cart = GetCart();
        if (cart != null && cart.monAns.Any())
        {
            var cartMonIds = cart.monAns.Select(m => m.mamon).ToList();
            ViewBag.AprioriCartSuggestions = await _recommendationService.GetAprioriRecommendations(cartMonIds, 4);
        }
        else
        {
            ViewBag.AprioriCartSuggestions = new List<tbMonAn>();
        }

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

        // Ch? gi? l?i ph??ng th?c thanh to�n: Ti?n m?t, Chuy?n kho?n (lo?i b? ZaloPay, MoMo, PayPal)
        var excludedMethods = new[] { "ZaloPay", "Momo", "Paypal" };
        ViewBag.phuongthuctt = db.tbLoaiHinhThanhToan
            .AsEnumerable()
            .Where(t => !excludedMethods.Any(e => t.tenhinhthuc.Contains(e, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        ViewBag.diachicosan = db.tbThongTinDatHang.Where(tt => tt.userid == user!.userid).ToList();
        ViewBag.cart = cart;
        ViewBag.CouponList = db.tbKhuyenMai.Where(k => k.ngayketthuc == null || k.ngayketthuc >= DateTime.Now).Take(5).ToList();
        return View();
    }

    // ─── API: Lấy danh sách mã giảm giá khả dụng (còn hạn, còn số lượng) ───
    [HttpGet]
    public JsonResult GetAvailableCoupons()
    {
        var now = DateTime.Now;
        var coupons = db.tbKhuyenMai
            .Where(k => (k.ngayketthuc == null || k.ngayketthuc >= now)
                       && (k.ngaybatdau == null || k.ngaybatdau <= now))
            .OrderByDescending(k => k.phantramgiam)
            .Select(k => new
            {
                makm = k.makm,
                tenkm = k.tenkm,
                mota = k.mota ?? "",
                phantramgiam = k.phantramgiam ?? 0,
                dieukien = k.dieukien ?? "",
                ngaybatdau = k.ngaybatdau,
                ngayketthuc = k.ngayketthuc
            })
            .ToList();

        return Json(new { success = true, coupons = coupons });
    }

    // ─── API: Lấy 1-2 mã giảm giá tốt nhất (gợi ý cho user) ───
    [HttpGet]
    public JsonResult GetTopCoupons()
    {
        var now = DateTime.Now;
        var coupons = db.tbKhuyenMai
            .Where(k => (k.ngayketthuc == null || k.ngayketthuc >= now)
                       && (k.ngaybatdau == null || k.ngaybatdau <= now))
            .OrderByDescending(k => k.phantramgiam)
            .Take(2)
            .Select(k => new
            {
                makm = k.makm,
                tenkm = k.tenkm,
                phantramgiam = k.phantramgiam ?? 0,
                mota = k.mota ?? "",
                dieukien = k.dieukien ?? ""
            })
            .ToList();

        return Json(new { success = true, coupons = coupons });
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

        // ─── Multi-restaurant check: nếu cart có món khác quán → redirect kèm TempData ───
        if (cart.maquanan != null && cart.maquanan != monAn!.maquanan)
        {
            TempData["CartConflict"] = "true";
            TempData["ConflictNewQuan"] = monAn.maquanan?.ToString() ?? "";
            TempData["ConflictNewMonId"] = maMonAn.ToString();
            TempData["ConflictNewSoLuong"] = soLuong.ToString();
            return RedirectToAction("Index");
        }

        if (cart.maquanan == null)
            cart.maquanan = monAn!.maquanan;

        cart.themMon(monAn, soLuong);
        SetCart(cart);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult TangSoLuong(int maMonAn, int soLuong)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

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

    // ─── API: Kiểm tra xung đột quán và thêm món (AJAX) ───
    [HttpGet]
    public JsonResult ApiThemMonAn(int maMonAn, int soLuong)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (soLuong <= 0)
            return Json(new { success = false, message = "Số lượng không hợp lệ" });

        var monAn = db.tbMonAn.Find(maMonAn);
        if (monAn == null)
            return Json(new { success = false, message = "Món ăn không tồn tại" });

        var cart = GetCart() ?? new Cart();

        // Kiểm tra xung đột quán
        if (cart.maquanan != null && cart.maquanan != monAn.maquanan && cart.monAns.Any())
        {
            var currentQuan = db.tbQuanAn.Find(cart.maquanan);
            var newQuan = db.tbQuanAn.Find(monAn.maquanan);
            return Json(new
            {
                success = false,
                conflict = true,
                maMonAn = maMonAn,
                soLuong = soLuong,
                currentRestaurant = currentQuan?.tenquanan ?? "",
                newRestaurant = newQuan?.tenquanan ?? "",
                message = "Giỏ hàng đã có món từ quán khác"
            });
        }

        // Cùng quán hoặc giỏ trống → thêm bình thường
        if (cart.maquanan == null)
            cart.maquanan = monAn.maquanan;
        cart.themMon(monAn, soLuong);
        SetCart(cart);

        return Json(new
        {
            success = true,
            conflict = false,
            soLuong = cart.monAns.FirstOrDefault(m => m.mamon == maMonAn)?.soLuong ?? soLuong,
            cartTotal = cart.tongTien?.ToString("N0") + " đ",
            cartGrandTotal = (cart.tongTien + 15000)?.ToString("N0") + " đ",
            redirect = Url.Action("Index", "Cart")
        });
    }

    // ─── API: Xác nhận chuyển quán (xóa cart cũ + thêm món mới) ───
    [HttpGet]
    public JsonResult ApiForceSwitchRestaurant(int maMonAn, int soLuong)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (soLuong <= 0)
            return Json(new { success = false, message = "Số lượng không hợp lệ" });

        var monAn = db.tbMonAn.Find(maMonAn);
        if (monAn == null)
            return Json(new { success = false, message = "Món ăn không tồn tại" });

        // Xóa cart cũ, tạo cart mới với quán mới
        var newCart = new Cart { maquanan = monAn.maquanan };
        newCart.themMon(monAn, soLuong);
        SetCart(newCart);

        return Json(new
        {
            success = true,
            redirect = Url.Action("Index", "Cart")
        });
    }

    /// <summary>
    /// API JSON: Tăng số lượng món trong giỏ hàng (trả về JSON thay vì HTML)
    /// </summary>
    [HttpGet]
    public JsonResult ApiTangSoLuong(int maMonAn, int soLuong)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (soLuong <= 0) soLuong = 1;

        var cart = GetCart() ?? new Cart();
        var monAn = db.tbMonAn.Find(maMonAn);
        if (monAn == null)
            return Json(new { success = false, message = "Món ăn không tồn tại" });

        if (cart.maquanan == null)
            cart.maquanan = monAn.maquanan;
        else if (cart.maquanan != monAn.maquanan)
            cart = new Cart { maquanan = monAn.maquanan };

        cart.themMon(monAn, soLuong);
        SetCart(cart);

        var item = cart.monAns.FirstOrDefault(m => m.mamon == maMonAn);
        return Json(new
        {
            success = true,
            soLuong = item?.soLuong ?? 0,
            itemTotal = item != null ? (item.giatien * item.soLuong)?.ToString("N0") + " đ" : "0 đ",
            cartTotal = cart.tongTien?.ToString("N0") + " đ",
            cartGrandTotal = (cart.tongTien + 15000)?.ToString("N0") + " đ"
        });
    }

    [HttpGet]
    public ActionResult GiamSoLuong(int maMonAn)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        var cart = GetCart();
        if (cart != null)
        {
            var existingItem = cart.monAns.FirstOrDefault(m => m.mamon == maMonAn);
            if (existingItem != null && existingItem.soLuong <= 1)
            {
                SetCart(cart);
                return RedirectToAction("Index");
            }
            cart.giamMon(maMonAn);
            SetCart(cart);
        }
        return RedirectToAction("Index");
    }

    /// <summary>
    /// API JSON: Giảm số lượng món trong giỏ hàng (trả về JSON thay vì HTML)
    /// </summary>
    [HttpGet]
    public JsonResult ApiGiamSoLuong(int maMonAn)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        var cart = GetCart();
        if (cart == null)
            return Json(new { success = false, message = "Giỏ hàng trống" });

        var existingItem = cart.monAns.FirstOrDefault(m => m.mamon == maMonAn);
        if (existingItem != null && existingItem.soLuong <= 1)
        {
            // Giữ nguyên, trả về thông tin hiện tại
            return Json(new
            {
                success = true,
                soLuong = existingItem.soLuong,
                itemTotal = (existingItem.giatien * existingItem.soLuong)?.ToString("N0") + " đ",
                cartTotal = cart.tongTien?.ToString("N0") + " đ",
                cartGrandTotal = (cart.tongTien + 15000)?.ToString("N0") + " đ"
            });
        }

        cart.giamMon(maMonAn);
        SetCart(cart);

        var item = cart.monAns.FirstOrDefault(m => m.mamon == maMonAn);
        return Json(new
        {
            success = true,
            soLuong = item?.soLuong ?? 0,
            itemTotal = item != null ? (item.giatien * item.soLuong)?.ToString("N0") + " đ" : "0 đ",
            cartTotal = cart.tongTien?.ToString("N0") + " đ",
            cartGrandTotal = (cart.tongTien + 15000)?.ToString("N0") + " đ"
        });
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
            .Include(dh => dh.tbQuanAn)
            .Include(dh => dh.tbChiTietDonHangs)
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

    public ActionResult FailureView()
    {
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

        // Validate thông tin bắt buộc (chỉ khi KHÔNG dùng địa chỉ có sẵn)
        if (mattdh == null)
        {
            if (string.IsNullOrWhiteSpace(hoten) || hoten.Length < 2 || hoten.Length > 100)
            {
                TempData["CartError"] = "Họ tên phải từ 2-100 ký tự";
                return RedirectToAction("Checkout");
            }
            if (string.IsNullOrWhiteSpace(SDT) || !System.Text.RegularExpressions.Regex.IsMatch(SDT, @"^0[1-9][0-9]{8,9}$"))
            {
                TempData["CartError"] = "Số điện thoại không hợp lệ — phải là 10-11 số, bắt đầu bằng 0";
                return RedirectToAction("Checkout");
            }
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
                diachi = $"{diachiadd}, {diachicuthe}, {quan}, TP. Hồ Chí Minh",
                toado = null,
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
