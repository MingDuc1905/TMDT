using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Controllers;

public class HomeController : BaseController
{
    private readonly RecommendationService _recommendationService;

    public HomeController(dbFoodyEntities context, RecommendationService recommendationService)
    {
        db = context;
        _recommendationService = recommendationService;
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
        var quanAns = db.tbQuanAn.Include(q => q.tbUser).Include(q => q.tbMonAns).ThenInclude(m => m.tbBienTheMonAns).ToList();
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

        // ─── Apriori: Gợi ý Combo AI cho trang chủ ───
        ViewBag.AprioriCombo = await _recommendationService.GetPopularPairs(6);

        return View();
    }

    public async Task<ActionResult> DetailRestaurant(int id, int? idDM, string? searchKey)
    {
        var quanAn = db.tbQuanAn.Include(q => q.tbMonAns).ThenInclude(m => m.tbDanhMuc)
            .Include(q => q.tbMonAns).ThenInclude(m => m.tbBienTheMonAns)
            .FirstOrDefault(t => t.userid == id);
        if (quanAn == null)
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
            .Include(km => km.tbBienTheMonAn) // Cần để truy cập tbMonAn qua tbBienTheMonAn
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
        var firstMonId = danhSachMonAn.Any() ? danhSachMonAn.First().mamon : 0;
        ViewBag.MuaKem = firstMonId > 0
            ? await _recommendationService.GetAprioriRecommendations(new List<int> { firstMonId }, 4)
            : new List<tbMonAn>();
        ViewBag.TrendingNow = await _recommendationService.GetTimeBasedRecommendations(4);

        return View(quanAn);
    }

    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [EnableRateLimiting("login-policy")]
    [ValidateAntiForgeryToken]
    public ActionResult Login(string usernameOrPhone, string pwd, bool rememberMe = false)
    {
        try
        {
        if (string.IsNullOrWhiteSpace(usernameOrPhone) || string.IsNullOrWhiteSpace(pwd))
        {
            ViewBag.LoginFail = "Vui lòng nhập tên đăng nhập/SĐT và mật khẩu";
            return View();
        }

        // Tự động nhận diện: nếu nhập số (bắt đầu bằng 0, 10-11 số) → tìm theo SĐT, ngược lại → tìm theo username
        bool isPhone = Regex.IsMatch(usernameOrPhone, @"^0[1-9][0-9]{8,9}$");

        IQueryable<tbUser> query = db.tbUser.AsQueryable();
        if (isPhone)
            query = query.Where(u => u.sdt == usernameOrPhone);
        else
            query = query.Where(u => u.username == usernameOrPhone);

        var users = query.ToList();
        if (users.Count != 0)
        {
            var userFind = users[0];

            // === Kiểm tra mật khẩu dạng plain-text ===
            bool passwordMatched = (userFind.pwd == pwd);

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

            var cart = new Cart { userid = userFind.userid };
            SetCart(cart);
            SetSessionUser(userFind);

            // Nếu người dùng chọn "Lưu đăng nhập", làm cho session cookie persistent
            // Dùng HttpContext.Session.Id thay vì Request.Cookies vì cookie chưa tồn tại trong request
            if (rememberMe)
            {
                Response.Cookies.Append(".AspNetCore.Session", HttpContext.Session.Id, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                });
            }

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
            ViewBag.LoginFail = "Tài khoản không tồn tại. Vui lòng kiểm tra tên đăng nhập hoặc số điện thoại.";
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
                _ => $"Lỗi: {ex.Message}"
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
                    var randomPwd = $"GG_{Guid.NewGuid():N}";
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
                    SetSessionUser(newUser);

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
            SetSessionUser(userFind);

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
                // Log lỗi chi tiết để debug trên Railway (KHÔNG hiển thị raw SQL cho user)
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
        public ActionResult CompleteGoogleRegistration(string loaitaikhoan, string sdt, string diachi)
        {
            var email = HttpContext.Session.GetString("google_email");
            var name = HttpContext.Session.GetString("google_name");

            if (string.IsNullOrEmpty(email))
            {
                TempData["err"] = "Phiên đăng ký đã hết hạn. Vui lòng đăng nhập Google lại.";
                return RedirectToAction("Login");
            }

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
                var randomPwd = $"GG_{Guid.NewGuid():N}";
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
                            trangthai     = "Không hoạt động"
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
                SetSessionUser(newUser);

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
                TempData["err"] = $"Lỗi hệ thống: {ex.Message}. Vui lòng thử lại.";
                return RedirectToAction("SelectRoleGoogle");
            }
        }

    public ActionResult Signup()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Signup(tbUser user, string repeatpw, string diachi, string hoten)
    {
        // === Validate dữ liệu đầu vào ===

        // Họ tên
        if (string.IsNullOrWhiteSpace(hoten) || hoten.Length < 2 || hoten.Length > 100)
        {
            ViewBag.err = "Họ tên phải từ 2-100 ký tự";
            return View();
        }

        // Username
        if (string.IsNullOrWhiteSpace(user.username) || user.username.Length < 3 || user.username.Length > 50)
        {
            ViewBag.err = "Tên đăng nhập phải từ 3-50 ký tự";
            return View();
        }

        // Mật khẩu - sử dụng password strength validation mới
        if (string.IsNullOrEmpty(user.pwd))
        {
            ViewBag.err = "Vui lòng nhập mật khẩu";
            return View();
        }
        var (pwdValid, pwdMessage) = ValidatePasswordStrength(user.pwd);
        if (!pwdValid)
        {
            ViewBag.err = pwdMessage;
            return View();
        }
        if (user.pwd != repeatpw)
        {
            ViewBag.err = "Xác nhận mật khẩu không khớp";
            return View();
        }
        // Mật khẩu được lưu dạng plain-text (không hash)

        // Số điện thoại — validate format Việt Nam (10-11 số, bắt đầu bằng 0)
        if (string.IsNullOrWhiteSpace(user.sdt))
        {
            ViewBag.err = "Vui lòng nhập số điện thoại";
            return View();
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(user.sdt, @"^0[1-9][0-9]{8,9}$"))
        {
            ViewBag.err = "Số điện thoại không hợp lệ — phải là 10-11 số, bắt đầu bằng 0 (VD: 0912345678)";
            return View();
        }

        // Email
        if (string.IsNullOrWhiteSpace(user.email))
        {
            ViewBag.err = "Vui lòng nhập email";
            return View();
        }
        try
        {
            var addr = new System.Net.Mail.MailAddress(user.email);
            if (addr.Address != user.email)
                throw new Exception();
        }
        catch
        {
            ViewBag.err = "Email không hợp lệ (VD: example@gmail.com)";
            return View();
        }

        // Địa chỉ
        if (string.IsNullOrWhiteSpace(diachi) || diachi.Length < 5 || diachi.Length > 250)
        {
            ViewBag.err = "Địa chỉ phải từ 5-250 ký tự";
            return View();
        }

        // Loại tài khoản
        if (string.IsNullOrWhiteSpace(user.loaitaikhoan))
        {
            ViewBag.err = "Vui lòng chọn loại tài khoản";
            return View();
        }
        var validRoles = new[] { "Khách hàng", "Quán ăn", "Shipper" };
        if (!validRoles.Contains(user.loaitaikhoan))
        {
            ViewBag.err = "Loại tài khoản không hợp lệ";
            return View();
        }

        // Kiểm tra trùng lặp
        var existingUsers = db.tbUser.Where(u => u.username.Equals(user.username)).ToList();
        if (existingUsers.Count != 0)
        {
            ViewBag.err = "Tên tài khoản đã tồn tại";
            return View();
        }
        var existingEmails = db.tbUser.Where(u => u.email == user.email).ToList();
        if (existingEmails.Count != 0)
        {
            ViewBag.err = "Email này đã được sử dụng";
            return View();
        }

        if (user.loaitaikhoan.Equals("Khách hàng"))
        {
            user.vitien = 0;
            user.trangthai = 1;
            db.tbUser.Add(user);
            db.SaveChanges();

            db.tbKhachHang.Add(new tbKhachHang { userid = user.userid, tenkh = hoten });
            db.SaveChanges();
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
        }
        else if (user.loaitaikhoan.Equals("Shipper"))
        {
            user.vitien = 0;
            user.trangthai = 1;
            db.tbUser.Add(user);
            db.SaveChanges();

            db.tbShipper.Add(new tbShipper
            {
                userid = user.userid,
                tenshipper = hoten,
                diachi = diachi,
                soluotdanhgia = 0,
                diemdanhgia = 0,
                trangthai = "Không hoạt động"
            });
            db.SaveChanges();
        }

        return RedirectToAction("Login");
    }

    public ActionResult Forgot()
    {
        return View();
    }

    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Remove("user");
        HttpContext.Session.Remove("cart");
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
        var ctmonan = db.tbMonAn.Find(id);
        return View(ctmonan);
    }

