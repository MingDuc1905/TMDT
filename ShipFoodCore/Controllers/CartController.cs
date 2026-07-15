using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipFood.Helpers;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Controllers;

public class CartController : BaseController
{
    private readonly RecommendationService _recommendationService;
    private readonly VoucherService _voucherService;
    private readonly IConfiguration _configuration;

    public CartController(dbFoodyEntities context, RecommendationService recommendationService,
                          VoucherService voucherService, IConfiguration configuration)
    {
        db = context;
        _recommendationService = recommendationService;
        _voucherService = voucherService;
        _configuration = configuration;
    }

    // ─── Bank transfer config ───
    // BankId = tên ngân hàng (hi?n th?), BankVietQrBinCode = BIN code (cho VietQR API)
    private string BankId => _configuration["BANK_ID"] ?? "Vietcombank";
    private string BankVietQrBinCode => BankHelper.GetVietQrBinCode(BankId);
    private string BankAccountNo => _configuration["BANK_ACCOUNT_NO"] ?? "1234567890";
    private string BankAccountName => _configuration["BANK_ACCOUNT_NAME"] ?? "FASTSHIP CO., LTD";

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
    public async Task<ActionResult> Checkout()
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

        // Hiển thị tất cả phương thức thanh toán (bao gồm MoMo, ZaloPay, Paypal)
        ViewBag.phuongthuctt = db.tbLoaiHinhThanhToan.ToList();
        // ═══ GROUP BY: deduplicate addresses by sdt + diachi + tennguoinhan ═══
        var allAddresses = db.tbThongTinDatHang.Where(tt => tt.userid == user!.userid).ToList();
        ViewBag.diachicosan = allAddresses
            .GroupBy(tt => new { tt.sdt, tt.diachi, tt.tennguoinhan })
            .Select(g => g.First())
            .ToList();
        ViewBag.cart = cart;

        // ═══ VOUCHER AUTO-ASSIGN: Gợi ý voucher theo khung giờ + thông tin user ═══
        var recommendedVouchers = await _voucherService.GetRecommendedVouchers(user?.userid, cart.tongTien);
        ViewBag.RecommendedVouchers = recommendedVouchers;
        ViewBag.CurrentTimeSlot = VoucherService.GetCurrentTimeSlotInfo();

        // Coupon list thường (fallback)
        ViewBag.CouponList = db.tbKhuyenMai.Where(k => k.ngayketthuc == null || k.ngayketthuc >= DateTime.Now).Take(5).ToList();

        // ═══ Bank info từ env vars — dùng BIN code cho VietQR API ═══
        ViewBag.BankId = BankVietQrBinCode; // BIN code (VD: 970415) cho VietQR API
        ViewBag.BankIdDisplay = BankId; // Tên ngân hàng (VD: VietinBank) cho UI hi?n th?
        ViewBag.BankAccountNo = BankAccountNo;
        ViewBag.BankAccountName = BankAccountName;

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

