using System.Globalization;
using System.Text;
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
        if (txtSearch != null)
        {
            string searchKeyNormalized = RemoveDiacritics(txtSearch.ToLower());
            quanAns = quanAns.Where(qa =>
                RemoveDiacritics(qa.tenquanan.ToLower()).Contains(searchKeyNormalized)
                || RemoveDiacritics(qa.tbUser.username.ToLower()).Contains(searchKeyNormalized)
                || qa.tbMonAn.Any(ma => RemoveDiacritics(ma.tenmon.ToLower()).Contains(searchKeyNormalized))
            ).ToList();
            ViewBag.txtSearch = txtSearch;
        }
        if (idDM != null)
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
        if (searchKey != null)
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
    public ActionResult Login(tbUser user)
    {
        var users = db.tbUser.Where(u => u.username.Equals(user.username) && u.pwd.Equals(user.pwd)).ToList();
        if (users.Count != 0)
        {
            var userFind = users[0];
            // Chỉ chặn tài khoản bị khóa (trangthai == 2), bỏ check duyệt
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

    public ActionResult Signup()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Signup(tbUser user, string repeatpw, string diachi, string hoten)
    {
        // BUG-004: Validate mật khẩu mạnh
        if (string.IsNullOrEmpty(user.pwd) || user.pwd.Length < 6)
        {
            ViewBag.err = "Mật khẩu phải có ít nhất 6 ký tự";
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
            ViewBag.err = "Xác nhận mật khẩu sai";
            return View();
        }

        // BUG-005/006: Validate các trường bắt buộc
        if (string.IsNullOrWhiteSpace(user.email))
        {
            ViewBag.err = "Vui lòng nhập email";
            return View();
        }
        if (string.IsNullOrWhiteSpace(diachi))
        {
            ViewBag.err = "Vui lòng nhập địa chỉ";
            return View();
        }
        if (string.IsNullOrWhiteSpace(hoten))
        {
            ViewBag.err = "Vui lòng nhập họ tên";
            return View();
        }

        var existingUsers = db.tbUser.Where(u => u.username.Equals(user.username)).ToList();
        if (existingUsers.Count != 0)
        {
            ViewBag.err = "Tên tài khoản đã tồn tại";
            return View();
        }

        // Kiểm tra email đã tồn tại chưa
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
            user.trangthai = 0;
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
            user.trangthai = 0;
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

    public ActionResult Logout()
    {
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