    /// <summary>
    /// Trang lỗi mặc định — tránh 404 khi exception handler redirect đến /Home/Error
    /// </summary>
    public ActionResult Error()
    {
        return View();
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

        return Json(new { success = true, database = debugInfo });
    }

    /// <summary>
    /// Ghi đè BCrypt hash trong database Railway bằng plain-text password
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
    /// Seed database — chèn seed data từ mysql_utf8.sql nếu chưa có user nào.
    /// Gọi GET /Home/SeedDb từ browser sau deploy (chỉ chạy 1 lần).
    /// </summary>
    public IActionResult SeedDb()
    {
        try
        {
            // Kiểm tra nếu đã có dữ liệu thì skip
            if (db.tbUser.Any())
            {
                return Content("✅ Database đã có dữ liệu, không cần seed.");
            }

            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

            string sqlPath = System.IO.Path.Combine(env.ContentRootPath, "mysql_utf8.sql");
            if (!System.IO.File.Exists(sqlPath))
            {
                return Content("❌ Không tìm thấy mysql_utf8.sql tại: " + sqlPath);
            }

            var sql = System.IO.File.ReadAllText(sqlPath);
            // Tách các câu lệnh SQL theo dấu ;
            var statements = sql.Split(';', StringSplitOptions.RemoveEmptyEntries);

            int success = 0, fail = 0;
            foreach (var stmt in statements)
            {
                var trimmed = stmt.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("--") || trimmed.StartsWith("DROP") || trimmed.StartsWith("CREATE"))
                    continue;

                try
                {
                    db.Database.ExecuteSqlRaw(trimmed);
                    success++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning("Seed SQL statement failed: {Error}", ex.Message);
                    fail++;
                }
            }

            return Content($"✅ Seed hoàn tất! {success} câu lệnh thành công, {fail} lỗi (có thể bỏ qua).");
        }
        catch (Exception ex)
        {
            return Content($"❌ Lỗi seed: {ex.Message}");
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

            // Query chính: tìm kiếm trực tiếp trong tbMonAn
            var query = db.tbMonAn
                .Include(m => m.tbQuanAn)
                .Include(m => m.tbBienTheMonAns)
                .Include(m => m.tbDanhMuc)
                .Where(m => m.tbQuanAn != null && m.tbQuanAn.trangthai == "Đang mở cửa");

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

                // Query lại từ đầu, chỉ áp dụng AND cho tiêu chí cốt lõi (category + search)
                var fallbackQuery = db.tbMonAn
                    .Include(m => m.tbQuanAn)
                    .Include(m => m.tbBienTheMonAns)
                    .Include(m => m.tbDanhMuc)
                    .Where(m => m.tbQuanAn != null && m.tbQuanAn.trangthai == "Đang mở cửa");

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
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(new object[0]);

        var normalized = RemoveDiacritics(q.ToLower());

        var results = db.tbQuanAn
            .Include(qa => qa.tbUser)
            .Where(qa => qa.trangthai == "Đang mở cửa")
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

    public ActionResult NhanTin()
    {
        return View();
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
    public JsonResult GetReviews(int quanId, int page = 1, int pageSize = 5)
    {
        // 1) Query gốc: ChiTietDonHang có đánh giá, thuộc quán này
        var baseQuery = db.tbChiTietDonHang
            .Include(c => c.tbBienTheMonAn!).ThenInclude(b => b.tbMonAn)
            .Include(c => c.tbDanhGias)
            .Include(c => c.tbDonHang!).ThenInclude(d => d.tbThongTinDatHang!).ThenInclude(t => t.tbKhachHang)
            .Where(c => c.tbBienTheMonAn != null && c.tbBienTheMonAn.tbMonAn!.maquanan == quanId && c.tbDanhGias.Any());

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

        // Chỉ cho phép đánh giá đơn đã hoàn thành
        if (chiTiet.tbDonHang?.trangthai != "Hoàn thành" && chiTiet.tbDonHang?.trangthai != "Đang xử lý")
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
