using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShipFood.Hubs;
using ShipFood.Models;
using ShipFood.Services;
using System.Text;

namespace ShipFood.Controllers;

public class AdminController : BaseController
{
    private readonly IWebHostEnvironment _env;
    private readonly RecommendationService _recommendationService;

    public AdminController(dbFoodyEntities context, IWebHostEnvironment env, RecommendationService recommendationService)
    {
        db = context;
        _env = env;
        _recommendationService = recommendationService;
    }

    public ActionResult Index()
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        return View();
    }

    public ActionResult Order()
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        var litsdh = db.tbDonHang
            .Include(d => d.tbShipper)
            .Include(d => d.tbQuanAn)
            .Include(d => d.tbThongTinDatHang)
            .ToList();
        return View(litsdh);
    }

    public ActionResult OrderDetail(int? id)
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        var donhang = db.tbDonHang
            .Include(d => d.tbQuanAn).ThenInclude(q => q.tbUser)
            .Include(d => d.tbThongTinDatHang)
            .Include(d => d.tbShipper).ThenInclude(s => s.tbUser)
            .FirstOrDefault(d => d.madh == id);
        var chitietdh = db.tbChiTietDonHang
            .Where(ct => ct.madh == id)
            .Include(c => c.tbBienTheMonAn!).ThenInclude(b => b.tbMonAn!).ThenInclude(m => m.tbDanhMuc)
            .ToList();
        ViewBag.chitietdonhang = chitietdh;
        return View(donhang);
    }

    public ActionResult Category(int? id)
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");

        if (id.HasValue)
        {
            var bd = db.tbDanhMuc.Include(d => d.tbMonAns).FirstOrDefault(d => d.madanhmuc == id);
            if (bd != null)
            {
                // Kiểm tra ràng buộc khóa ngoại trước khi xóa
                if (bd.tbMonAns.Any())
                {
                    TempData["AdminError"] = "Không thể xóa danh mục này vì vẫn còn món ăn thuộc danh mục. Vui lòng xóa hoặc chuyển các món ăn sang danh mục khác trước.";
                    return RedirectToAction("Category");
                }
                db.tbDanhMuc.Remove(bd);
                db.SaveChanges();
                TempData["AdminSuccess"] = "Xóa danh mục thành công";
            }
        }
        var listdm = db.tbDanhMuc.ToList();
        return View(listdm);
    }

    public ActionResult CreateCategory()
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> CreateCategory(tbDanhMuc tbDanhMuc, IFormFile? hinhanh)
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");

        if (hinhanh != null && hinhanh.Length > 0)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "Source/images/Danhmuc");
            Directory.CreateDirectory(uploadsDir);
            var fileName = Path.GetFileName(hinhanh.FileName);
            var path = Path.Combine(uploadsDir, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await hinhanh.CopyToAsync(stream);
            tbDanhMuc.hinhanh = fileName;
        }
        else
        {
            tbDanhMuc.hinhanh = null;
        }

        db.tbDanhMuc.Add(tbDanhMuc);
        await db.SaveChangesAsync();
        return RedirectToAction("Category");
    }

    public async Task<ActionResult> EditCategory(int? id)
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        if (id == null)
            return BadRequest();

        var tbDanhMuc = await db.tbDanhMuc.FirstOrDefaultAsync(x => x.madanhmuc == id);
        if (tbDanhMuc == null)
            return NotFound();
        return View(tbDanhMuc);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditCategory(tbDanhMuc tbDanhMuc, IFormFile? hinhanh)
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");

        var existingDanhMuc = await db.tbDanhMuc.FirstOrDefaultAsync(x => x.madanhmuc == tbDanhMuc.madanhmuc);
        if (existingDanhMuc == null) return NotFound();

        if (hinhanh != null && hinhanh.Length > 0)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "Source/images/Danhmuc");
            Directory.CreateDirectory(uploadsDir);
            var fileName = Path.GetFileName(hinhanh.FileName);
            var path = Path.Combine(uploadsDir, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await hinhanh.CopyToAsync(stream);                var oldImagePath = Path.Combine(uploadsDir, tbDanhMuc.hinhanh ?? "");
                if (!string.IsNullOrEmpty(tbDanhMuc.hinhanh) && System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);

            tbDanhMuc.hinhanh = fileName;
        }
        else
        {
            tbDanhMuc.hinhanh = existingDanhMuc.hinhanh;
        }

        db.Entry(existingDanhMuc).CurrentValues.SetValues(tbDanhMuc);
        await db.SaveChangesAsync();
        return RedirectToAction("Category");
    }

    public ActionResult GetListCategory(string? timkiem)
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");

        IQueryable<tbDanhMuc> list = db.tbDanhMuc;
        if (!string.IsNullOrEmpty(timkiem))
        {
            string text = timkiem.ToLower();
            list = list.Where(p => p.tendanhmuc.ToLower().Contains(text) || (p.mota != null && p.mota.ToLower().Contains(text)));
        }
        var listdm = list.ToList();
        return PartialView("_ListCategory", listdm);
    }

    public ActionResult QuanLyQuanAn()
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        var quanAns = db.tbQuanAn.Include(q => q.tbUser).ToList();
        ViewBag.datas = quanAns;
        return View();
    }

    public ActionResult QuanLyKhachHang()
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        var khachHangs = db.tbKhachHang.Include(k => k.tbUser).ToList();
        ViewBag.datas = khachHangs;
        return View();
    }

    public ActionResult QuanLyQuanTriVien()
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        var admins = db.tbAdmin.Include(a => a.tbUser).ToList();
        ViewBag.datas = admins;
        return View();
    }

    public ActionResult QuanLyShipper()
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        var shippers = db.tbShipper.Include(s => s.tbUser).ToList();
        ViewBag.datas = shippers;
        return View();
    }

    public ActionResult PostTaiKhoan(int? id)
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");

        string hoten = "", quan = "", diachicuthe = "", diachiadd = "";
        string? fileAnh = null;
        var user = new tbUser();

        if (id != null)
        {
            user = db.tbUser.Find(id) ?? new tbUser();
            hoten = user.loaitaikhoan switch
            {
                "Khách hàng" => user.tbKhachHang?.tenkh ?? "",
                "Quán ăn" => user.tbQuanAn?.tenquanan ?? "",
                "Shipper" => user.tbShipper?.tenshipper ?? "",
                "Admin" => user.tbAdmin?.tenadmin ?? "",
                _ => ""
            };

            if (user.loaitaikhoan == "Quán ăn" && user.tbQuanAn != null)
            {
                var txt = user.tbQuanAn.diachi.Split(", ");
                if (txt.Length >= 3)
                {
                    diachiadd = txt[0]; diachicuthe = txt[1]; quan = txt[2];
                }
                fileAnh = user.tbQuanAn.hinhanh;
            }
            else if (user.loaitaikhoan == "Shipper" && user.tbShipper != null)
            {
                fileAnh = user.tbShipper.hinhanh;
                var txt = user.tbShipper.diachi.Split(", ");
                if (txt.Length >= 3)
                {
                    diachiadd = txt[0]; diachicuthe = txt[1]; quan = txt[2];
                }
            }
        }

        ViewBag.user = user;
        ViewBag.hoten = hoten;
        ViewBag.quan = quan;
        ViewBag.diachicuthe = diachicuthe;
        ViewBag.diachiadd = diachiadd;
        ViewBag.fileAnh = fileAnh;
        return View();
    }

    [HttpPost]
    public ActionResult PostTaiKhoan(tbUser user, string hoten, string quan, string diachicuthe,
        string diachiadd, IFormFile? fileAnh)
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");

        if (fileAnh != null)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "Source/images/Avatar");
            Directory.CreateDirectory(uploadsDir);
            var path = Path.Combine(uploadsDir, fileAnh.FileName);
            using var stream = new FileStream(path, FileMode.Create);
            fileAnh.CopyTo(stream);
        }

        if (user.userid == 0)
        {
            user.trangthai = 1;
            user.vitien = 0;
            db.tbUser.Add(user);
            db.SaveChanges();

            var fullAddress = $"{diachiadd}, {diachicuthe}, {quan}, TP. Hồ Chí Minh";

            switch (user.loaitaikhoan)
            {
                case "Khách hàng":
                    db.tbKhachHang.Add(new tbKhachHang { userid = user.userid, tenkh = hoten });
                    db.tbThongTinDatHang.Add(new tbThongTinDatHang
                    {
                        diachi = fullAddress,
                        sdt = user.sdt,
                        tennguoinhan = hoten,
                        userid = user.userid,
                        toado = null
                    });
                    break;
                case "Quán ăn":
                    db.tbQuanAn.Add(new tbQuanAn
                    {
                        userid = user.userid,
                        tenquanan = hoten,
                        diachi = fullAddress,
                        soluotdanhgia = 0,
                        diemdanhgia = 0,
                        trangthai = "Đóng cửa",
                        hinhanh = fileAnh?.FileName
                    });
                    break;
                case "Shipper":
                    db.tbShipper.Add(new tbShipper
                    {
                        userid = user.userid,
                        tenshipper = hoten,
                        diachi = fullAddress,
                        soluotdanhgia = 0,
                        diemdanhgia = 0,
                        trangthai = "Không hoạt động",
                        hinhanh = fileAnh?.FileName
                    });
                    break;
                case "Admin":
                    db.tbAdmin.Add(new tbAdmin { userid = user.userid, tenadmin = hoten });
                    break;
            }
            db.SaveChanges();
        }
        else
        {
            var userOld = db.tbUser.Find(user.userid);
            if (userOld != null)
            {
                userOld.pwd = user.pwd;
                userOld.sdt = user.sdt;
                userOld.email = user.email;
            }

            var fullAddress = $"{diachiadd}, {diachicuthe}, {quan}, TP. Hồ Chí Minh";

            switch (user.loaitaikhoan)
            {
                case "Khách hàng":
                    var kh = db.tbKhachHang.Find(user.userid);
                    if (kh != null) kh.tenkh = hoten;
                    break;
                case "Quán ăn":
                    var qa = db.tbQuanAn.Find(user.userid);
                    if (qa != null)
                    {
                        qa.tenquanan = hoten;
                        qa.diachi = fullAddress;
                        if (fileAnh != null) qa.hinhanh = fileAnh.FileName;
                    }
                    break;
                case "Shipper":
                    var sh = db.tbShipper.Find(user.userid);
                    if (sh != null)
                    {
                        sh.tenshipper = hoten;
                        sh.diachi = fullAddress;
                        if (fileAnh != null) sh.hinhanh = fileAnh.FileName;
                    }
                    break;
                case "Admin":
                    var ad = db.tbAdmin.Find(user.userid);
                    if (ad != null) ad.tenadmin = hoten;
                    break;
            }
            db.SaveChanges();
        }

        return RedirectToAction("PostTaiKhoan");
    }

    public ActionResult Duyet(int? id)
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        var user = db.tbUser.Find(id);
        if (user != null)
        {
            user.trangthai = 1;
            db.SaveChanges();
        }
        return user?.loaitaikhoan switch
        {
            "Shipper" => RedirectToAction("QuanLyShipper"),
            "Quán ăn" => RedirectToAction("QuanLyQuanAn"),
            _ => RedirectToAction("Index")
        };
    }

    public ActionResult Huy(int? id)
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        var user = db.tbUser.Find(id);
        if (user != null)
        {
            user.trangthai = 3;
            db.SaveChanges();
        }
        return user?.loaitaikhoan switch
        {
            "Shipper" => RedirectToAction("QuanLyShipper"),
            "Quán ăn" => RedirectToAction("QuanLyQuanAn"),
            _ => RedirectToAction("Index")
        };
    }

    public ActionResult LockOrUnLock(int? id)
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");
        
        if (id == null)
        {
            TempData["AdminError"] = "Không tìm thấy tài khoản";
            return RedirectToAction("Index");
        }

        var user = db.tbUser.Find(id);
        if (user != null)
        {
            if (user.loaitaikhoan.Equals("Admin"))
            {
                // Kiểm tra không khóa admin cuối cùng
                var adminCount = db.tbUser.Count(u => u.loaitaikhoan == "Admin" && u.trangthai == 1);
                if (user.trangthai == 1 && adminCount <= 1)
                {
                    TempData["AdminError"] = "Không thể khóa tài khoản Admin cuối cùng";
                    return RedirectToAction("QuanLyQuanTriVien");
                }
            }
            user.trangthai = user.trangthai == 1 ? 2 : 1;
            db.SaveChanges();
            TempData["AdminSuccess"] = "Cập nhật trạng thái tài khoản thành công";
        }
        else
        {
            TempData["AdminError"] = "Không tìm thấy tài khoản";
        }

        return user?.loaitaikhoan switch
        {
            "Shipper" => RedirectToAction("QuanLyShipper"),
            "Quán ăn" => RedirectToAction("QuanLyQuanAn"),
            "Admin" => RedirectToAction("QuanLyQuanTriVien"),
            "Khách hàng" => RedirectToAction("QuanLyKhachHang"),
            _ => RedirectToAction("Index")
        };
    }

    // ===== DASHBOARD ANALYTICS API =====

    [HttpGet]
    public async Task<ActionResult> Dashboard()
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");

        // ─── Apriori: Phân tích liên kết danh mục ───
        // ponytail: try-catch để RecommendationService crash không làm hỏng dashboard
        try
        {
            ViewBag.CategoryAprioriInsights = await _recommendationService.GetCategoryAprioriInsights(6);
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminController>>();
            logger.LogWarning(ex, "Category Apriori insights failed");
            ViewBag.CategoryAprioriInsights = new List<object>();
        }

        return View();
    }

    [HttpGet]
    public JsonResult GetDashboardStats(DateTime? fromDate, DateTime? toDate)
    {
        if (!checkLogin())
            return Json(new { error = "Unauthorized" });

        var tuNgay = fromDate ?? DateTime.Now.AddDays(-30);
        var denNgay = toDate ?? DateTime.Now;

        // Tổng doanh thu (đơn hoàn thành)
        var tongDoanhThu = db.tbDonHang
            .Where(dh => dh.ngaydathang >= tuNgay && dh.ngaydathang <= denNgay && dh.trangthai == "Hoàn thành")
            .Sum(dh => (decimal?)dh.tongtien) ?? 0;

        // Tổng số đơn
        var tongSoDon = db.tbDonHang
            .Where(dh => dh.ngaydathang >= tuNgay && dh.ngaydathang <= denNgay)
            .Count();

        // Số khách hàng mới đã đăng ký (lọc theo thời gian)
        var khachHangMoi = db.tbUser
            .Count(u => u.loaitaikhoan == "Khách hàng" && u.trangthai == 1);

        // Số quán ăn đã duyệt
        var quanAnMoi = db.tbQuanAn.Count(qa => qa.tbUser != null && qa.tbUser.trangthai == 1);

        // Tỷ lệ hủy đơn
        var donHuy = db.tbDonHang
            .Where(dh => dh.ngaydathang >= tuNgay && dh.ngaydathang <= denNgay && dh.trangthai == "Đã hủy")
            .Count();
        var tiLeHuy = tongSoDon > 0 ? Math.Round((double)donHuy / tongSoDon * 100, 1) : 0;

        return Json(new
        {
            tongDoanhThu = tongDoanhThu,
            tongSoDon = tongSoDon,
            khachHangMoi = khachHangMoi,
            quanAnMoi = quanAnMoi,
            donHuy = donHuy,
            tiLeHuy = tiLeHuy
        });
    }

    [HttpGet]
    public JsonResult GetRevenueChart(DateTime? fromDate, DateTime? toDate)
    {
        if (!checkLogin())
            return Json(new { error = "Unauthorized" });

        var tuNgay = fromDate ?? DateTime.Now.AddDays(-30);
        var denNgay = toDate ?? DateTime.Now;

        var donHangs = db.tbDonHang
            .Where(dh => dh.ngaydathang >= tuNgay && dh.ngaydathang <= denNgay && dh.trangthai == "Hoàn thành")
            .ToList();

        var dailyData = donHangs
            .Where(dh => dh.ngaydathang != null)
            .GroupBy(dh => dh.ngaydathang!.Value.Date)
            .Select(g => new
            {
                date = g.Key.ToString("dd/MM"),
                doanhThu = g.Sum(dh => dh.tongtien ?? 0),
                soDon = g.Count()
            })
            .OrderBy(d => d.date)
            .ToList();

        return Json(dailyData);
    }

    [HttpGet]
    public JsonResult GetTopRestaurants()
    {
        if (!checkLogin())
            return Json(new { error = "Unauthorized" });

        var topQuan = db.tbDonHang
            .Where(dh => dh.trangthai == "Hoàn thành" && dh.tbQuanAn != null)
            .GroupBy(dh => new { dh.maquan, ten = dh.tbQuanAn!.tenquanan })
            .Select(g => new
            {
                tenQuan = g.Key.ten,
                doanhThu = g.Sum(dh => dh.tongtien ?? 0),
                soDon = g.Count()
            })
            .OrderByDescending(g => g.doanhThu)
            .Take(5)
            .ToList();

        return Json(topQuan);
    }

    [HttpGet]
    public JsonResult GetOrderStatusPie()
    {
        if (!checkLogin())
            return Json(new { error = "Unauthorized" });

        var hoanThanh = db.tbDonHang.Count(dh => dh.trangthai == "Hoàn thành");
        var daHuy = db.tbDonHang.Count(dh => dh.trangthai == "Đã hủy");
        var dangXuLy = db.tbDonHang.Count(dh => dh.trangthai != "Hoàn thành" && dh.trangthai != "Đã hủy");

        return Json(new
        {
            labels = new[] { "Hoàn thành", "Đã hủy", "Đang xử lý" },
            data = new[] { hoanThanh, daHuy, dangXuLy },
            colors = new[] { "#28a745", "#dc3545", "#ffc107" }
        });
    }

    [HttpGet]
    public ActionResult ExportExcel(string type = "revenue")
    {
        if (!checkLogin())
            return RedirectToAction("Login", "Home");

        var donHangs = db.tbDonHang
            .Include(d => d.tbQuanAn)
            .Include(d => d.tbThongTinDatHang)
            .Where(d => d.trangthai == "Hoàn thành")
            .OrderByDescending(d => d.ngaydathang)
            .ToList();

        // Build CSV content
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Mã ĐH,Ngày,Nhà hàng,Khách hàng,Tổng tiền,Phí ship,Trạng thái");
        foreach (var dh in donHangs)
        {
            sb.AppendLine($"{dh.madh},{dh.ngaydathang:dd/MM/yyyy},{dh.tbQuanAn?.tenquanan},{dh.tbThongTinDatHang?.tennguoinhan},{dh.tongtien:N0},{dh.phiship:N0},{dh.trangthai}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"bao-cao-doanh-thu-{DateTime.Now:yyyyMMdd}.csv");
    }

    // ─── Task 3a: SignalR Payment Confirmation (Mock Webhook) ───
    /// <summary>
    /// Admin bấm "Xác nhận đã nhận tiền" → SignalR broadcast real-time đến khách hàng
    /// </summary>
    [HttpPost]
    public async Task<JsonResult> MockPaymentWebhook(int madh)
    {
        if (!checkLogin())
            return Json(new { success = false, message = "Không có quyền" });

        var donHang = await db.tbDonHang.FindAsync(madh);
        if (donHang == null)
            return Json(new { success = false, message = "Đơn hàng không tồn tại" });

        // Cập nhật trạng thái
        donHang.trangthai = "Đã thanh toán";
        donHang.ngaythanhtoan = DateTime.Now;
        await db.SaveChangesAsync();

        // SignalR broadcast đến group order_{madh}
        var hubContext = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<ShipFood.Hubs.Chats>>();
        await hubContext.Clients.Group($"order_{madh}").SendAsync("paymentConfirmed", madh, donHang.tongtien);

        return Json(new
        {
            success = true,
            message = $"✅ Đã xác nhận thanh toán cho đơn hàng #{madh}. Real-time notification đã gửi đến khách hàng."
        });
    }

    private bool checkLogin()
    {
        var user = GetCurrentUser();
        return user != null && user.loaitaikhoan.Equals("Admin");
    }
}
