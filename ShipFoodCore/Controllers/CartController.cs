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
        if (cart != null && cart.items.Any())
        {
            var cartMonIds = cart.items.Select(m => m.mamon).Distinct().ToList();
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

        if (cart == null || cart.items.Count == 0)
        {
            TempData["CartError"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm món trước khi thanh toán.";
            return RedirectToAction("Index");
        }

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

    [HttpGet]
    public JsonResult GetAvailableCoupons()
    {
        var roleCheck = CheckRoleJson("Khách hàng");
        if (roleCheck != null) return roleCheck;

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

    [HttpGet]
    public JsonResult GetTopCoupons()
    {
        var roleCheck = CheckRoleJson("Khách hàng");
        if (roleCheck != null) return roleCheck;

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
        var roleCheck = CheckRoleJson("Khách hàng");
        if (roleCheck != null) return roleCheck;

        if (string.IsNullOrWhiteSpace(code))
            return Json(new { success = false, message = "Vui lòng nhập mã khuyến mãi" });

        var coupon = db.tbKhuyenMai
            .FirstOrDefault(k => k.tenkm.ToLower() == code.ToLower()
                && (k.ngayketthuc == null || k.ngayketthuc >= DateTime.Now));

        if (coupon == null)
            return Json(new { success = false, message = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn" });

        if (coupon.ngaybatdau != null && coupon.ngaybatdau > DateTime.Now)
            return Json(new { success = false, message = "Mã khuyến mãi chưa đến ngày áp dụng" });

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

    // ─── Helper: build CartItem from tbBienTheMonAn.id ───
    private CartItem? BienTheToCartItem(int mabienthe)
    {
        // Try tbBienTheMonAn.id first, then fallback to tbBienTheMonAn.mamon (tbMonAn FK)
        // DetailRestaurant sends tbMonAn.mamon, API sends tbBienTheMonAn.id
        var bt = db.tbBienTheMonAn.Include(b => b.tbMonAn).FirstOrDefault(b => b.id == mabienthe)
               ?? db.tbBienTheMonAn.Include(b => b.tbMonAn).FirstOrDefault(b => b.mamon == mabienthe);
        if (bt?.tbMonAn == null) return null;
        return new CartItem
        {
            mabienthe = bt.id,
            mamon = bt.tbMonAn.mamon,
            tenmon = bt.tbMonAn.tenmon,
            size = bt.size,
            giatien = bt.giatien,
            hinhanh = bt.tbMonAn.hinhanh,
            maquanan = bt.tbMonAn.maquanan
        };
    }

    [HttpGet]
    public ActionResult ThemMonAn(int maMonAn, int soLuong)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        if (soLuong <= 0)
        {
            TempData["CartError"] = "Hãy chọn số lượng trước khi thêm";
            var bt = db.tbBienTheMonAn.Include(b => b.tbMonAn).FirstOrDefault(b => b.id == maMonAn)
                  ?? db.tbBienTheMonAn.Include(b => b.tbMonAn).FirstOrDefault(b => b.mamon == maMonAn);
            if (bt?.tbMonAn != null)
                return RedirectToAction("DetailRestaurant", "Home", new { id = bt.tbMonAn.maquanan });
            return RedirectToAction("Index", "Home");
        }

        var cart = GetCart() ?? new Cart();
        var item = BienTheToCartItem(maMonAn);
        if (item == null)
        {
            TempData["CartError"] = "Món ăn không tồn tại";
            return RedirectToAction("Index", "Home");
        }

        if (cart.maquanan != null && cart.maquanan != item.maquanan)
        {
            TempData["CartConflict"] = "true";
            TempData["ConflictNewQuan"] = item.maquanan?.ToString() ?? "";
            TempData["ConflictNewMonId"] = maMonAn.ToString();
            TempData["ConflictNewSoLuong"] = soLuong.ToString();
            return RedirectToAction("Index");
        }

        if (cart.maquanan == null)
            cart.maquanan = item.maquanan;

        cart.themMon(item, soLuong);
        SetCart(cart);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult TangSoLuong(int maMonAn, int soLuong)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        if (soLuong <= 0) soLuong = 1;

        var cart = GetCart() ?? new Cart();
        var item = BienTheToCartItem(maMonAn);
        if (item == null)
        {
            TempData["CartError"] = "Món ăn không tồn tại";
            return RedirectToAction("Index", "Home");
        }

        if (cart.maquanan == null)
            cart.maquanan = item.maquanan;
        else if (cart.maquanan != item.maquanan)
            cart = new Cart();

        cart.themMon(item, soLuong);
        SetCart(cart);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public JsonResult ApiThemMonAn(int maMonAn, int soLuong)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (soLuong <= 0)
            return Json(new { success = false, message = "Số lượng không hợp lệ" });

        var item = BienTheToCartItem(maMonAn);
        if (item == null)
            return Json(new { success = false, message = "Món ăn không tồn tại" });

        var cart = GetCart() ?? new Cart();

        if (cart.maquanan != null && cart.maquanan != item.maquanan && cart.items.Any())
        {
            var currentQuan = db.tbQuanAn.Find(cart.maquanan);
            var newQuan = db.tbQuanAn.Find(item.maquanan);
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

        if (cart.maquanan == null)
            cart.maquanan = item.maquanan;
        cart.themMon(item, soLuong);
        SetCart(cart);

        return Json(new
        {
            success = true,
            conflict = false,
            soLuong = cart.items.FirstOrDefault(m => m.mabienthe == maMonAn)?.soLuong ?? soLuong,
            cartTotal = cart.tongTien?.ToString("N0") + " đ",
            cartGrandTotal = (cart.tongTien + 15000)?.ToString("N0") + " đ",
            redirect = Url.Action("Index", "Cart")
        });
    }

    [HttpGet]
    public JsonResult ApiForceSwitchRestaurant(int maMonAn, int soLuong)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (soLuong <= 0)
            return Json(new { success = false, message = "Số lượng không hợp lệ" });

        var item = BienTheToCartItem(maMonAn);
        if (item == null)
            return Json(new { success = false, message = "Món ăn không tồn tại" });

        var newCart = new Cart { maquanan = item.maquanan };
        newCart.themMon(item, soLuong);
        SetCart(newCart);

        return Json(new { success = true, redirect = Url.Action("Index", "Cart") });
    }

    [HttpGet]
    public JsonResult ApiTangSoLuong(int maMonAn, int soLuong)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (soLuong <= 0) soLuong = 1;

        var cart = GetCart() ?? new Cart();
        var item = BienTheToCartItem(maMonAn);
        if (item == null)
            return Json(new { success = false, message = "Món ăn không tồn tại" });

        if (cart.maquanan == null)
            cart.maquanan = item.maquanan;
        else if (cart.maquanan != item.maquanan)
            cart = new Cart { maquanan = item.maquanan };

        cart.themMon(item, soLuong);
        SetCart(cart);

        var cartItem = cart.items.FirstOrDefault(m => m.mabienthe == maMonAn);
        return Json(new
        {
            success = true,
            soLuong = cartItem?.soLuong ?? 0,
            itemTotal = cartItem != null ? (cartItem.giatien * cartItem.soLuong)?.ToString("N0") + " đ" : "0 đ",
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
            var existingItem = cart.items.FirstOrDefault(m => m.mabienthe == maMonAn);
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

    [HttpGet]
    public JsonResult ApiGiamSoLuong(int maMonAn)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        var cart = GetCart();
        if (cart == null)
            return Json(new { success = false, message = "Giỏ hàng trống" });

        var existingItem = cart.items.FirstOrDefault(m => m.mabienthe == maMonAn);
        if (existingItem != null && existingItem.soLuong <= 1)
        {
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

        var item = cart.items.FirstOrDefault(m => m.mabienthe == maMonAn);
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
            .Include(d => d.tbChiTietDonHangs).ThenInclude(c => c.tbBienTheMonAn!).ThenInclude(b => b.tbMonAn)
            .Include(d => d.tbThongTinDatHang)
            .Include(d => d.tbShipper).ThenInclude(s => s.tbUser)
            .Include(d => d.tbLoaiHinhThanhToan)
            .FirstOrDefault(d => d.madh == id);
        return View();
    }

    // ─── Phase 4: Order Tracking with Live Map ───
    [HttpGet]
    public ActionResult OrderTracking(int? id)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");
        if (id == null)
            return RedirectToAction("LichSuDatHang");

        var donHang = db.tbDonHang
            .Include(d => d.tbChiTietDonHangs).ThenInclude(c => c.tbBienTheMonAn!).ThenInclude(b => b.tbMonAn)
            .Include(d => d.tbThongTinDatHang)
            .Include(d => d.tbQuanAn)
            .Include(d => d.tbShipper!).ThenInclude(s => s.tbUser)
            .Include(d => d.tbLoaiHinhThanhToan)
            .FirstOrDefault(d => d.madh == id);

        if (donHang == null)
            return RedirectToAction("LichSuDatHang");

        ViewBag.DonHang = donHang;
        return View();
    }

    // ─── Phase 2a: Restore cart from localStorage ───
    [HttpPost]
    public JsonResult RestoreFromLocal([FromBody] Cart localCart)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (localCart == null || localCart.items == null || localCart.items.Count == 0)
            return Json(new { success = false, message = "Giỏ hàng trống" });

        // Chỉ restore nếu session cart đang trống
        var sessionCart = GetCart();
        if (sessionCart != null && sessionCart.items.Any())
            return Json(new { success = true, exists = true });

        // Kiểm tra các item có tồn tại trong DB không
        var validItems = new List<CartItem>();
        foreach (var item in localCart.items)
        {
            var bt = db.tbBienTheMonAn.Include(b => b.tbMonAn).FirstOrDefault(b => b.id == item.mabienthe);
            if (bt?.tbMonAn != null)
            {
                validItems.Add(new CartItem
                {
                    mabienthe = bt.id,
                    mamon = bt.tbMonAn.mamon,
                    tenmon = bt.tbMonAn.tenmon,
                    size = bt.size,
                    giatien = bt.giatien,
                    hinhanh = bt.tbMonAn.hinhanh,
                    maquanan = bt.tbMonAn.maquanan,
                    soLuong = item.soLuong
                });
            }
        }

        if (validItems.Count == 0)
            return Json(new { success = false, message = "Món ăn không còn tồn tại" });

        var newCart = new Cart
        {
            maquanan = validItems.First().maquanan,
            items = validItems
        };
        newCart.tongTien = validItems.Sum(i => i.giatien * i.soLuong);
        SetCart(newCart);

        return Json(new { success = true, redirect = Url.Action("Index") });
    }

    public ActionResult FailureView() => View();

    public ActionResult SuccessView()
    {
        var user = GetCurrentUser();
        if (user == null) return RedirectToAction("Login", "Home");

        var cart = GetCart();
        if (cart == null || cart.items.Count == 0) return RedirectToAction("Index", "Cart");

        string? hoten = TempData["hoten"] as string;
        string? quan = TempData["quan"] as string;
        string? diachicuthe = TempData["diachicuthe"] as string;
        string? diachiadd = TempData["diachiadd"] as string;
        string? SDT = TempData["SDT"] as string;
        int pttt = int.TryParse(TempData["pttt"] as string, out var p) ? p : 1;
        int? mattdh = int.TryParse(TempData["mattdh"] as string, out var m) ? m : null;
        string? note = TempData["note"] as string;

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

        decimal tongTienMon = cart.items.Sum(m => (m.giatien ?? 0) * m.soLuong);
        decimal phiShip = 15000;
        decimal tongCong = tongTienMon + phiShip;

        var dh = new tbDonHang
        {
            maquan = cart.maquanan,
            mattdh = ttdh.mattdh,
            ngaydathang = DateTime.Now,
            trangthai = "Đã đặt",
            tongtien = tongTienMon,
            hinhthucthanhtoan = pttt,
            ghichu = note,
            phiship = phiShip
        };
        db.tbDonHang.Add(dh);
        db.SaveChanges();

        foreach (var i in cart.items)
        {
            db.tbChiTietDonHang.Add(new tbChiTietDonHang
            {
                madh = dh.madh,
                mamon = i.mabienthe,
                soluong = i.soLuong,
                dongia = i.giatien
            });
        }
        db.SaveChanges();

        SetCart(new Cart());
        TempData["OrderSuccess"] = "Đặt hàng thành công! Mã đơn hàng: #" + dh.madh;
        return View();
    }
}
