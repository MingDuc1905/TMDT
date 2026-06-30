using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
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

    public ActionResult Index(string? txtSearch, int? idDM)
    {
        var quanAns = db.tbQuanAn.Include(q => q.tbUser).Include(q => q.tbMonAns).ToList();
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
        return View();
    }

    public async Task<ActionResult> DetailRestaurant(int id, int? idDM, string? searchKey)
    {
        var quanAn = db.tbQuanAn.Include(q => q.tbMonAns).ThenInclude(m => m.tbDanhMuc)
            .FirstOrDefault(t => t.userid == id);
        if (quanAn == null)
            return NotFound();

        var danhSachMonAn = db.tbMonAn.Where(m => m.maquanan == id).Include(m => m.tbDanhMuc).ToList();
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
            .Where(km => monAnIds.Contains(km.mamon ?? 0) && km.trangthai == "Còn hạn")
            .Include(km => km.tbKhuyenMai)
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
            daMuaMonAnIds = db.tbDonHang
                .Where(dh => mattdhIds.Contains(dh.mattdh) && dh.trangthai != "Đã hủy")
                .SelectMany(dh => dh.tbChiTietDonHangs)
                .Select(ct => ct.mamon ?? 0)
                .Distinct()
                .ToHashSet();
        }
        ViewBag.DaMuaMonAnIds = daMuaMonAnIds;

        ViewBag.ThucDon = thucDon;
        ViewBag.DanhSachMonAn = danhSachMonAn;
        ViewBag.maquan = id;
        ViewBag.searchKey = searchKey;
        ViewBag.KhuyenMais = khuyenMais;

        // Recommendation: Gợi ý món thường mua kèm (dựa trên món đầu tiên)
        if (danhSachMonAn.Any())
        {
            var firstMon = danhSachMonAn.First();
            ViewBag.MuaKem = await _recommendationService.GetFrequentlyBoughtTogether(firstMon.mamon, 4);
            ViewBag.TrendingNow = await _recommendationService.GetTimeBasedRecommendations(4);
        }
        else
        {
            ViewBag.MuaKem = new List<tbMonAn>();
            ViewBag.TrendingNow = await _recommendationService.GetTimeBasedRecommendations(4);
        }

        return View(quanAn);
    }

    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    [HttpPost]
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

            // === Kiểm tra mật khẩu với backward compatibility ===
            // Mật khẩu cũ trong seed data là plaintext, mật khẩu mới là BCrypt hash
            bool passwordMatched = false;

            // Thử 1: BCrypt (dành cho user đăng ký từ khi có BCrypt)
            if (userFind.pwd != null && userFind.pwd.StartsWith("$2"))
            {
                passwordMatched = BCrypt.Net.BCrypt.Verify(pwd, userFind.pwd);
            }
            // Thử 2: So sánh plaintext (dành cho user cũ / seed data)
            else
            {
                passwordMatched = (userFind.pwd == pwd);
            }

            // Nếu khớp bằng plaintext, nâng cấp lên BCrypt ngay
            // EF Core tự động tracking entity sau ToList(), chỉ cần set property + SaveChanges
            if (passwordMatched && (userFind.pwd == null || !userFind.pwd.StartsWith("$2")))
            {
                userFind.pwd = BCrypt.Net.BCrypt.HashPassword(pwd, workFactor: 12);
                db.SaveChanges();
            }

            if (!passwordMatched)
            {
                ViewBag.LoginFail = "Đăng nhập thất bại";
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

            return userFind.loaitaikhoan switch
            {
                "Khách hàng" => RedirectToAction("Index", "Home"),
                "Shipper" => RedirectToAction("Index", "Shipper"),
                "Quán ăn" => RedirectToAction("Index", "Restaurant"),
                "Admin" => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction("Index"),
            };
        }
        else
        {
            ViewBag.LoginFail = "Đăng nhập thất bại";
            return View();
        }
        }
        catch (Exception ex)
        {
            // Log chi tiết để debug — hiển thị lỗi thân thiện cho người dùng
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogError(ex, "Login failed for user {User}", usernameOrPhone);
            ViewBag.LoginFail = "Lỗi hệ thống: " + ex.Message;
            return View();
        }
    }

    /// <summary>
    /// Đăng nhập bằng Google - chuyển hướng đến Google OAuth
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
    /// Google OAuth callback - xử lý sau khi Google xác thực thành công
    /// </summary>
    public async Task<ActionResult> GoogleResponse()
    {
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
        var email = authenticateResult.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = authenticateResult.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            ViewBag.LoginFail = "Không thể lấy thông tin email từ Google";
            return View("Login");
        }

        // Tìm user theo email
        var users = db.tbUser.Where(u => u.email == email).ToList();
        if (users.Count == 0)
        {
            ViewBag.LoginFail = $"Email '{email}' chưa được đăng ký trong hệ thống. Vui lòng đăng ký tài khoản trước.";
            return View("Login");
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
            ViewBag.LoginFail = "Đăng nhập Google gặp sự cố. Vui lòng thử lại hoặc dùng tài khoản thường.";
            return View("Login");
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
        // Hash password bằng BCrypt trước khi lưu
        user.pwd = BCrypt.Net.BCrypt.HashPassword(user.pwd, workFactor: 12);

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
    /// Seed database — chèn seed data từ seed_mysql.sql nếu chưa có user nào.
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

            string sqlPath = System.IO.Path.Combine(env.ContentRootPath, "seed_mysql.sql");
            if (!System.IO.File.Exists(sqlPath))
            {
                return Content("❌ Không tìm thấy seed_mysql.sql tại: " + sqlPath);
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
            .Include(c => c.tbMonAn)
            .Where(c => c.tbMonAn != null && c.tbMonAn.maquanan == quanId
                        && c.tbDonHang != null && c.tbDonHang.tbThongTinDatHang != null
                        && c.tbDonHang.tbThongTinDatHang.userid == user!.userid
                        && !c.tbDanhGias.Any())     // chưa đánh giá
            .Select(c => new
            {
                mactdh = c.mactdh,
                tenmon = c.tbMonAn!.tenmon,
                madh   = c.madh
            })
            .Take(20)
            .ToList();

        return Json(new { items });
    }

    // ===== API: Lấy danh sách đánh giá của một quán (public) =====
    [HttpGet]
    public JsonResult GetReviews(int quanId, int page = 1, int pageSize = 5)
    {
        var chiTietDHs = db.tbChiTietDonHang
            .Include(c => c.tbDonHang).ThenInclude(d => d!.tbThongTinDatHang).ThenInclude(t => t!.tbKhachHang)
            .Include(c => c.tbMonAn)
            .Include(c => c.tbDanhGias)
            .Where(c => c.tbMonAn != null && c.tbMonAn.maquanan == quanId && c.tbDanhGias.Any())
            .ToList();

        var reviews = chiTietDHs
            .SelectMany(c => c.tbDanhGias.Select(dg => new
            {
                madg       = dg.madg,
                diem       = dg.diemdanhgia ?? 0,
                nhanxet    = dg.nhanxet ?? "",
                tenmon     = c.tbMonAn?.tenmon ?? "",
                hinhanh    = c.tbMonAn?.hinhanh ?? "",
                tenkh      = c.tbDonHang?.tbThongTinDatHang?.tbKhachHang?.tenkh ?? "Khách hàng",
                ngaydat    = c.tbDonHang?.ngaydathang?.ToString("dd/MM/yyyy") ?? ""
            }))
            .OrderByDescending(r => r.madg)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var total = chiTietDHs.SelectMany(c => c.tbDanhGias).Count();
        var avgDiem = total > 0 ? chiTietDHs.SelectMany(c => c.tbDanhGias).Average(d => d.diemdanhgia ?? 0) : 0;

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
        var monAn = db.tbMonAn.Find(chiTiet.mamon);
        if (monAn?.maquanan != null)
        {
            var quanAn = db.tbQuanAn.Find(monAn.maquanan);
            if (quanAn != null)
            {
                var allDGs = db.tbDanhGia
                    .Include(d => d.tbChiTietDonHang).ThenInclude(c => c!.tbMonAn)
                    .Where(d => d.tbChiTietDonHang != null && d.tbChiTietDonHang.tbMonAn != null
                                && d.tbChiTietDonHang.tbMonAn.maquanan == quanAn.userid)
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
