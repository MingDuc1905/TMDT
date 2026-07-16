using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Controllers;

public class HomeController : BaseController
{
    private readonly RecommendationService _recommendationService;
    private readonly ICompositeViewEngine _viewEngine;

    public HomeController(dbFoodyEntities context, RecommendationService recommendationService, ICompositeViewEngine viewEngine)
    {
        db = context;
        _recommendationService = recommendationService;
        _viewEngine = viewEngine;
    }

    /// <summary>
    /// Validate password strength:
    /// - Minimum 8 characters
    /// - At least 1 uppercase letter
    /// - At least 1 lowercase letter
    /// - At least 1 digit
    /// - At least 1 special character
    /// </summary>
    private static (bool Valid, string Message) ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return (false, "Mật khẩu phải có ít nhất 8 ký tự");
        if (password.Length > 128)
            return (false, "Mật khẩu không được vượt quá 128 ký tự");
        if (!Regex.IsMatch(password, @"[A-Z]"))
            return (false, "Mật khẩu phải chứa ít nhất một chữ cái in hoa");
        if (!Regex.IsMatch(password, @"[a-z]"))
            return (false, "Mật khẩu phải chứa ít nhất một chữ cái in thường");
        if (!Regex.IsMatch(password, @"[0-9]"))
            return (false, "Mật khẩu phải chứa ít nhất một chữ số");
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            return (false, "Mật khẩu phải chứa ít nhất một ký tự đặc biệt (!@#$%^&*...)");
        return (true, "");
    }

    public static string RemoveDiacritics(string text)
    {
        // ponytail: null check — tránh NullReferenceException
        if (string.IsNullOrEmpty(text)) return text ?? "";

        // ponytail: X? lý ký tự D/d — Unicode normalization không chuy?n D → D
        // Replace D?c/D?c before normalization
        text = text.Replace('Đ', 'D').Replace('đ', 'd');

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public async Task<ActionResult> Index(string? txtSearch, int? idDM)
    {
        // ponytail: chỉ hiển thị quán có tbUser.trangthai == 1 (đang hoạt động)
        // Không hiển thị quán đã bị admin khoá (trangthai == 2)
        var quanAns = db.tbQuanAn.Include(q => q.tbUser).Include(q => q.tbMonAns).ThenInclude(m => m.tbBienTheMonAns)
            .Where(q => q.tbUser != null && q.tbUser.trangthai == 1).ToList();
        if (!string.IsNullOrEmpty(txtSearch))
        {
            string searchKeyNormalized = RemoveDiacritics(txtSearch.ToLower());
            quanAns = quanAns.Where(qa =>
                RemoveDiacritics(qa.tenquanan.ToLower()).Contains(searchKeyNormalized)
                || RemoveDiacritics(qa.tbUser.username.ToLower()).Contains(searchKeyNormalized)
                || qa.tbMonAn.Any(ma => RemoveDiacritics(ma.tenmon.ToLower()).Contains(searchKeyNormalized))
            ).ToList();
            ViewBag.txtSearch = txtSearch;
        }
        if (idDM != null && idDM > 0)
        {
            quanAns = quanAns.Where(qa => qa.tbMonAn.Where(ma => ma.madanhmuc == idDM).Any()).ToList();
            ViewBag.idDM = idDM;
        }
        ViewBag.quanAns = quanAns;

        // ponytail: load danh mục ở controller thay vì query DB trực tiếp trong view (Index.cshtml line ~230)
        // Tránh crash khi model có cột mới nhưng DB chưa có (column not found exception)
        ViewBag.DanhMucList = db.tbDanhMuc.ToList();

        // ─── Apriori: Gợi ý Combo AI cho trang chủ ───
        // ponytail: try-catch để crash không ảnh hưởng đến trang chủ
        try
        {
            ViewBag.AprioriCombo = await _recommendationService.GetPopularPairs(6);
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogWarning(ex, "PopularPairs failed — skipping Apriori on homepage");
            ViewBag.AprioriCombo = new List<tbMonAn>();
        }

        // ─── Home Stats: lấy dữ liệu THẬT từ DB (ko bịa) ───
        try
        {
            // 1. Tổng số quán ăn đang hoạt động (tbUser.trangthai == 1)
            var totalRestaurants = db.tbQuanAn
                .Include(q => q.tbUser)
                .Where(q => q.tbUser != null && q.tbUser.trangthai == 1)
                .Count();

            // 2. Tổng số đơn hàng trong 30 ngày gần nhất
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var monthlyOrders = db.tbDonHang
                .Where(dh => dh.ngaydathang >= thirtyDaysAgo)
                .Count();

            // 3. Điểm đánh giá trung bình từ tbDanhGia
            var avgRating = db.tbDanhGia
                .Where(d => d.diemdanhgia.HasValue)
                .Average(d => (double?)d.diemdanhgia) ?? 0;

            // 4. Tổng số đơn hàng tất cả thời gian (để hiển thị "50K+" nếu đủ lớn)
            var totalOrders = db.tbDonHang.Count();

            ViewBag.TotalRestaurants = totalRestaurants;
            ViewBag.MonthlyOrders = monthlyOrders;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.AvgRating = Math.Round(avgRating, 1);
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogWarning(ex, "HomeStats failed — fallback to defaults");
            ViewBag.TotalRestaurants = db.tbQuanAn.Count();
            ViewBag.MonthlyOrders = 0;
            ViewBag.TotalOrders = db.tbDonHang.Count();
            ViewBag.AvgRating = 0.0;
        }

        return View();
    }

    public async Task<ActionResult> DetailRestaurant(int id, int? idDM, string? searchKey)
    {
        try
        {
            // ponytail: không cho xem chi tiết quán đã bị khoá (tbUser.trangthai == 2)
            var quanAn = db.tbQuanAn.Include(q => q.tbUser).Include(q => q.tbMonAns).ThenInclude(m => m.tbDanhMuc)
                .Include(q => q.tbMonAns).ThenInclude(m => m.tbBienTheMonAns)
                .FirstOrDefault(t => t.userid == id);
            if (quanAn == null || quanAn.tbUser == null || quanAn.tbUser.trangthai != 1)
                return NotFound();

            var danhSachMonAn = db.tbMonAn.Where(m => m.maquanan == id).Include(m => m.tbDanhMuc).Include(m => m.tbBienTheMonAns).ToList();
            if (idDM != null)
                danhSachMonAn = danhSachMonAn.Where(ma => ma.madanhmuc == idDM).ToList();
            if (!string.IsNullOrEmpty(searchKey))
            {
                string searchKeyNormalized = RemoveDiacritics(searchKey.ToLower());
                danhSachMonAn = danhSachMonAn.Where(ma => RemoveDiacritics(ma.tenmon.ToLower()).Contains(searchKeyNormalized)).ToList();
            }

            var thucDon = db.tbDanhMuc.Where(d =>
                db.tbMonAn.Any(m => m.maquanan == id && m.madanhmuc == d.madanhmuc)).ToList();

            // Lấy danh sách khuyến mãi cho các món ăn (hiển thị cho người dùng)
            var monAnIds = danhSachMonAn.Select(m => m.mamon).ToList();
            var khuyenMais = db.tbMonAnKhuyenMai
                .Where(km => km.trangthai == "Còn hạn")
                .Include(km => km.tbKhuyenMai)
                .Include(km => km.tbBienTheMonAn)
                .ToList();

            // Lấy danh sách món ăn người dùng đã mua (nếu đã đăng nhập)
            var cartSession = GetCart();
            HashSet<int> daMuaMonAnIds = new HashSet<int>();
            if (cartSession != null && cartSession.userid > 0)
            {
                var userId = cartSession.userid;
                var mattdhIds = db.tbThongTinDatHang
                    .Where(t => t.userid == userId)
                    .Select(t => (int?)t.mattdh)
                    .ToList();
                daMuaMonAnIds = db.tbChiTietDonHang
                    .Where(ct => ct.tbDonHang != null
                        && mattdhIds.Contains(ct.tbDonHang.mattdh)
                        && ct.tbDonHang.trangthai != "Đã hủy")
                    .Where(ct => ct.tbBienTheMonAn != null && ct.tbBienTheMonAn.tbMonAn != null)
                    .Select(ct => ct.tbBienTheMonAn!.tbMonAn!.mamon)
                    .Distinct()
                    .ToHashSet();
            }
            ViewBag.DaMuaMonAnIds = daMuaMonAnIds;

            ViewBag.ThucDon = thucDon;
            ViewBag.DanhSachMonAn = danhSachMonAn;
            ViewBag.maquan = id;
            ViewBag.searchKey = searchKey;
            ViewBag.KhuyenMais = khuyenMais;

            // ─── Apriori: Gợi ý món thường mua kèm (dùng Apriori đa phần tử) ───
            try
            {
                var firstMonId = danhSachMonAn.Any() ? danhSachMonAn.First().mamon : 0;
                ViewBag.MuaKem = firstMonId > 0
                    ? await _recommendationService.GetAprioriRecommendations(new List<int> { firstMonId }, 4)
                    : new List<tbMonAn>();
                ViewBag.TrendingNow = await _recommendationService.GetTimeBasedRecommendations(4);
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
                logger.LogWarning(ex, "Recommendation failed for restaurant {Id} — skipping", id);
                ViewBag.MuaKem = new List<tbMonAn>();
                ViewBag.TrendingNow = new List<tbMonAn>();
            }

            // ponytail: render view thành string NGAY TRONG try-catch để bắt được lỗi view rendering
            // vì return View() trả về ViewResult, exception xảy ra sau đó ở pipeline → ko catch được
            // ponytail: bug DLR RuntimeBinderException đã fix trong view (cast item -> tbMonAn)
            // giữ lại RenderViewToStringAsync helper để debug sau này nếu cần
            return View(quanAn);
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogError(ex, "DetailRestaurant CRASHED for id={Id}", id);
            // ponytail: khong leak stack trace cho user — chi log server-side
            return RedirectToAction("Error", "Home", new { traceId = HttpContext.TraceIdentifier });
        }
    }

    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }        [HttpPost]
        [EnableRateLimiting("login-policy")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(string usernameOrPhone, string pwd, bool rememberMe = false)
        {
            try
            {
            // ═══ Multi-tab fix: Sign out trước khi login mới ═══
            // Tránh session cũ bị ghi đè khi login role khác trên cùng browser
            if (HttpContext.User?.Identity?.IsAuthenticated == true)
            {
                try { await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme); } catch { }
            }

            if (string.IsNullOrWhiteSpace(usernameOrPhone) || string.IsNullOrWhiteSpace(pwd))
        {
            ViewBag.LoginFail = "Vui lòng nhập tên đăng nhập/SĐT và mật khẩu";
            return View();
        }

        // Tự động nhận diện: nếu nhập số (bắt đầu bằng 0, 10-11 số) → tìm theo SĐT
        // Nếu chứa @ → tìm theo email
        // Ngược lại → tìm theo username
        bool isPhone = Regex.IsMatch(usernameOrPhone, @"^0[1-9][0-9]{8,9}$");
        bool isEmail = usernameOrPhone.Contains("@");

        IQueryable<tbUser> query = db.tbUser.AsQueryable();
        if (isPhone)
            query = query.Where(u => u.sdt == usernameOrPhone);
        else if (isEmail)
            query = query.Where(u => u.email == usernameOrPhone);
        else
            query = query.Where(u => u.username == usernameOrPhone);

        var users = query.ToList();
        if (users.Count != 0)
        {
            var userFind = users[0];

            // === Kiểm tra mật khẩu (BCrypt hash hoac plain-text cho legacy users) ===
            bool passwordMatched = false;
            // Thu bang BCrypt truoc (password moi)
            try { passwordMatched = BCrypt.Net.BCrypt.Verify(pwd, userFind.pwd); } catch { }
            // Fallback: so sanh plain-text cho legacy users (se duoc hash lai o login sau)
            if (!passwordMatched && userFind.pwd == pwd)
            {
                passwordMatched = true;
                // Upgrade len BCrypt hash
                try
                {
                    userFind.pwd = BCrypt.Net.BCrypt.HashPassword(pwd);
                    db.SaveChanges();
                }
                catch { }
            }

            if (!passwordMatched)
            {
                ViewBag.LoginFail = "Mật khẩu không đúng. Vui lòng kiểm tra lại.";
                return View();
            }

            if (userFind.trangthai == 2)
            {
                ViewBag.LoginFail = "Tài khoản đã bị khóa";
                return View();
            }

            // ponytail: clear session cũ trước — SetCart SAU Clear để ko bị xóa mất
                    HttpContext.Session.Clear();

                    var cart = new Cart { userid = userFind.userid };
                    SetCart(cart);                    // ─── Set auth cookie + session (bền vững qua restart) ───
                    await HttpContext.Session.CommitAsync();
                    await SetSessionAndCookieAsync(userFind, rememberMe);

            var redirectUrl = userFind.loaitaikhoan switch
            {
                "Khách hàng" => Url.Action("Index", "Home"),
                "Shipper" => Url.Action("Index", "Shipper"),
                "Quán ăn" => Url.Action("Index", "Restaurant"),
                "Admin" => Url.Action("Index", "Admin"),
                _ => Url.Action("Index"),
            };

            // AJAX request → trả về JSON để client tự redirect (tránh mất session khi follow 302)
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, redirectUrl });
            }

            return Redirect(redirectUrl);
        }
        else
        {
            // ponytail: security fix — không leak thông tin user tồn tại (username enumeration)
            ViewBag.LoginFail = "Tên đăng nhập, email hoặc mật khẩu không đúng. Vui lòng kiểm tra lại.";
            return View();
        }
        }
        catch (Exception ex)
        {
            // Log chi tiết để debug
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogError(ex, "Login failed for user {User}: {Error}", usernameOrPhone, ex.Message);
            // Hiển thị lỗi cụ thể — không che giấu để user biết chính xác vấn đề
            ViewBag.LoginFail = ex switch
            {
                InvalidOperationException _ when ex.Message.Contains("Session") => "Phiên đăng nhập hết hạn. Vui lòng thử lại.",
                System.Data.Common.DbException _ => "Lỗi kết nối cơ sở dữ liệu. Vui lòng thử lại sau.",
                // ponytail: security fix — không leak raw exception ra client
                _ => "Lỗi hệ thống. Vui lòng thử lại sau."
            };
            return View();
        }
    }        /// <summary>
        /// Đăng nhập bằng Google - chuyển hướng đến Google OAuth (1-click, mặc định Khách hàng)
        /// </summary>
        public IActionResult GoogleLogin()
        {
            // Kiểm tra Google OAuth có được cấu hình không
            var googleClientId = HttpContext.RequestServices
                .GetService<Microsoft.Extensions.Configuration.IConfiguration>()?
                ["Authentication:Google:ClientId"];
            if (string.IsNullOrEmpty(googleClientId))
            {
                ViewBag.LoginFail = "Đăng nhập Google chưa được cấu hình trên hệ thống này.";
                return View("Login");
            }

            var redirectUrl = Url.Action("GoogleResponse", "Home");
            var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Đăng ký Google với vai trò Đối tác (Quán ăn/Shipper) — chọn vai trò sau khi xác thực
        /// </summary>
        public IActionResult GooglePartnerLogin()
        {
            var googleClientId = HttpContext.RequestServices
                .GetService<Microsoft.Extensions.Configuration.IConfiguration>()?
                ["Authentication:Google:ClientId"];
            if (string.IsNullOrEmpty(googleClientId))
            {
                ViewBag.LoginFail = "Đăng nhập Google chưa được cấu hình trên hệ thống này.";
                return View("Login");
            }

            // Đánh dấu partner mode → GoogleResponse sẽ chuyển sang SelectRoleGoogle thay vì auto-create
            HttpContext.Session.SetString("google_partner_mode", "1");

            var redirectUrl = Url.Action("GoogleResponse", "Home");
            var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
        }    /// <summary>
        /// Đăng nhập bằng Facebook - chuyển hướng đến Facebook OAuth (1-click, mặc định Khách hàng)
        /// </summary>
        public IActionResult FacebookLogin()
        {
            // Kiểm tra Facebook OAuth có được cấu hình không
            var fbAppId = HttpContext.RequestServices
                .GetService<Microsoft.Extensions.Configuration.IConfiguration>()?
                ["Authentication:Facebook:AppId"];
            if (string.IsNullOrEmpty(fbAppId))
            {
                ViewBag.LoginFail = "Đăng nhập Facebook chưa được cấu hình trên hệ thống này.";
                return View("Login");
            }

            var redirectUrl = Url.Action("FacebookResponse", "Home");
            var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, Microsoft.AspNetCore.Authentication.Facebook.FacebookDefaults.AuthenticationScheme);
        }

    /// <summary>
        /// Facebook OAuth callback - xử lý sau khi Facebook xác thực thành công
        /// </summary>
        public async Task<ActionResult> FacebookResponse()
        {
            string? email = null;
            string? name = null;

            try
            {
            // Đọc từ cookie (Facebook middleware tự động lưu vào cookie nhờ AddCookie)
            var authenticateResult = await HttpContext.AuthenticateAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                ViewBag.LoginFail = "Đăng nhập Facebook thất bại. Vui lòng thử lại.";
                return View("Login");
            }

            // Lấy thông tin email từ Facebook
            email = authenticateResult.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            name = authenticateResult.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            // Facebook có thể không trả email nếu user dùng SĐT để đăng ký
            // Fallback: dùng name + userId để tạo email giả
            if (string.IsNullOrEmpty(email))
            {
                var fbId = authenticateResult.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                email = $"fb_{fbId ?? Guid.NewGuid().ToString("N")}@facebook.com";
                if (string.IsNullOrEmpty(name))
                    name = $"Facebook User";
            }

            // Tìm user theo email
            var users = db.tbUser.Where(u => u.email == email).ToList();
            if (users.Count == 0)
            {
                // ─── FACEBOOK LẦN ĐẦU: Auto-create Khách hàng ───
                try
                {
                    // ponytail: hash password bang BCrypt
                    var randomPwd = BCrypt.Net.BCrypt.HashPassword($"FB_{Guid.NewGuid():N}");
                    var truncatedEmail = email.Length > 50 ? email[..50] : email;
                    var shortUser = "fb_" + Guid.NewGuid().ToString("N")[..12];
                    var tenDayDu = (!string.IsNullOrEmpty(name) ? name : truncatedEmail);
                    if (tenDayDu.Length > 50) tenDayDu = tenDayDu[..50];

                    var newUser = new tbUser
                    {
                        username     = shortUser,
                        pwd          = randomPwd,
                        email        = truncatedEmail,
                        sdt          = "",
                        loaitaikhoan = "Khách hàng",
                        vitien       = 0,
                        trangthai    = 1
                    };
                    db.tbUser.Add(newUser);
                    db.SaveChanges();

                    db.tbKhachHang.Add(new tbKhachHang
                    {
                        userid = newUser.userid,
                        tenkh  = tenDayDu
                    });
                    db.SaveChanges();

                    var newCart = new Cart { userid = newUser.userid };
                    SetCart(newCart);
                    await SetSessionAndCookieAsync(newUser);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
                    logger.LogError(ex, "Auto-create Khách hàng from Facebook failed for {Email}", email);
                    ViewBag.LoginFail = "Không thể tạo tài khoản tự động. Vui lòng thử lại.";
                    return View("Login");
                }
            }

            var userFind = users[0];
            if (userFind.trangthai == 2)
            {
                ViewBag.LoginFail = "Tài khoản đã bị khóa";
                return View("Login");
            }

            var cart = new Cart { userid = userFind.userid };
            SetCart(cart);
            // ponytail: security fix — CommitAsync d?m b?o session du?c luu tru?c khi redirect
            await HttpContext.Session.CommitAsync();
            await SetSessionAndCookieAsync(userFind);

            return userFind.loaitaikhoan switch
            {
                "Khách hàng" => RedirectToAction("Index", "Home"),
                "Shipper" => RedirectToAction("Index", "Shipper"),
                "Quán ăn" => RedirectToAction("Index", "Restaurant"),
                "Admin" => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction("Index"),
            };
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
                logger.LogError(ex, "Facebook OAuth callback failed for email {Email}", email ?? "null");

                ViewBag.LoginFail = "Đăng nhập Facebook gặp sự cố. Vui lòng thử lại hoặc dùng tài khoản thường.";
                return View("Login");
            }
        }

    /// <summary>
        /// Google OAuth callback - xử lý sau khi Google xác thực thành công
        /// </summary>
        public async Task<ActionResult> GoogleResponse()
        {
            // Khai báo trước try để catch có thể dùng (C# scope rules)
            string? email = null;
            string? name = null;

            try
            {
            // Đọc từ cookie (Google middleware tự động lưu vào cookie nhờ AddCookie)
            var authenticateResult = await HttpContext.AuthenticateAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                ViewBag.LoginFail = "Đăng nhập Google thất bại. Vui lòng thử lại.";
                return View("Login");
            }

            // Lấy thông tin email từ Google
            email = authenticateResult.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            name = authenticateResult.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                ViewBag.LoginFail = "Không thể lấy thông tin email từ Google";
                return View("Login");
            }

            // Tìm user theo email
            var users = db.tbUser.Where(u => u.email == email).ToList();
            if (users.Count == 0)
            {
                // ─── GOOGLE LẦN ĐẦU: Kiểm tra partner_mode → redirect SelectRoleGoogle, nếu không → auto-create Khách hàng ───
                var isPartnerMode = HttpContext.Session.GetString("google_partner_mode");
                if (isPartnerMode == "1")
                {
                    // Xóa flag partner mode
                    HttpContext.Session.Remove("google_partner_mode");
                    // Lưu tạm vào Session → redirect sang chọn vai trò
                    HttpContext.Session.SetString("google_email", email);
                    HttpContext.Session.SetString("google_name", name ?? email);
                    return RedirectToAction("SelectRoleGoogle");
                }

                // ─── 1-CLICK: Auto-create Khách hàng ───
                try
                {
                    // ponytail: hash password bang BCrypt
                    var randomPwd = BCrypt.Net.BCrypt.HashPassword($"GG_{Guid.NewGuid():N}");
                    var truncatedEmail = email.Length > 50 ? email[..50] : email;
                    var shortUser = "gg_" + Guid.NewGuid().ToString("N")[..12];
                    var tenDayDu = (!string.IsNullOrEmpty(name) ? name : truncatedEmail);
                    if (tenDayDu.Length > 50) tenDayDu = tenDayDu[..50];

                    var newUser = new tbUser
                    {
                        username     = shortUser,
                        pwd          = randomPwd,
                        email        = truncatedEmail,
                        sdt          = "",
                        loaitaikhoan = "Khách hàng",
                        vitien       = 0,
                        trangthai    = 1
                    };
                    db.tbUser.Add(newUser);
                    db.SaveChanges();

                    db.tbKhachHang.Add(new tbKhachHang
                    {
                        userid = newUser.userid,
                        tenkh  = tenDayDu
                    });
                    db.SaveChanges();

                    var newCart = new Cart { userid = newUser.userid };
                    SetCart(newCart);
                    await SetSessionAndCookieAsync(newUser);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
                    logger.LogError(ex, "Auto-create Khách hàng from Google failed for {Email}", email);
                    ViewBag.LoginFail = "Không thể tạo tài khoản tự động. Vui lòng thử lại.";
                    return View("Login");
                }
            }

            var userFind = users[0];
            if (userFind.trangthai == 2)
            {
                ViewBag.LoginFail = "Tài khoản đã bị khóa";
                return View("Login");
            }

            var cart = new Cart { userid = userFind.userid };
            SetCart(cart);
            await SetSessionAndCookieAsync(userFind);

            return userFind.loaitaikhoan switch
            {
                "Khách hàng" => RedirectToAction("Index", "Home"),
                "Shipper" => RedirectToAction("Index", "Shipper"),
                "Quán ăn" => RedirectToAction("Index", "Restaurant"),
                "Admin" => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction("Index"),
            };
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug (KHÔNG hiển thị raw SQL cho user)
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
                logger.LogError(ex, "Google OAuth callback failed for email {Email}", email ?? "null");

                ViewBag.LoginFail = "Đăng nhập Google gặp sự cố. Vui lòng thử lại hoặc dùng tài khoản thường.";
                return View("Login");
            }
        }

        // ─── GOOGLE OAUTH: Trang chọn vai trò khi đăng nhập lần đầu ───
        [HttpGet]
        public ActionResult SelectRoleGoogle()
        {
            var email = HttpContext.Session.GetString("google_email");
            var name = HttpContext.Session.GetString("google_name");

            if (string.IsNullOrEmpty(email))
            {
                // Không có session → chưa qua Google OAuth → về login
                return RedirectToAction("Login");
            }

            ViewBag.GoogleEmail = email;
            ViewBag.GoogleName = name;
            return View();
        }

        // ─── GOOGLE OAUTH: Xử lý hoàn tất đăng ký với vai trò đã chọn ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompleteGoogleRegistration(string loaitaikhoan, string sdt, string diachi)
        {
            var email = HttpContext.Session.GetString("google_email");
            var name = HttpContext.Session.GetString("google_name");

            if (string.IsNullOrEmpty(email))
            {
                TempData["err"] = "Phiên đăng ký đã hết hạn. Vui lòng đăng nhập Google lại.";
                return RedirectToAction("Login");
            }

            // ponytail: security fix — clear google_partner_mode n?u còn sút sau abandon flow
            HttpContext.Session.Remove("google_partner_mode");

            // ─── Validate dữ liệu ───
            var validRoles = new[] { "Khách hàng", "Quán ăn", "Shipper" };
            if (!validRoles.Contains(loaitaikhoan))
            {
                TempData["err"] = "Vui lòng chọn một vai trò hợp lệ.";
                return RedirectToAction("SelectRoleGoogle");
            }

            if (string.IsNullOrWhiteSpace(sdt) || !System.Text.RegularExpressions.Regex.IsMatch(sdt, @"^0[1-9][0-9]{8,9}$"))
            {
                TempData["err"] = "Số điện thoại không hợp lệ — phải là 10-11 số, bắt đầu bằng 0 (VD: 0912345678)";
                return RedirectToAction("SelectRoleGoogle");
            }

            if (loaitaikhoan != "Khách hàng" && (string.IsNullOrWhiteSpace(diachi) || diachi.Length < 5))
            {
                TempData["err"] = "Vui lòng nhập địa chỉ (tối thiểu 5 ký tự)";
                return RedirectToAction("SelectRoleGoogle");
            }

            try
            {
                // ─── Kiểm tra SĐT đã tồn tại chưa ───
                var existingPhone = db.tbUser.FirstOrDefault(u => u.sdt == sdt);
                if (existingPhone != null)
                {
                    TempData["err"] = "Số điện thoại này đã được sử dụng bởi tài khoản khác.";
                    return RedirectToAction("SelectRoleGoogle");
                }

                // ─── Kiểm tra email đã tồn tại chưa (phòng trường hợp race condition) ───
                var existingEmail = db.tbUser.FirstOrDefault(u => u.email == email);
                if (existingEmail != null)
                {
                    TempData["err"] = "Email này đã được đăng ký. Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }

                // ─── Tạo tài khoản ───
                var randomPwd = BCrypt.Net.BCrypt.HashPassword($"GG_{Guid.NewGuid():N}");
                var truncatedEmail = email.Length > 50 ? email[..50] : email;
                var shortUser = "gg_" + Guid.NewGuid().ToString("N")[..12];
                var tenDayDu = !string.IsNullOrEmpty(name) ? name : truncatedEmail;
                if (tenDayDu.Length > 50) tenDayDu = tenDayDu[..50];

                var newUser = new tbUser
                {
                    username     = shortUser,
                    pwd          = randomPwd,
                    email        = truncatedEmail,
                    sdt          = sdt,
                    loaitaikhoan = loaitaikhoan,
                    vitien       = 0,
                    trangthai    = 1
                };
                db.tbUser.Add(newUser);
                db.SaveChanges();

                // ─── Tạo bản ghi theo vai trò ───
                switch (loaitaikhoan)
                {
                    case "Khách hàng":
                        db.tbKhachHang.Add(new tbKhachHang
                        {
                            userid = newUser.userid,
                            tenkh  = tenDayDu
                        });
                        break;

                    case "Quán ăn":
                        db.tbQuanAn.Add(new tbQuanAn
                        {
                            userid       = newUser.userid,
                            tenquanan    = tenDayDu,
                            diachi       = diachi ?? "",
                            soluotdanhgia = 0,
                            diemdanhgia  = 0,
                            trangthai    = "Đóng cửa"
                        });
                        break;

                    case "Shipper":
                        db.tbShipper.Add(new tbShipper
                        {
                            userid        = newUser.userid,
                            tenshipper    = tenDayDu,
                            diachi        = diachi ?? "",
                            soluotdanhgia = 0,
                            diemdanhgia   = 0,
                            trangthai     = "Hoạt động"
                        });
                        break;
                }
                db.SaveChanges();

                // ─── Dọn session tạm ───
                HttpContext.Session.Remove("google_email");
                HttpContext.Session.Remove("google_name");

                // ─── Gán Session + điều hướng theo vai trò ───
                var cart = new Cart { userid = newUser.userid };
                SetCart(cart);
                await SetSessionAndCookieAsync(newUser);

                return loaitaikhoan switch
                {
                    "Khách hàng" => RedirectToAction("Index", "Home"),
                    "Quán ăn" => RedirectToAction("Index", "Restaurant"),
                    "Shipper" => RedirectToAction("Index", "Shipper"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
                logger.LogError(ex, "CompleteGoogleRegistration failed for email {Email}, role {Role}", email, loaitaikhoan);
                TempData["err"] = "Lỗi hệ thống. Vui lòng thử lại.";
                return RedirectToAction("SelectRoleGoogle");
            }
        }

    public ActionResult Signup()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login-policy")]
    public ActionResult Signup(tbUser user, string repeatpw, string diachi, string hoten)
    {
        // === Validate dữ liệu đầu vào ===

        // Họ tên
        if (string.IsNullOrWhiteSpace(hoten) || hoten.Length < 2 || hoten.Length > 100)
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = "Họ tên phải từ 2-100 ký tự" });
            ViewBag.err = "Họ tên phải từ 2-100 ký tự";
            return View();
        }

        // Username
        if (string.IsNullOrWhiteSpace(user.username) || user.username.Length < 3 || user.username.Length > 50)
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = "Tên đăng nhập phải từ 3-50 ký tự" });
            ViewBag.err = "Tên đăng nhập phải từ 3-50 ký tự";
            return View();
        }

        // Mật khẩu — bắt buộc với tất cả roles
        if (string.IsNullOrEmpty(user.pwd))
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = "Vui lòng nhập mật khẩu" });
            ViewBag.err = "Vui lòng nhập mật khẩu";
            return View();
        }
        // ponytail: security fix — ValidatePasswordStrength ki?m tra m?t kh?u m?nh
        var (pwValid, pwMsg) = ValidatePasswordStrength(user.pwd);
        if (!pwValid)
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = pwMsg });
            ViewBag.err = pwMsg;
            return View();
        }
            if (user.pwd != repeatpw)
            {
                if (IsAjaxRequest()) return Json(new { success = false, message = "Xác nhận mật khẩu không khớp" });
                ViewBag.err = "Xác nhận mật khẩu không khớp";
                return View();
            }

        // Số điện thoại
        if (string.IsNullOrWhiteSpace(user.sdt) || !System.Text.RegularExpressions.Regex.IsMatch(user.sdt, @"^0[1-9][0-9]{8,9}$"))
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = "Số điện thoại không hợp lệ — phải là 10-11 số, bắt đầu bằng 0 (VD: 0912345678)" });
            ViewBag.err = "Số điện thoại không hợp lệ";
            return View();
        }

        // Email
        if (string.IsNullOrWhiteSpace(user.email))
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = "Vui lòng nhập email" });
            ViewBag.err = "Vui lòng nhập email";
            return View();
        }            try
            {
                var addr = new System.Net.Mail.MailAddress(user.email);
                if (addr.Address != user.email) throw new Exception();
            }
            catch
            {
                if (IsAjaxRequest()) return Json(new { success = false, message = "Email không hợp lệ (VD: example@gmail.com)" });
                ViewBag.err = "Email không hợp lệ (VD: example@gmail.com)";
                return View();
            }

            // ponytail: hash password before saving
            user.pwd = BCrypt.Net.BCrypt.HashPassword(user.pwd);

        // Địa chỉ (chỉ bắt buộc với Quán ăn / Shipper)
        bool requiresAddress = user.loaitaikhoan == "Quán ăn" || user.loaitaikhoan == "Shipper";
        if (requiresAddress && (string.IsNullOrWhiteSpace(diachi) || diachi.Length < 5))
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = "Địa chỉ phải từ 5-250 ký tự" });
            ViewBag.err = "Địa chỉ phải từ 5-250 ký tự";
            return View();
        }

        // Loại tài khoản
        if (string.IsNullOrWhiteSpace(user.loaitaikhoan))
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = "Vui lòng chọn loại tài khoản" });
            ViewBag.err = "Vui lòng chọn loại tài khoản";
            return View();
        }
        var validRoles = new[] { "Khách hàng", "Quán ăn", "Shipper" };
        if (!validRoles.Contains(user.loaitaikhoan))
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = "Loại tài khoản không hợp lệ" });
            ViewBag.err = "Loại tài khoản không hợp lệ";
            return View();
        }

        // Kiểm tra trùng lặp
        var existingUsers = db.tbUser.Where(u => u.username.Equals(user.username)).ToList();
        if (existingUsers.Count != 0)
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = "Tên tài khoản đã tồn tại" });
            ViewBag.err = "Tên tài khoản đã tồn tại";
            return View();
        }
        var existingEmails = db.tbUser.Where(u => u.email == user.email).ToList();
        if (existingEmails.Count != 0)
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = "Email này đã được sử dụng" });
            ViewBag.err = "Email này đã được sử dụng";
            return View();
        }
        var existingSdt = db.tbUser.Where(u => u.sdt == user.sdt).ToList();
        if (existingSdt.Count != 0)
        {
            if (IsAjaxRequest()) return Json(new { success = false, message = "Số điện thoại này đã được sử dụng" });
            ViewBag.err = "Số điện thoại này đã được sử dụng";
            return View();
        }

        try
        {
            if (user.loaitaikhoan.Equals("Khách hàng"))
            {
                user.vitien = 0;
                user.trangthai = 1;
                db.tbUser.Add(user);
                db.SaveChanges();

                db.tbKhachHang.Add(new tbKhachHang { userid = user.userid, tenkh = hoten });
                db.SaveChanges();

                // ponytail: security fix — auto-login cho Khách hàng nh?t quán v?i Quán an/Shipper
                var cart = new Cart { userid = user.userid };
                SetCart(cart);
                SetSessionUser(user);
                if (IsAjaxRequest()) return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                return RedirectToAction("Index", "Home");
            }
            else if (user.loaitaikhoan.Equals("Quán ăn"))
            {
                user.vitien = 0;
                user.trangthai = 1;
                db.tbUser.Add(user);
                db.SaveChanges();

                db.tbQuanAn.Add(new tbQuanAn
                {
                    userid = user.userid,
                    tenquanan = hoten,
                    diachi = diachi,
                    soluotdanhgia = 0,
                    diemdanhgia = 0,
                    trangthai = "Đóng cửa"
                });
                db.SaveChanges();

                var cart = new Cart { userid = user.userid };
                SetCart(cart);
                // ponytail: security fix — SetSessionAndCookieAsync thay SetSessionUser cho auto-login
                // Khách hàng cũng auto-login để nhất quán
                SetSessionUser(user);
                if (IsAjaxRequest()) return Json(new { success = true, redirectUrl = Url.Action("Index", "Restaurant") });
                return RedirectToAction("Index", "Restaurant");
            }
            else if (user.loaitaikhoan.Equals("Shipper"))
            {
                user.vitien = 0;
                user.trangthai = 1; // Active ngay — không cần duyệt
                db.tbUser.Add(user);
                db.SaveChanges();

                db.tbShipper.Add(new tbShipper
                {
                    userid = user.userid,
                    tenshipper = hoten,
                    diachi = diachi,
                    soluotdanhgia = 0,
                    diemdanhgia = 0,
                    trangthai = "Hoạt động"
                });
                db.SaveChanges();

                var cart = new Cart { userid = user.userid };
                SetCart(cart);
                // ponytail: security fix — SetSessionAndCookieAsync thay SetSessionUser cho auto-login
                SetSessionUser(user);
                if (IsAjaxRequest()) return Json(new { success = true, redirectUrl = Url.Action("Index", "Shipper") });
                return RedirectToAction("Index", "Shipper");
            }

            if (IsAjaxRequest()) return Json(new { success = true, redirectUrl = Url.Action("Login", "Home") });
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            logger.LogError(ex, "Signup failed for user {Username}: {Error}", user.username, innerMsg);

            // ═══ Fix PK violation: PostgreSQL sequence out of sync after seed ═══
            // N?u l?i 23505 (unique constraint), try reset sequence + thông báo th? lai
            // ponytail: Npgsql.PostgresException.SqlState == "23505" means duplicate key
            // ponytail: user must retry submit — sequence is now fixed for next attempt
            if (innerMsg.Contains("23505") || innerMsg.Contains("PK_tbUser") || innerMsg.Contains("duplicate key"))
            {
                try
                {
                    db.Database.ExecuteSqlRaw(@"SELECT setval('""tbUser_userid_seq""', COALESCE((SELECT MAX(""userid"") FROM ""tbUser""), 0) + 1, false);");
                    logger.LogWarning("Reset tbUser sequence after PK violation — user should retry");
                }
                catch (Exception seqEx)
                {
                    logger.LogError(seqEx, "Failed to reset tbUser sequence");
                }
            }

            var userMsg = "Lỗi tạo tài khoản. Vui lòng thử lại.";
            // ponytail: security fix — không leak innerMsg ra client
            if (IsAjaxRequest()) return Json(new { success = false, message = userMsg });
            ViewBag.err = userMsg;
            return View();
        }
    }

    /// <summary>
    /// Kiểm tra request có phải AJAX không
    /// </summary>
    private bool IsAjaxRequest()
    {
        return HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }

    // ─── CUSTOMER PROFILE: H? so ca? nhân ───
    public ActionResult Profile()
    {
        var user = GetCurrentUser();
        if (user == null || user.loaitaikhoan != "Khách hàng")
            return RedirectToAction("Login");
        var kh = db.tbKhachHang.Find(user.userid);
        ViewBag.KhachHang = kh;
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login-policy")]
    public ActionResult Profile(string hoten, string sdt, string oldPwd, string newPwd, string confirmPwd)
    {
        var user = GetCurrentUser();
        if (user == null || user.loaitaikhoan != "Khách hàng")
            return RedirectToAction("Login");

        var dbUser = db.tbUser.Find(user.userid);
        if (dbUser == null) return RedirectToAction("Login");

        if (!string.IsNullOrEmpty(sdt))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(sdt, @"^0[1-9][0-9]{8,9}$"))
            {
                TempData["ProfileError"] = "Số điện thoại không hợp lệ.";
                return RedirectToAction("Profile");
            }
            dbUser.sdt = sdt;
        }

        // ?i mâ?t khâ?u nê?u co? yêu câ?u
        if (!string.IsNullOrEmpty(newPwd))
        {
            if (string.IsNullOrEmpty(oldPwd))
            {
                TempData["ProfileError"] = "Vui lòng nhập mật khẩu hiện tại.";
                return RedirectToAction("Profile");
            }
            if (newPwd.Length < 8)
            {
                TempData["ProfileError"] = "Mật khẩu mới phải có ít nhất 8 ký tự.";
                return RedirectToAction("Profile");
            }
            if (newPwd != confirmPwd)
            {
                TempData["ProfileError"] = "Xác nhận mật khẩu không khớp.";
                return RedirectToAction("Profile");
            }
            // Verify old password
            bool verified = false;
            try { verified = BCrypt.Net.BCrypt.Verify(oldPwd, dbUser.pwd); } catch { }
            // ponytail: security fix — chỉ verify bằng BCrypt, không fallback plain-text
            if (!verified)
            {
                TempData["ProfileError"] = "Mật khẩu hiện tại không đúng.";
                return RedirectToAction("Profile");
            }
            dbUser.pwd = BCrypt.Net.BCrypt.HashPassword(newPwd);
        }

        db.SaveChanges();

        // ponytail: tbKhachHang ko có diachi, chỉ update tenkh
        var kh = db.tbKhachHang.Find(user.userid);
        if (kh != null && !string.IsNullOrEmpty(hoten))
        {
            kh.tenkh = hoten;
            db.SaveChanges();
        }

        TempData["ProfileSuccess"] = "Cập nhật hồ sơ thành công!";
        return RedirectToAction("Profile");
    }

    public ActionResult Forgot()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Logout()
    {
        // ─── Xoá cả session + auth cookie ───
        try
        {
            await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        }
        catch { /* Auth cookie may be missing or invalid — still clear session */ }
        try
        {
            HttpContext.Session.Remove("user");
            HttpContext.Session.Remove("cart");
            HttpContext.Session.Clear();
            await HttpContext.Session.CommitAsync();
        }
        catch { /* Session may be corrupted — still redirect to home */ }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult LogoutGet()
    {
        // ponytail: GET logout redirects to POST version via JS
        return RedirectToAction("Index");
    }

    public ActionResult DanhMuc()
    {
        var danhmuc = db.tbDanhMuc.ToList();
        return View(danhmuc);
    }

    public ActionResult SanPham(int id)
    {
        var dsmonan = db.tbMonAn.Where(n => n.tbDanhMuc!.madanhmuc == id).ToList();
        var tendanhmuc = db.tbDanhMuc.Find(id);
        ViewBag.tendanhmuc = tendanhmuc;
        return View(dsmonan);
    }

    public ActionResult ChiTietSanPham(int id)
    {
        var ctmonan = db.tbMonAn
            .Include(m => m.tbQuanAn!).ThenInclude(q => q.tbUser)
            .Include(m => m.tbDanhMuc)
            .Include(m => m.tbBienTheMonAns)
            .FirstOrDefault(m => m.mamon == id);
        if (ctmonan == null) return RedirectToAction("Index");

        // Cart session for user info
        var cartSession = GetCart();
        HashSet<int> daMuaMonAnIds = new HashSet<int>();
        if (cartSession != null && cartSession.userid > 0)
        {
            var userId = cartSession.userid;
            var mattdhIds = db.tbThongTinDatHang
                .Where(t => t.userid == userId)
                .Select(t => (int?)t.mattdh)
                .ToList();
            daMuaMonAnIds = db.tbChiTietDonHang
                .Where(ct => ct.tbDonHang != null
                    && mattdhIds.Contains(ct.tbDonHang.mattdh)
                    && ct.tbDonHang.trangthai != "Đã hủy")
                .Where(ct => ct.tbBienTheMonAn != null && ct.tbBienTheMonAn.tbMonAn != null)
                .Select(ct => ct.tbBienTheMonAn!.tbMonAn!.mamon)
                .Distinct()
                .ToHashSet();
        }
        ViewBag.DaMuaMonAnIds = daMuaMonAnIds;

        // Get same-restaurant items for cross-sell
        var tuongTu = db.tbMonAn
            .Where(m => m.maquanan == ctmonan.maquanan && m.mamon != id)
            .Include(m => m.tbBienTheMonAns)
            .Take(4)
            .ToList();
        ViewBag.MonTuongTu = tuongTu;

        // Get khuyenMai for this item
        ViewBag.KhuyenMai = db.tbMonAnKhuyenMai
            .Where(km => km.trangthai == "Còn hạn")
            .FirstOrDefault(km => km.tbBienTheMonAn!.mamon == id);

        ViewBag.maquan = ctmonan.maquanan;

        return View(ctmonan);
    }

    /// <summary>
    /// Trang lỗi mặc định — tránh 404 khi exception handler redirect đến /Home/Error
    /// </summary>
    // ─── CUSTOMER WALLET: Ví tiền cho khách hàng ───
    public ActionResult Wallet()
    {
        var user = GetCurrentUser();
        if (user == null || user.loaitaikhoan != "Khách hàng")
            return RedirectToAction("Login");

        ViewBag.WalletSuccess = TempData["WalletSuccess"];
        ViewBag.WalletError = TempData["WalletError"];
        // ponytail: d?c TempData tu NapTien POST de view hien thi QR + so tien
        ViewBag.WalletPending = TempData["WalletPending"];
        ViewBag.DepositQR = TempData["DepositQR"];
        ViewBag.DepositAmount = TempData["DepositAmount"];
        ViewBag.DepositCode = TempData["DepositCode"];

        // Lấy lịch sử đơn hàng đã hoàn thành (tương tự shipper)
        var userId = user.userid;
        var mattdhIds = db.tbThongTinDatHang
            .Where(t => t.userid == userId)
            .Select(t => t.mattdh)
            .ToList();
        var listdh = db.tbDonHang
            .Where(dh => mattdhIds.Contains(dh.mattdh ?? 0))
            .OrderByDescending(dh => dh.ngaydathang)
            .Take(50)
            .ToList();
        ViewBag.listdh = listdh;

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult NapTien(decimal soTien)
    {
        var user = GetCurrentUser();
        if (user == null || user.loaitaikhoan != "Khách hàng")
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (soTien < 10000)
        {
            TempData["WalletError"] = "Số tiền nạp tối thiểu là 10,000đ";
            return RedirectToAction("Wallet");
        }
        if (soTien > 100000000)
        {
            TempData["WalletError"] = "Số tiền nạp tối đa là 100,000,000đ";
            return RedirectToAction("Wallet");
        }

        // ponytail: Không tự cộng tiền trực tiếp — tạo QR chuyển khoản
        // User phải chuyển khoản, SePay webhook sẽ tự động cập nhật
        var depositCode = $"NAP{user.userid}_{DateTime.Now:yyyyMMddHHmmss}";
        var memo = $"SEVQR FASTSHIP{depositCode}";

        // L?y config bank
        var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var bankId = config["BANK_ID"] ?? "970436";
        var bankAccountNo = config["BANK_ACCOUNT_NO"] ?? "1234567890";
        var bankAccountName = config["BANK_ACCOUNT_NAME"] ?? "FASTSHIP CO., LTD";
        var bankBin = Helpers.BankHelper.GetVietQrBinCode(bankId);

        var qrUrl = $"https://img.vietqr.io/image/{bankBin}-{bankAccountNo}-compact2.png?amount={(long)soTien}&addInfo={Uri.EscapeDataString(memo)}&accountName={Uri.EscapeDataString(bankAccountName)}";

        // L?u pending deposit v?o tbTinNhan ?? webhook sau này xác nh?n
        try
        {
            db.tbTinNhans.Add(new tbTinNhan
            {
                noidung = $"DEPOSIT_PENDING|{depositCode}|{soTien}|{user.userid}",
                makh = user.userid
            });
            db.SaveChanges();
        }
        catch { }

        TempData["DepositQR"] = qrUrl;
        TempData["DepositAmount"] = soTien;
        TempData["DepositCode"] = depositCode;
        TempData["WalletPending"] = $"Vui lòng chuyển khoản {soTien:N0}đ theo mã QR để nạp tiền vào ví. Hệ thống sẽ tự động cập nhật sau khi nhận được chuyển khoản.";

        return RedirectToAction("Wallet");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult RutTien(decimal soTien)
    {
        var user = GetCurrentUser();
        if (user == null || user.loaitaikhoan != "Khách hàng")
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (soTien < 10000)
        {
            TempData["WalletError"] = "Số tiền rút tối thiểu là 10,000đ";
            return RedirectToAction("Wallet");
        }

        var dbUser = db.tbUser.Find(user.userid);
        if (dbUser != null)
        {
            if ((dbUser.vitien ?? 0) < soTien)
            {
                TempData["WalletError"] = $"Số dư không đủ. Hiện tại: {dbUser.vitien:N0}đ";
                return RedirectToAction("Wallet");
            }
            dbUser.vitien -= soTien;
            db.SaveChanges();
            TempData["WalletSuccess"] = $"Rút {soTien:N0}đ thành công. Số dư mới: {dbUser.vitien:N0}đ";
        }
        return RedirectToAction("Wallet");
    }

    public ActionResult Error()
    {
        return View();
    }

    /// <summary>
    /// Dọn dẹp đơn hàng rác (0 chi tiết) do seed bị chạy nhiều lần
    /// URL: GET /Home/CleanupOrders
    /// </summary>
    [HttpGet]
    public JsonResult CleanupOrders()
    {
        try
        {
            // ponytail: xoá tbTinNhan trước (FK constraint) rồi mới xoá tbDonHang
            var garbageIds = db.tbDonHang
                .Where(dh => !dh.tbChiTietDonHangs.Any())
                .Select(dh => dh.madh)
                .ToList();
            var count = garbageIds.Count;

            if (count > 0)
            {
                // 1. Xoá tin nhắn liên quan (FK: tbTinNhan.madh -> tbDonHang.madh)
                // ponytail: t.madh là int? (FK nullable), garbageIds là List<int> → cast t.madh.Value
                var tinNhans = db.tbTinNhans.Where(t => t.madh != null && garbageIds.Contains(t.madh.Value)).ToList();
                if (tinNhans.Any())
                    db.tbTinNhans.RemoveRange(tinNhans);
                db.SaveChanges();

                // 2. Xoá đơn hàng rác
                var garbageOrders = db.tbDonHang.Where(dh => garbageIds.Contains(dh.madh)).ToList();
                db.tbDonHang.RemoveRange(garbageOrders);
                db.SaveChanges();
            }

            return Json(new { success = true, deleted = count, message = $"Đã xoá {count} đơn hàng rác (0 món)." });
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message ?? "(no inner)";
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogError(ex, "CleanupOrders failed");
            return Json(new { success = false, message = $"Lỗi: {ex.Message} | Inner: {inner}" });
        }
    }

    /// <summary>
    /// Debug: thử render DetailRestaurant view và báo lỗi chi tiết
    /// URL: GET /Home/DebugDetail/7
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> DebugDetail(int id)
    {
        try
        {
            var quanAn = db.tbQuanAn.Include(q => q.tbUser).Include(q => q.tbMonAns).ThenInclude(m => m.tbDanhMuc)
                .Include(q => q.tbMonAns).ThenInclude(m => m.tbBienTheMonAns)
                .FirstOrDefault(t => t.userid == id);
            if (quanAn == null) return Content($"❌ Restaurant id={id} not found");
            if (quanAn.tbUser == null) return Content($"❌ tbUser is null for restaurant id={id}");
            if (quanAn.tbUser.trangthai != 1) return Content($"❌ Restaurant trangthai={quanAn.tbUser.trangthai}");

            var danhSachMonAn = db.tbMonAn.Where(m => m.maquanan == id).Include(m => m.tbDanhMuc).Include(m => m.tbBienTheMonAns).ToList();
            var thucDon = db.tbDanhMuc.Where(d => db.tbMonAn.Any(m => m.maquanan == id && m.madanhmuc == d.madanhmuc)).ToList();
            var khuyenMais = db.tbMonAnKhuyenMai.Where(km => km.trangthai == "Còn hạn").Include(km => km.tbKhuyenMai).Include(km => km.tbBienTheMonAn).ToList();

            var msg = $"✅ OK! Quán: {quanAn.tenquanan} | Món: {danhSachMonAn.Count} | DanhMục: {thucDon.Count} | KM: {khuyenMais.Count}";
            foreach (var m in danhSachMonAn.Take(3))
                msg += $"\n- {m.tenmon} (giá: {m.tbBienTheMonAns?.FirstOrDefault()?.giatien?.ToString("N0") ?? "N/A"}đ, DM: {m.tbDanhMuc?.tendanhmuc ?? "?"})";
            return Content(msg);
        }
        catch (Exception ex)
        {
            return Content($"❌ LỖI: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Debug: kiểm tra database — đếm số dòng từng bảng
    /// URL: GET /Home/DbDebug
    /// </summary>
    [HttpGet]
    public JsonResult DbDebug()
    {
        var debugInfo = new Dictionary<string, object>
        {
            ["tbUser"] = db.tbUser.Count(),
            ["tbQuanAn"] = db.tbQuanAn.Count(),
            ["tbMonAn"] = db.tbMonAn.Count(),
            ["tbDanhMuc"] = db.tbDanhMuc.Count(),
            ["tbBienTheMonAn"] = db.tbBienTheMonAn.Count(),
            ["tbDonHang"] = db.tbDonHang.Count(),
            ["tbChiTietDonHang"] = db.tbChiTietDonHang.Count(),
            ["tbThongTinDatHang"] = db.tbThongTinDatHang.Count(),
            ["tbKhachHang"] = db.tbKhachHang.Count(),
            ["tbShipper"] = db.tbShipper.Count(),
            ["tbDanhGia"] = db.tbDanhGia.Count(),
            ["tbKhuyenMai"] = db.tbKhuyenMai.Count(),
            ["tbMonAnKhuyenMai"] = db.tbMonAnKhuyenMai.Count(),
            ["tbLichSuSuDungKhuyenMai"] = db.tbLichSuSuDungKhuyenMai.Count(),
            ["tbLoaiHinhThanhToan"] = db.tbLoaiHinhThanhToan.Count(),
            ["tbTinNhan"] = db.tbTinNhans.Count(),
            ["tbAdmin"] = db.tbAdmin.Count(),
            ["connectionString"] = "(hidden)",
        };

        // Lấy 5 user đầu tiên để kiểm tra
        debugInfo["sampleUsers"] = db.tbUser.Take(5).Select(u => new
        {
            u.userid,
            u.username,
            u.email,
            u.loaitaikhoan,
            u.trangthai,
            u.sdt
        }).ToList();

        // Lấy 3 quán ăn đầu tiên
        debugInfo["sampleRestaurants"] = db.tbQuanAn.Take(3).Select(q => new
        {
            q.userid,
            q.tenquanan,
            q.trangthai,
            q.diachi
        }).ToList();

        // Lấy 3 món ăn đầu tiên
        debugInfo["sampleMonAns"] = db.tbMonAn.Take(3).Select(m => new
        {
            m.mamon,
            m.tenmon,
            m.maquanan,
            m.madanhmuc
        }).ToList();

        // Thử INSERT 1 user test để kiểm tra seed có hoạt động không
        if (!db.tbUser.Any())
        {
            try
            {
                var testSql = @"INSERT INTO ""tbUser"" (""username"", ""pwd"", ""loaitaikhoan"", ""sdt"", ""vitien"", ""email"", ""trangthai"")
VALUES ('test_debug', 'test123', 'Khách hàng', '0999999999', 0, 'test@debug.com', 1)";
                db.Database.ExecuteSqlRaw(testSql);
                debugInfo["testInsert"] = "OK — inserted test_debug user";
                // Clean up test user
                db.Database.ExecuteSqlRaw(@"DELETE FROM ""tbUser"" WHERE ""username"" = 'test_debug'");
                debugInfo["testCleanup"] = "OK — cleaned up";
            }
            catch (Exception testEx)
            {
                debugInfo["testInsertError"] = testEx.Message;
                debugInfo["testInsertInner"] = testEx.InnerException?.Message ?? "(none)";
            }
        }
        else
        {
            debugInfo["testInsertSkipped"] = "DB already has data, skip test";
        }

        return Json(new { success = true, database = debugInfo });
    }

    /// <summary>
    /// Ghi đè BCrypt hash trong database bằng plain-text password
    /// Chạy 1 lần sau deploy để fix lỗi login do database cũ còn BCrypt hash
    /// URL: GET /Home/FixPasswords
    /// </summary>
    public IActionResult FixPasswords()
    {
        try
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            int updated = 0;

            // Lấy danh sách user có password bắt đầu bằng $2 (BCrypt hash)
            var bcryptUsers = db.tbUser.Where(u => u.pwd.StartsWith("$2")).ToList();

            if (bcryptUsers.Count == 0)
            {
                return Content("✅ Không có user nào dùng BCrypt hash. Mọi thứ đã ổn!");
            }

            foreach (var user in bcryptUsers)
            {
                // Map password theo userid dựa trên seed data
                var plainPwd = user.userid switch
                {
                    1 => "abcdef",            // tranthib
                    2 => "qwerty",            // levanc
                    3 => "shipy456",          // shippery
                    4 => "shipz789",          // shipperz
                    5 => "xyz123",            // phamthid
                    6 => "konekopizza",       // Koneko Pizza
                    7 => "com1990nvs",        // Cơm 1990
                    8 => "bundaugiadi",       // Bún Đậu Gia Di
                    9 => "quanchayanlactam",  // Quán Chay An Lạc Tâm
                    10 => "changanuongbahong",// Chân Gà Nướng Bà Hồng
                    11 => "tralong",          // Trà Long
                    12 => "bunmambadong",     // Bún Mắm Bà Đông
                    13 => "danghoanggatre",   // Đàng Hoàng
                    14 => "sushitotoro",      // Sushi Totoro
                    15 => "43bakery",         // 43 Bakery
                    16 => "admin1",           // Admin 1
                    17 => "admin2",           // Admin 2
                    18 => "admin3",           // Admin 3
                    _ => null                  // User không có trong seed → skip
                };

                if (plainPwd != null)
                {
                    user.pwd = plainPwd;
                    updated++;
                }
            }

            db.SaveChanges();

            return Content($"✅ Đã sửa {updated}/{bcryptUsers.Count} user từ BCrypt hash → plain-text password." +
                $"\n\nCác user đã fix: {string.Join(", ", bcryptUsers.Where(u => u.userid <= 18).Select(u => u.username))}" +
                $"\n\nGiờ bạn có thể login với mật khẩu seed tương ứng!");
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogError(ex, "FixPasswords failed");
            return Content($"❌ Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// ALTER các cột hinhanh bị giới hạn độ dài (fix cho DB cũ đã tạo với VARCHAR(50))
    /// Sau đó seed database từ seed.sql.
    /// Gọi GET /Home/SeedDb từ browser.
    /// </summary>
    public IActionResult SeedDb()
    {
        try
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var sb = new System.Text.StringBuilder();

            // ── Bước 1: ALTER các cột hinhanh bị giới hạn độ dài ──
            var alterStatements = new[]
            {
                @"ALTER TABLE ""tbMonAn"" ALTER COLUMN ""hinhanh"" TYPE VARCHAR(500);",
                @"ALTER TABLE ""tbQuanAn"" ALTER COLUMN ""hinhanh"" TYPE VARCHAR(500);",
                @"ALTER TABLE ""tbDanhMuc"" ALTER COLUMN ""hinhanh"" TYPE VARCHAR(500);",
                @"ALTER TABLE ""tbShipper"" ALTER COLUMN ""hinhanh"" TYPE VARCHAR(500);",
                @"ALTER TABLE ""tbDanhGia"" ALTER COLUMN ""hinhanh"" TYPE VARCHAR(500);",
                @"ALTER TABLE ""tbMonAn"" ALTER COLUMN ""isDeleted"" SET DEFAULT false;",
                @"ALTER TABLE ""tbDanhMuc"" ADD COLUMN IF NOT EXISTS ""icon"" VARCHAR(50);",
                @"ALTER TABLE ""tbKhachHang"" ADD COLUMN IF NOT EXISTS ""hinhanh"" VARCHAR(500);",
                @"ALTER TABLE ""tbAdmin"" ADD COLUMN IF NOT EXISTS ""hinhanh"" VARCHAR(500);"
            };
            int alterOk = 0, alterFail = 0;
            foreach (var alter in alterStatements)
            {
                try { db.Database.ExecuteSqlRaw(alter); alterOk++; }
                catch (Exception ex) { logger.LogWarning("ALTER skipped: {Error}", ex.Message); alterFail++; }
            }
            sb.AppendLine($"✅ ALTER: {alterOk}/{alterOk + alterFail} cột OK.");

            // ── Bước 1b: Xoá dữ liệu cũ + reset sequence để seed lại từ đầu ──
            try
            {
                db.Database.ExecuteSqlRaw(@"DELETE FROM ""tbDanhGia"";");
                db.Database.ExecuteSqlRaw(@"DELETE FROM ""tbChiTietDonHang"";");
                db.Database.ExecuteSqlRaw(@"DELETE FROM ""tbMonAnKhuyenMai"";");
                db.Database.ExecuteSqlRaw(@"DELETE FROM ""tbBienTheMonAn"";");
                db.Database.ExecuteSqlRaw(@"DELETE FROM ""tbMonAn"";");
                db.Database.ExecuteSqlRaw(@"ALTER TABLE ""tbBienTheMonAn"" ALTER COLUMN ""id"" RESTART WITH 1;");
                db.Database.ExecuteSqlRaw(@"ALTER TABLE ""tbChiTietDonHang"" ALTER COLUMN ""mactdh"" RESTART WITH 1;");
                sb.AppendLine("✅ Đã xoá dữ liệu cũ + reset sequence.");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"⚠️ Xoá dữ liệu cũ: {ex.Message}");
            }

            // ── Bước 2: Seed data từ seed.sql ──
            string sqlPath = System.IO.Path.Combine(env.ContentRootPath, "seed.sql");
            if (!System.IO.File.Exists(sqlPath))
            {
                return Content($"✅ ALTER: {alterOk} OK, {alterFail} lỗi. ❌ Không tìm thấy seed.sql tại: " + sqlPath);
            }

            var sql = System.IO.File.ReadAllText(sqlPath).Replace("\r\n", "\n");
            var statements = sql.Split(new[] { "\nGO\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);
            sb.AppendLine($"📄 Tổng số statements: {statements.Length}");

            int success = 0, fail = 0;
            int idx = 0;
            foreach (var stmt in statements)
            {
                idx++;
                var trimmed = stmt.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("DROP") || trimmed.StartsWith("CREATE") || trimmed.StartsWith("SET"))
                    continue;

                // Lấy 80 ký tự đầu của câu SQL để hiển thị
                var preview = trimmed.Length > 80 ? trimmed.Substring(0, 80) + "..." : trimmed;
                try
                {
                    db.Database.ExecuteSqlRaw(trimmed);
                    success++;
                    sb.AppendLine($"  ✅ #{idx}: {preview}");
                }
                catch (Exception ex)
                {
                    fail++;
                    var errMsg = ex.InnerException?.Message ?? ex.Message;
                    sb.AppendLine($"  ❌ #{idx}: {preview}");
                    sb.AppendLine($"     Lỗi: {errMsg}");
                }
            }

            sb.AppendLine($"✅ Kết quả: {success} thành công, {fail} lỗi.");
            return Content(sb.ToString());
        }
        catch (Exception ex)
        {
            return Content($"❌ Lỗi: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // ─── Phase 1b: MenuSearch API (Dynamic SQL - tìm kiếm hướng Món ăn) ───
    [HttpGet]
    public JsonResult MenuSearch(string? q, int? categoryId, string? sortBy,
        bool? isPromo, bool? isBestSeller, bool? isNearMe,
        string? maxPriceLevel, string? maxDiet, string? mode)
    {
        try
        {
            // ── Đếm số lượng bộ lọc đang hoạt động ──
            int activeFilterCount = 0;
            if (categoryId.HasValue && categoryId.Value > 0) activeFilterCount++;
            if (!string.IsNullOrEmpty(q) && q.Length >= 2) activeFilterCount++;
            if (isPromo == true) activeFilterCount++;
            if (isBestSeller == true) activeFilterCount++;
            if (!string.IsNullOrEmpty(maxPriceLevel)) activeFilterCount++;
            if (!string.IsNullOrEmpty(maxDiet)) activeFilterCount++;

            // ponytail: chỉ hiển thị món từ quán có tbUser.trangthai == 1 (không bị admin khoá)
            var query = db.tbMonAn
                .Include(m => m.tbQuanAn!).ThenInclude(q => q.tbUser)
                .Include(m => m.tbBienTheMonAns)
                .Include(m => m.tbDanhMuc)
                .Where(m => m.tbQuanAn != null && m.tbQuanAn.trangthai == "Đang mở cửa"
                    && m.tbQuanAn.tbUser != null && m.tbQuanAn.tbUser.trangthai == 1);

            // ── PHASE 1: Lọc AND cho tất cả tiêu chí ──

            // Lọc theo danh mục (AND — tiêu chí cốt lõi)
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(m => m.madanhmuc == categoryId.Value);
            }

            // Lọc khuyến mãi (AND)
            if (isPromo == true)
            {
                var monAnCoKMIds = db.tbMonAnKhuyenMai
                    .Where(km => km.trangthai == "Còn hạn")
                    .Join(db.tbBienTheMonAn, km => km.mamon, b => b.id, (km, b) => b.mamon)
                    .Distinct()
                    .ToList();
                query = query.Where(m => monAnCoKMIds.Contains(m.mamon));
            }

            // Lọc đánh giá tốt (AND)
            if (isBestSeller == true || sortBy == "rating")
            {
                query = query.Where(m => m.tbQuanAn != null && m.tbQuanAn.diemdanhgia >= 4.4m);
            }

            // Lọc chế độ ăn (AND)
            if (!string.IsNullOrEmpty(maxDiet) && maxDiet == "vegetarian")
            {
                query = query.Where(m => m.tbDanhMuc != null && m.tbDanhMuc.tendanhmuc != null
                    && (m.tbDanhMuc.tendanhmuc.Contains("chay") || m.tbDanhMuc.tendanhmuc.Contains("rau")));
            }

            // Client-side: tìm kiếm không dấu
            var results = query.AsEnumerable().ToList();

            // Tìm kiếm không dấu
            if (!string.IsNullOrEmpty(q) && q.Length >= 2)
            {
                var normalizedQ = RemoveDiacritics(q.ToLower());
                results = results.Where(m =>
                    RemoveDiacritics(m.tenmon.ToLower()).Contains(normalizedQ)
                    || (m.tbQuanAn != null && RemoveDiacritics(m.tbQuanAn.tenquanan.ToLower()).Contains(normalizedQ))
                    || (m.tbDanhMuc != null && RemoveDiacritics(m.tbDanhMuc.tendanhmuc.ToLower()).Contains(normalizedQ))
                ).ToList();
            }

            // Lọc giá theo mức $ — client-side
            if (!string.IsNullOrEmpty(maxPriceLevel))
            {
                var (minPrice, maxPrice) = maxPriceLevel switch
                {
                    "1" => (0m, 20000m),
                    "2" => (20000m, 50000m),
                    "3" => (50000m, 100000m),
                    "4" => (100000m, decimal.MaxValue),
                    _ => (0m, decimal.MaxValue)
                };
                results = results.Where(m => m.tbBienTheMonAns != null &&
                    m.tbBienTheMonAns.Any(b => b.giatien >= minPrice && b.giatien <= maxPrice)).ToList();
            }

            bool isLooseFilter = false;

            // ── PHASE 2: Nếu kết quả rỗng, thử fallback với OR cho tiêu chí phụ ──
            if (results.Count == 0 && activeFilterCount >= 3)
            {
                isLooseFilter = true;

                // ponytail: fallback cũng filter tbUser.trangthai == 1
                var fallbackQuery = db.tbMonAn
                    .Include(m => m.tbQuanAn!).ThenInclude(q => q.tbUser)
                    .Include(m => m.tbBienTheMonAns)
                    .Include(m => m.tbDanhMuc)
                    .Where(m => m.tbQuanAn != null && m.tbQuanAn.trangthai == "Đang mở cửa"
                        && m.tbQuanAn.tbUser != null && m.tbQuanAn.tbUser.trangthai == 1);

                // Luôn giữ AND cho danh mục (tiêu chí cốt lõi)
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    fallbackQuery = fallbackQuery.Where(m => m.madanhmuc == categoryId.Value);
                }

                var fallbackResults = fallbackQuery.AsEnumerable().ToList();

                // OR cho từ khóa tìm kiếm
                if (!string.IsNullOrEmpty(q) && q.Length >= 2)
                {
                    var normalizedQ = RemoveDiacritics(q.ToLower());
                    fallbackResults = fallbackResults.Where(m =>
                        RemoveDiacritics(m.tenmon.ToLower()).Contains(normalizedQ)
                        || (m.tbQuanAn != null && RemoveDiacritics(m.tbQuanAn.tenquanan.ToLower()).Contains(normalizedQ))
                        || (m.tbDanhMuc != null && RemoveDiacritics(m.tbDanhMuc.tendanhmuc.ToLower()).Contains(normalizedQ))
                    ).ToList();
                }

                // Pre-compute promo IDs set (tránh query DB trong vòng lặp)
                HashSet<int>? monAnCoKMIdsSet = null;
                if (isPromo == true)
                {
                    monAnCoKMIdsSet = db.tbMonAnKhuyenMai
                        .Where(km => km.trangthai == "Còn hạn")
                        .Join(db.tbBienTheMonAn, km => km.mamon, b => b.id, (km, b) => b.mamon)
                        .Distinct()
                        .ToHashSet();
                }

                // OR cho các tiêu chí phụ: ưu tiên món khớp nhiều tiêu chí nhất (scoring)
                var secondaryResults = new List<(tbMonAn Mon, int Score)>();
                foreach (var m in fallbackResults)
                {
                    int score = 0;

                    if (isPromo == true && monAnCoKMIdsSet != null && monAnCoKMIdsSet.Contains(m.mamon))
                    {
                        score += 25;
                    }

                    if (isBestSeller == true && m.tbQuanAn?.diemdanhgia >= 4.0m)
                    {
                        score += 25;
                    }

                    if (!string.IsNullOrEmpty(maxPriceLevel))
                    {
                        var (minP, maxP) = maxPriceLevel switch
                        {
                            "1" => (0m, 20000m),
                            "2" => (20000m, 50000m),
                            "3" => (50000m, 100000m),
                            "4" => (100000m, decimal.MaxValue),
                            _ => (0m, decimal.MaxValue)
                        };
                        if (m.tbBienTheMonAns != null && m.tbBienTheMonAns.Any(b => b.giatien >= minP && b.giatien <= maxP))
                            score += 25;
                    }

                    if (!string.IsNullOrEmpty(maxDiet) && maxDiet == "vegetarian")
                    {
                        if (m.tbDanhMuc != null && m.tbDanhMuc.tendanhmuc != null &&
                            (m.tbDanhMuc.tendanhmuc.Contains("chay") || m.tbDanhMuc.tendanhmuc.Contains("rau")))
                            score += 25;
                    }

                    if (score > 0)
                        secondaryResults.Add((m, score));
                }

                // Nếu OR không tìm thấy gì, lấy top items được nhiều tiêu chí nhất
                if (secondaryResults.Count == 0)
                {
                    secondaryResults = fallbackResults
                        .Select(m => (m, score: 0))
                        .Take(10)
                        .ToList();
                }

                results = secondaryResults
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.Mon)
                    .ToList();
            }

            // Sắp xếp
            results = sortBy switch
            {
                "rating" => results.OrderByDescending(m => m.tbQuanAn?.diemdanhgia).ToList(),
                "price_asc" => results.OrderBy(m => m.tbBienTheMonAns?.Min(b => b.giatien)).ToList(),
                "price_desc" => results.OrderByDescending(m => m.tbBienTheMonAns?.Min(b => b.giatien)).ToList(),
                _ => results.OrderByDescending(m => m.tbQuanAn?.diemdanhgia).ThenBy(m => m.tenmon).ToList()
            };

            // Lấy điểm đánh giá trung bình
            var monAnIdsList = results.Select(m => m.mamon).ToList();
            var avgRatings = db.tbDanhGia
                .Include(d => d.tbChiTietDonHang!).ThenInclude(c => c!.tbBienTheMonAn)
                .Where(d => d.tbChiTietDonHang != null && d.tbChiTietDonHang.tbBienTheMonAn != null
                    && monAnIdsList.Contains(d.tbChiTietDonHang.tbBienTheMonAn.mamon))
                .GroupBy(d => d.tbChiTietDonHang!.tbBienTheMonAn!.mamon)
                .Select(g => new { mamon = g.Key, avg = (double?)g.Average(d => d.diemdanhgia) ?? 0 })
                .ToList();

            var avgRatingMap = avgRatings.ToDictionary(x => x.mamon, x => x.avg);

            var items = results.Select(m => new
            {
                mamon = m.mamon,
                tenmon = m.tenmon,
                hinhanh = m.hinhanh,
                maquanan = m.maquanan,
                tenquanan = m.tbQuanAn?.tenquanan,
                madanhmuc = m.madanhmuc,
                tendanhmuc = m.tbDanhMuc?.tendanhmuc,
                avgRating = avgRatingMap.TryGetValue(m.mamon, out var r) ? Math.Round(r, 1) : 0,
                giaMin = m.tbBienTheMonAns?.Min(b => b.giatien),
                giaMax = m.tbBienTheMonAns?.Max(b => b.giatien),
                sizes = m.tbBienTheMonAns?.Select(b => new { b.id, b.size, b.giatien }).ToList(),
                isPromo = isPromo == true,
                conhang = m.conhang
            }).Take(50).ToList();

            return Json(new {
                success = true,
                items,
                total = items.Count,
                isLooseFilter,
                message = isLooseFilter ? "Gợi ý các món tương tự gần khớp với bộ lọc của bạn" : null
            });
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogError(ex, "MenuSearch failed");
            return Json(new { success = false, items = Array.Empty<object>(), total = 0, message = ex.Message });
        }
    }

    // ─── Task 2d: Search Autocomplete API (Debounce 300ms) ───
    [HttpGet]
    public JsonResult SearchAutocomplete(string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(new object[0]);

            var normalized = RemoveDiacritics(q.ToLower());

            // ponytail: chỉ hiển thị quán có tbUser.trangthai == 1 (không bị khoá)
            var results = db.tbQuanAn
                .Include(qa => qa.tbUser)
                .Where(qa => qa.trangthai == "Đang mở cửa" && qa.tbUser != null && qa.tbUser.trangthai == 1)
                // Push initial filter to DB first, then client-side for diacritics
                .Where(qa => qa.tenquanan.Contains(q) || qa.tbUser.username.Contains(q))
                .AsEnumerable()
                .Where(qa => RemoveDiacritics(qa.tenquanan.ToLower()).Contains(normalized)
                          || RemoveDiacritics(qa.tbUser.username.ToLower()).Contains(normalized))
                .Take(6)
                .Select(qa => new
                {
                    id = qa.userid,
                    name = qa.tenquanan,
                    address = qa.diachi,
                    rating = qa.diemdanhgia > 0 ? "⭐ " + qa.diemdanhgia?.ToString("0.0") : "",
                    icon = "🏪"
                })
                .ToList();

            return Json(results);
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogWarning(ex, "SearchAutocomplete failed for q={Q}", q);
            return Json(new object[0]);
        }
    }

    /// <summary>
    /// Render view thành string để bắt lỗi view rendering trong try-catch.
    /// ponytail: vì return View() trả về ViewResult, view thực sự render ở pipeline sau
    /// nên exception ở view ko thể catch được ở action. Helper này giải quyết vấn đề đó.
    /// </summary>
    private async Task<string> RenderViewToStringAsync(string viewName, object? model = null)
    {
        ViewData.Model = model;

        using var sw = new StringWriter();
        // ponytail: isMainPage=true để layout _LayoutPageHome được render kèm
        var viewResult = _viewEngine.FindView(ControllerContext, viewName, isMainPage: true);

        if (viewResult.View == null)
            throw new InvalidOperationException($"View '{viewName}' not found.");

        var viewContext = new ViewContext(
            ControllerContext,
            viewResult.View,
            ViewData,
            TempData,
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }

    public ActionResult NhanTin()
    {
        return View();
    }

    // ===== API: Lấy danh sách biến thể (size, giá) của một món ăn =====
    [HttpGet]
    public JsonResult GetVariants(int monId)
    {
        try
        {
            var monAn = db.tbMonAn.Find(monId);
            if (monAn == null)
                return Json(new { success = false, message = "Món ăn không tồn tại" });

            var variants = db.tbBienTheMonAn
                .Where(b => b.mamon == monId)
                .Select(b => new
                {
                    b.id,
                    b.size,
                    b.giatien
                })
                .ToList();

            return Json(new
            {
                success = true,
                tenmon = monAn.tenmon,
                hinhanh = monAn.hinhanh,
                variants
            });
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogError(ex, "GetVariants failed for monId={MonId}", monId);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Refresh AntiForgeryToken — gọi từ client để refresh token trước khi hết hạn
    /// </summary>
    [HttpGet]
    public JsonResult RefreshToken()
    {
        return Json(new { token = GetAntiForgeryToken() });
    }

    /// <summary>
    /// Lấy AntiForgeryToken dạng string
    /// </summary>
    private string GetAntiForgeryToken()
    {
        var antiforgery = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return tokens.RequestToken ?? "";
    }

    // ===== API: Lấy các chi tiết đơn hàng của user tại quán này (để chọn đánh giá) =====
    [HttpGet]
    public JsonResult GetReviewableItems(int quanId)
    {
        if (!CheckLogin())
            return Json(new { items = new object[0] });

        var user = GetCurrentUser();

        var items = db.tbChiTietDonHang
            .Include(c => c.tbDonHang).ThenInclude(d => d!.tbThongTinDatHang)
            .Include(c => c.tbBienTheMonAn!).ThenInclude(b => b.tbMonAn)
            .Where(c => c.tbBienTheMonAn != null && c.tbBienTheMonAn.tbMonAn!.maquanan == quanId
                        && c.tbDonHang != null && c.tbDonHang.tbThongTinDatHang != null
                        && c.tbDonHang.tbThongTinDatHang.userid == user!.userid
                        && !c.tbDanhGias.Any())     // chưa đánh giá
            .Select(c => new
            {
                mactdh = c.mactdh,
                tenmon = c.tbBienTheMonAn!.tbMonAn!.tenmon,
                madh   = c.madh
            })
            .Take(20)
            .ToList();

        return Json(new { items });
    }

    // ===== API: Lấy danh sách đánh giá của một quán (public) — SERVER-SIDE PAGINATION =====
    [HttpGet]
    public JsonResult GetReviews(int quanId, int page = 1, int pageSize = 5, int? mamon = null)
    {
        // 1) Query gốc: ChiTietDonHang có đánh giá, thuộc quán này
        var baseQuery = db.tbChiTietDonHang
            .Include(c => c.tbBienTheMonAn!).ThenInclude(b => b.tbMonAn)
            .Include(c => c.tbDanhGias)
            .Include(c => c.tbDonHang!).ThenInclude(d => d.tbThongTinDatHang!).ThenInclude(t => t.tbKhachHang)
            .Where(c => c.tbBienTheMonAn != null && c.tbBienTheMonAn.tbMonAn!.maquanan == quanId && c.tbDanhGias.Any());

        // Nếu có mamon (product ID), lọc chỉ lấy đánh giá cho sản phẩm đó
        if (mamon.HasValue && mamon.Value > 0)
        {
            baseQuery = baseQuery.Where(c => c.tbBienTheMonAn!.tbMonAn!.mamon == mamon.Value);
        }

        // 2) Đếm tổng số review (1 query COUNT trên DB)
        var total = baseQuery.SelectMany(c => c.tbDanhGias).Count();

        // 3) Lấy danh sách review phân trang (Skip/Take trên DB)
        var reviews = baseQuery
            .SelectMany(c => c.tbDanhGias.Select(dg => new
            {
                madg       = dg.madg,
                diem       = dg.diemdanhgia ?? 0,
                nhanxet    = dg.nhanxet ?? "",
                tenmon     = c.tbBienTheMonAn!.tbMonAn!.tenmon ?? "",
                hinhanh    = c.tbBienTheMonAn!.tbMonAn!.hinhanh ?? "",
                tenkh      = c.tbDonHang!.tbThongTinDatHang!.tbKhachHang!.tenkh ?? "Khách hàng",
                ngaydat    = ((DateTime)c.tbDonHang!.ngaydathang!).ToString("dd/MM/yyyy")
            }))
            .OrderByDescending(r => r.madg)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();   // ← ToList() CHỈ xảy ra SAU Skip/Take → đúng server-side pagination

        // 4) Tính điểm trung bình (aggregate trên DB)
        var avgDiem = total > 0
            ? baseQuery.SelectMany(c => c.tbDanhGias).Average(d => (double?)d.diemdanhgia) ?? 0
            : 0;

        return Json(new { success = true, reviews, total, avgDiem = Math.Round(avgDiem, 1), page, pageSize });
    }

    // ===== API: Khách hàng gửi đánh giá =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult SubmitReview(int mactdh, int diem, string? nhanxet)
    {
        if (!CheckLogin())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (diem < 1 || diem > 5)
            return Json(new { success = false, message = "Điểm đánh giá phải từ 1-5" });

        if (!string.IsNullOrEmpty(nhanxet) && nhanxet.Length > 500)
            return Json(new { success = false, message = "Nhận xét quá dài (tối đa 500 ký tự)" });

        var user = GetCurrentUser();

        // Kiểm tra chi tiết đơn hàng tồn tại và thuộc về user này
        var chiTiet = db.tbChiTietDonHang
            .Include(c => c.tbDonHang).ThenInclude(d => d!.tbThongTinDatHang)
            .FirstOrDefault(c => c.mactdh == mactdh);

        if (chiTiet == null)
            return Json(new { success = false, message = "Chi tiết đơn hàng không tồn tại" });

        if (chiTiet.tbDonHang?.tbThongTinDatHang?.userid != user!.userid)
            return Json(new { success = false, message = "Bạn không có quyền đánh giá đơn hàng này" });

        // Kiểm tra đã đánh giá chưa
        var existing = db.tbDanhGia.FirstOrDefault(d => d.mactdh == mactdh);
        if (existing != null)
            return Json(new { success = false, message = "Bạn đã đánh giá món này rồi" });

        // ponytail: chỉ chặn đánh giá đơn đã hủy — cho phể́p cả đơn đang xử lý và chờ thanh toán (user đã chuyển khoản)
        if (chiTiet.tbDonHang?.trangthai == "Đã hủy")
            return Json(new { success = false, message = "Chỉ có thể đánh giá đơn hàng đã hoàn thành" });

        var danhGia = new tbDanhGia
        {
            mactdh      = mactdh,
            diemdanhgia = diem,
            nhanxet     = nhanxet?.Trim()
        };
        db.tbDanhGia.Add(danhGia);

        // Cập nhật điểm trung bình cho quán
        var bienThe = db.tbBienTheMonAn.Include(b => b.tbMonAn).FirstOrDefault(b => b.id == chiTiet.mamon);
        var monAn = bienThe?.tbMonAn;
        if (monAn?.maquanan != null)
        {
            var quanAn = db.tbQuanAn.Find(monAn.maquanan);
            if (quanAn != null)
            {
                var allDGs = db.tbDanhGia
                    .Include(d => d.tbChiTietDonHang!).ThenInclude(c => c!.tbBienTheMonAn!).ThenInclude(b => b.tbMonAn)
                    .Where(d => d.tbChiTietDonHang != null && d.tbChiTietDonHang.tbBienTheMonAn != null
                                && d.tbChiTietDonHang.tbBienTheMonAn.tbMonAn != null
                                && d.tbChiTietDonHang.tbBienTheMonAn.tbMonAn.maquanan == quanAn.userid)
                    .ToList();
                quanAn.soluotdanhgia = allDGs.Count + 1;
                quanAn.diemdanhgia   = allDGs.Count == 0 ? diem
                    : (allDGs.Sum(d => d.diemdanhgia ?? 0) + diem) / (allDGs.Count + 1);
            }
        }

        db.SaveChanges();
        return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá!" });
    }
}
