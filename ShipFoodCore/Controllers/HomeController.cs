using System.Globalization;
using System.Text;
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
    public ActionResult Login(string usernameOrPhone, string pwd)
    {
        if (string.IsNullOrWhiteSpace(usernameOrPhone) || string.IsNullOrWhiteSpace(pwd))
        {
            ViewBag.LoginFail = "Vui lòng nhập tên đăng nhập/SĐT và mật khẩu";
            return View();
        }

        // Tự động nhận diện: nếu nhập số (bắt đầu bằng 0, 10-11 số) → tìm theo SĐT, ngược lại → tìm theo username
        bool isPhone = System.Text.RegularExpressions.Regex.IsMatch(usernameOrPhone, @"^0[1-9][0-9]{8,9}$");

        IQueryable<tbUser> query = db.tbUser.Where(u => u.pwd == pwd);
        if (isPhone)
            query = query.Where(u => u.sdt == usernameOrPhone);
        else
            query = query.Where(u => u.username == usernameOrPhone);

        var users = query.ToList();
        if (users.Count != 0)
        {
            var userFind = users[0];
            if (userFind.trangthai == 2)
            {
                ViewBag.LoginFail = "Tài khoản đã bị khóa";
                return View();
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
        else
        {
            ViewBag.LoginFail = "Đăng nhập thất bại";
            return View();
        }
    }

    /// <summary>
    /// Đăng nhập bằng Google - chuyển hướng đến Google OAuth
    /// </summary>
    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action("GoogleResponse", "Home");
        var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Google OAuth callback - xử lý sau khi Google xác thực thành công
    /// </summary>
    public async Task<ActionResult> GoogleResponse()
    {
        // Đọc từ cookie (Google middleware tự động lưu vào cookie nhờ AddCookie)
        var authenticateResult = await HttpContext.AuthenticateAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
        {
            ViewBag.LoginFail = "Đăng nhập Google thất bại";
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
            ViewBag.LoginFail = "Email Google này chưa được đăng ký trong hệ thống. Vui lòng đăng ký tài khoản trước.";
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

    public ActionResult Signup()
    {
        return View();
    }

    [HttpPost]
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

        // Mật khẩu
        if (string.IsNullOrEmpty(user.pwd) || user.pwd.Length < 6 || user.pwd.Length > 50)
        {
            ViewBag.err = "Mật khẩu phải từ 6-50 ký tự";
            return View();
        }
        if (!user.pwd.Any(char.IsDigit))
        {
            ViewBag.err = "Mật khẩu phải chứa ít nhất một chữ số";
            return View();
        }
        if (!user.pwd.Any(char.IsLetter))
        {
            ViewBag.err = "Mật khẩu phải chứa ít nhất một chữ cái";
            return View();
        }
        if (user.pwd != repeatpw)
        {
            ViewBag.err = "Xác nhận mật khẩu không khớp";
            return View();
        }

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

    public ActionResult NhanTin()
    {
        return View();
    }
}