    // ─── API: Bỏ chọn mã giảm giá ───
    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult RemoveCoupon()
    {
        var roleCheck = CheckRoleJson("Khách hàng");
        if (roleCheck != null) return roleCheck;

        var cart = GetCart();
        if (cart != null)
        {
            cart.maKM = null;
            SetCart(cart);
        }

        return Json(new { success = true });
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
            return Json(new { success = false, message = "Mã khuyến mãi chưa đến ngày áp dụng" });            // ─── 1b: Kiểm tra tần suất sử dụng mã của User ───
            var user = GetCurrentUser();
            if (user != null)
            {
                // Non-blocking: nếu bảng chưa tồn tại, bỏ qua kiểm tra usage
                try
                {
                    var usageCount = db.tbLichSuSuDungKhuyenMai
                        .Count(ls => ls.userid == user.userid && ls.makm == coupon.makm);

                    if (usageCount > 0)
                    {
                        return Json(new { success = false, message = "Bạn đã sử dụng mã này rồi. Mỗi mã chỉ được dùng 1 lần." });
                    }
                }
                catch (Exception ex)
                {
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<CartController>>();
                    logger.LogWarning(ex, "Coupon usage check failed — skipping");
                }
            }

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

    // ─── Helper: build CartItem from mamon or bientheId ───
    private CartItem? BienTheToCartItem(int maMonAn)
    {
        // ⚠️ VIEWS gửi tbMonAn.mamon (VD: 12 = "Mẹt B"), không phải tbBienTheMonAn.id!
        // Tìm theo tbBienTheMonAn.mamon (FK→tbMonAn) TRƯỚC để khớp đúng món.
        // Fallback: tìm theo b.id (nếu có API gửi thẳng tbBienTheMonAn.id)
        var bt = db.tbBienTheMonAn.Include(b => b.tbMonAn).FirstOrDefault(b => b.mamon == maMonAn)
               ?? db.tbBienTheMonAn.Include(b => b.tbMonAn).FirstOrDefault(b => b.id == maMonAn);
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

        var item = BienTheToCartItem(maMonAn);
        if (item == null)
        {
            TempData["CartError"] = "Món ăn không tồn tại";
            return RedirectToAction("Index", "Home");
        }

        // ═══ HIGH-2: Stock check — kiểm tra món còn hàng không ═══
        var monAn = db.tbMonAn.Find(item.mamon);
        if (monAn != null && monAn.conhang == false)
        {
            TempData["CartError"] = $"Món '{item.tenmon}' đã hết hàng. Vui lòng chọn món khác.";
            if (item.maquanan != null)
                return RedirectToAction("DetailRestaurant", "Home", new { id = item.maquanan });
            return RedirectToAction("Index", "Home");
        }

        var cart = GetCart() ?? new Cart();

        // ═══ MULTI-RESTAURANT: Cho phép thêm món từ nhiều quán khác nhau ═══
        // Nếu quán mới khác quán hiện tại, giữ nguyên giỏ hàng và cho phép thêm
        // Checkout sẽ tách đơn riêng theo từng quán
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

        // ═══ HIGH-2: Stock check — kiểm tra món còn hàng không ═══
        var monAn = db.tbMonAn.Find(item.mamon);
        if (monAn != null && monAn.conhang == false)
            return Json(new { success = false, message = $"Món '{item.tenmon}' đã hết hàng. Vui lòng chọn món khác." });

        var cart = GetCart() ?? new Cart();

        // ═══ MULTI-RESTAURANT: Cho phép thêm món từ nhiều quán ═══
        if (cart.maquanan == null)
            cart.maquanan = item.maquanan;
        cart.themMon(item, soLuong);
        SetCart(cart);

        return Json(new
        {
            success = true,
            conflict = false,
            soLuong = FindCartItemByMamon(cart, maMonAn)?.soLuong ?? soLuong,
            cartCount = cart.items.Sum(i => i.soLuong),
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

        // ═══ MULTI-RESTAURANT: Cho phép tăng số lượng từ bất kỳ quán nào ═══
        if (cart.maquanan == null)
            cart.maquanan = item.maquanan;

        cart.themMon(item, soLuong);
        SetCart(cart);

        var cartItem = FindCartItemByMamon(cart, maMonAn);
        return Json(new
        {
            success = true,
            soLuong = cartItem?.soLuong ?? 0,
            itemTotal = cartItem != null ? (cartItem.giatien * cartItem.soLuong)?.ToString("N0") + " đ" : "0 đ",
            cartTotal = cart.tongTien?.ToString("N0") + " đ",
            cartGrandTotal = (cart.tongTien + 15000)?.ToString("N0") + " đ"
        });
    }

    // ─── Helper: tìm CartItem trong giỏ theo tbMonAn.mamon (vì view gửi mamon) ───
    private CartItem? FindCartItemByMamon(Cart cart, int maMonAn)
    {
        return cart?.items.FirstOrDefault(m => m.mamon == maMonAn);
    }

    [HttpGet]
    public ActionResult GiamSoLuong(int maMonAn)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        var cart = GetCart();
        if (cart != null)
        {
            var existingItem = FindCartItemByMamon(cart, maMonAn);
            if (existingItem != null && existingItem.soLuong <= 1)
            {
                SetCart(cart);
                return RedirectToAction("Index");
            }
            if (existingItem != null)
                cart.giamMon(existingItem.mabienthe);
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

        var existingItem = FindCartItemByMamon(cart, maMonAn);
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

        if (existingItem != null)
            cart.giamMon(existingItem.mabienthe);
        SetCart(cart);

        var item = FindCartItemByMamon(cart, maMonAn);
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
            var item = FindCartItemByMamon(cart, maMonAn);
            if (item != null)
                cart.xoaMon(item.mabienthe);
            SetCart(cart);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public JsonResult ApiXoaMon(int maMonAn)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        var cart = GetCart();
        if (cart == null)
            return Json(new { success = false, message = "Giỏ hàng trống" });

        var item = FindCartItemByMamon(cart, maMonAn);
        if (item != null)
            cart.xoaMon(item.mabienthe);
        SetCart(cart);

        return Json(new
        {
            success = true,
            cartTotal = cart.tongTien?.ToString("N0") + " đ",
            cartGrandTotal = (cart.tongTien + 15000)?.ToString("N0") + " đ",
            isEmpty = cart.items == null || cart.items.Count == 0
        });
    }

    public ActionResult LichSuDatHang()
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        var user = GetCurrentUser();
        if (user == null)
            return RedirectToAction("Login", "Home");

        try
        {
            var donHangs = db.tbDonHang
                .Include(dh => dh.tbThongTinDatHang)
                .Include(dh => dh.tbQuanAn)
                .Include(dh => dh.tbChiTietDonHangs).ThenInclude(c => c.tbBienTheMonAn!).ThenInclude(b => b.tbMonAn)
                .Where(dh => dh.tbThongTinDatHang != null && dh.tbThongTinDatHang.userid == user.userid)
                // ponytail: lọc bỏ đơn rác (0 món) do seed bị chạy nhiều lần hoặc lỗi payment
                .Where(dh => dh.tbChiTietDonHangs != null && dh.tbChiTietDonHangs.Any())
                .OrderByDescending(dh => dh.ngaydathang)
                .ToList();
            ViewBag.donHangs = donHangs;
            return View();
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<CartController>>();
            logger.LogError(ex, "LichSuDatHang failed for user {UserId}", user.userid);
            TempData["CartError"] = "Không thể tải lịch sử đơn hàng. Vui lòng thử lại sau.";
            ViewBag.donHangs = new List<tbDonHang>();
            return View();
        }
    }

    public ActionResult ChiTietDonHang(int? id)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");
        if (id == null)
            return RedirectToAction("LichSuDatHang");
        var donHang = db.tbDonHang
            .Include(d => d.tbChiTietDonHangs).ThenInclude(c => c.tbBienTheMonAn!).ThenInclude(b => b.tbMonAn)
            .Include(d => d.tbThongTinDatHang)
            .Include(d => d.tbShipper).ThenInclude(s => s.tbUser)
            .Include(d => d.tbLoaiHinhThanhToan)
            .FirstOrDefault(d => d.madh == id);
        if (donHang == null)
        {
            TempData["CartError"] = "Không tìm thấy đơn hàng #" + id;
            return RedirectToAction("LichSuDatHang");
        }
        ViewBag.donHang = donHang;
        return View();
    }

    // ─── E-Delivery: Xem hóa đơn / vận đơn điện tử ───
    [HttpGet]
    public ActionResult EInvoice(int id)
    {
        if (!CheckLogin())
            return RedirectToAction("Login", "Home");

        var invoice = db.tbEInvoices
            .Include(e => e.tbDonHang)
                .ThenInclude(d => d.tbQuanAn)
            .Include(e => e.tbDonHang)
                .ThenInclude(d => d.tbThongTinDatHang)
            .FirstOrDefault(e => e.einvoice_id == id);

        if (invoice == null)
        {
            TempData["CartError"] = "Không tìm thấy chứng từ điện tử.";
            return RedirectToAction("LichSuDatHang");
        }

        // Kiểm tra quyền xem: chỉ chủ đơn hàng/quán/shipper/admin mới được xem
        var user = GetCurrentUser();
        if (user == null)
            return RedirectToAction("Login", "Home");

        if (user.loaitaikhoan != "Admin" && user.loaitaikhoan != "Quán ăn")
        {
            var ttdh = invoice.tbDonHang?.tbThongTinDatHang;
            if (ttdh?.userid != user.userid)
            {
                // Cho phép shipper xem nếu là shipper của đơn hàng
                if (user.loaitaikhoan == "Shipper" && invoice.tbDonHang?.mashipper != user.userid)
                {
                    TempData["CartError"] = "Bạn không có quyền xem chứng từ này.";
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        ViewBag.donHang = invoice.tbDonHang;
        return View(invoice);
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

        // ═══ Bank Transfer QR: generate VietQR URL nếu đơn hàng đang chờ thanh toán ───
        if (donHang.trangthai == "Chờ thanh toán" && donHang.tbLoaiHinhThanhToan != null)
        {
            var methodName = (donHang.tbLoaiHinhThanhToan.tenhinhthuc ?? "").ToLowerInvariant();
            if (methodName.Contains("chuyển khoản") || methodName.Contains("ngân hàng") || methodName.Contains("bank"))
            {
                // ponytail: SePay format — "SEVQR FASTSHIP{OrderId}"
                var memo = $"SEVQR FASTSHIP{donHang.madh}";
                var qrUrl = $"https://img.vietqr.io/image/{BankVietQrBinCode}-{BankAccountNo}-compact2.png?amount={(long)(donHang.tongtien ?? 0)}&addInfo={Uri.EscapeDataString(memo)}&accountName={Uri.EscapeDataString(BankAccountName)}";

                ViewBag.QrCodeUrl = qrUrl;
                ViewBag.BankInfo = new
                {
                    bankId = BankId,
                    accountNo = BankAccountNo,
                    accountName = BankAccountName,
                    amount = (long)(donHang.tongtien ?? 0),
                    memo = memo
                };
            }
        }

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

        // ⚠️ Fix multi-device: Nếu user vừa đặt hàng thành công trong 5 phút gần đây, không restore localStorage cũ
        var user = GetCurrentUser();
        var recentOrder = db.tbDonHang
            .Where(dh => dh.tbThongTinDatHang != null && dh.tbThongTinDatHang.userid == user!.userid)
            .OrderByDescending(dh => dh.ngaydathang)
            .FirstOrDefault();
        if (recentOrder != null && recentOrder.ngaydathang != null 
            && (DateTime.Now - recentOrder.ngaydathang.Value).TotalMinutes < 5)
        {
            // User vừa đặt hàng (trong 5 phút) → không restore localStorage cũ
            // Xóa localStorage (frontend sẽ gọi clearCartLocal)
            return Json(new { success = true, justOrdered = true });
        }

        // Chỉ restore nếu session cart đang trống
        var sessionCart = GetCart();
        if (sessionCart != null && sessionCart.items.Any())
            return Json(new { success = true, exists = true });

        // Kiểm tra các item có tồn tại trong DB không
        var validItems = new List<CartItem>();
        foreach (var item in localCart.items)
        {
            // ═══ FIX: tìm bằng mabienthe trước, fallback bằng mamon (anonymous cart) ═══
            var bt = db.tbBienTheMonAn.Include(b => b.tbMonAn).FirstOrDefault(b => b.id == item.mabienthe);
            if (bt == null && item.mamon > 0)
            {
                bt = db.tbBienTheMonAn.Include(b => b.tbMonAn).FirstOrDefault(b => b.id == item.mamon
                    || b.mamon == item.mamon);
            }
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

    // ─── MoMo Redirect Landing: Không tạo đơn mới, chỉ hiển thị kết quả ───
    // orderId được truyền qua query string từ MoMo redirect
    public ActionResult SuccessView(int? orderId)
    {
        var user = GetCurrentUser();
        if (user == null) return RedirectToAction("Login", "Home");

        if (orderId == null)
        {
            // Nếu không có orderId, thử lấy từ TempData (trường hợp MoMo redirect có orderId trong TempData)
            if (TempData["OrderSuccess"] != null)
            {
                ViewBag.SuccessMessage = TempData["OrderSuccess"].ToString();
                ViewBag.OrderId = TempData["OrderId"];
                return View();
            }
            return RedirectToAction("LichSuDatHang");
        }

        // Kiểm tra đơn hàng có tồn tại không
        var donHang = db.tbDonHang.Find(orderId);
        if (donHang != null)
        {
            ViewBag.SuccessMessage = $"Thanh toán MoMo thành công! Mã đơn hàng: #{orderId}";
            ViewBag.OrderId = orderId;
            return View();
        }

        TempData["CartError"] = "Không tìm thấy đơn hàng. Vui lòng thử lại.";
        return RedirectToAction("LichSuDatHang");
    }
}
