using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ShipFood.Hubs;
using ShipFood.Models;
using ShipFood.Services;

namespace ShipFood.Controllers;

public class RestaurantController : BaseController
{
    private readonly IWebHostEnvironment _env;
    private readonly IHubContext<Chats> _hubContext;
    private readonly RecommendationService _recommendationService;

    public RestaurantController(dbFoodyEntities context, IWebHostEnvironment env, IHubContext<Chats> hubContext, RecommendationService recommendationService)
    {
        db = context;
        _env = env;
        _hubContext = hubContext;
        _recommendationService = recommendationService;
    }

    public async Task<ActionResult> Index()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var QuanAn = getQuanAn();

        ViewBag.soLuongMonAn = QuanAn.tbMonAn.Count;
        ViewBag.tongDoanhThu = (double?)QuanAn.tbDonHang.Sum(dh => dh.tongtien) ?? 0;
        ViewBag.soDonDatHang = QuanAn.tbDonHang.Count;

        var userids = new List<int?>();
        foreach (var i in QuanAn.tbDonHang)
        {
            if (i.tbThongTinDatHang != null && !userids.Contains(i.tbThongTinDatHang.userid))
                userids.Add(i.tbThongTinDatHang.userid);
        }
        ViewBag.soLuongKhachHang = userids.Count;
        ViewBag.dhChuanBi = QuanAn.tbDonHang.Count(dh => dh.trangthai == "Đang chuẩn bị");
        ViewBag.dhHoanThanh = QuanAn.tbDonHang.Count(dh => dh.trangthai == "Hoàn thành");
        ViewBag.dhHuy = QuanAn.tbDonHang.Count(dh => dh.trangthai == "Đã huỷ");

        // ─── Apriori: Phân tích cặp món bán chéo cho chủ quán ───
        ViewBag.AprioriInsights = await _recommendationService.GetRestaurantAprioriInsights(QuanAn.userid, 5);

        return View();
    }

    public ActionResult Wallet()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var QuanAn = getQuanAn();
        var donHangs = QuanAn.tbDonHang.ToList();
        ViewBag.donHangs = donHangs;
        ViewBag.soDu = Math.Round((double?)donHangs.Sum(dh => dh.tongtien) ?? 0, 1);
        return View();
    }

    public ActionResult Analytics()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var quanAn = getQuanAn();
        var datas = new List<DataAnalytic>();
        var dataDanhMucs = new List<DataAnalyticDanhMuc>();
        var idDanhMucs = new List<int>();

        foreach (var m in quanAn.tbMonAn)
        {
            int idDanhMuc = m.tbDanhMuc?.madanhmuc ?? 0;
            if (idDanhMuc != 0 && !idDanhMucs.Contains(idDanhMuc))
            {
                var dataDanhMuc = new DataAnalyticDanhMuc
                {
                    maDanhMuc = idDanhMuc,
                    hinhAnh = m.tbDanhMuc?.hinhanh,
                    tenDanhMuc = m.tbDanhMuc?.tendanhmuc,
                    soLuongMonAn = (from dm in db.tbDanhMuc
                                    join ma in db.tbMonAn on dm.madanhmuc equals ma.madanhmuc
                                    where ma.maquanan == quanAn.userid && dm.madanhmuc == idDanhMuc
                                    select ma).Count(),
                    tongSoLuongBanRa = (from dm in db.tbDanhMuc
                                         join ma in db.tbMonAn on dm.madanhmuc equals ma.madanhmuc
                                         join ctdh in db.tbChiTietDonHang on ma.mamon equals ctdh.mamon
                                         where ma.maquanan == quanAn.userid && dm.madanhmuc == idDanhMuc
                                         select ctdh.soluong).Sum() ?? 0,
                    doanhThu = (double?)(from dm in db.tbDanhMuc
                                         join ma in db.tbMonAn on dm.madanhmuc equals ma.madanhmuc
                                         join ctdh in db.tbChiTietDonHang on ma.mamon equals ctdh.mamon
                                         where ma.maquanan == quanAn.userid && dm.madanhmuc == idDanhMuc
                                         select ctdh.soluong * ctdh.dongia).Sum() ?? 0
                };
                dataDanhMucs.Add(dataDanhMuc);
                idDanhMucs.Add(idDanhMuc);
            }
        }

        foreach (var m in quanAn.tbMonAn)
        {
            var data = new DataAnalytic
            {
                maMonAn = m.mamon,
                giaTien = m.giatien,
                tenMonAn = m.tenmon,
                hinhAnh = m.hinhanh,
                tenDanhMuc = m.tbDanhMuc?.tendanhmuc,
                diemDanhGia = 0,
                soDanhGia = 0,
                soLuongBanDuoc = 0
            };

            var chiTietDHs = m.tbChiTietDonHang.Where(ct => ct.mamon == data.maMonAn).ToList();
            int totalDiem = 0;
            foreach (var i in chiTietDHs)
            {
                data.soLuongBanDuoc += i.soluong ?? 0;
                foreach (var tdg in i.tbDanhGia)
                {
                    data.soDanhGia += 1;
                    totalDiem += tdg.diemdanhgia ?? 0;
                }
            }
            data.diemDanhGia = data.soDanhGia == 0 ? 0 : totalDiem / data.soDanhGia;
            datas.Add(data);
        }

        datas = datas.OrderByDescending(d => d.soLuongBanDuoc).ToList();
        ViewBag.datas = datas;
        ViewBag.doanhThu = (double?)quanAn.tbDonHang.Sum(dh => dh.tongtien) ?? 0;
        ViewBag.dataDanhMucs = dataDanhMucs;
        return View();
    }

    public ActionResult Review()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var danhGias = new List<tbDanhGia>();
        var quanAn = getQuanAn();
        foreach (var i in quanAn.tbMonAn)
            foreach (var j in i.tbChiTietDonHang)
                foreach (var o in j.tbDanhGia)
                    danhGias.Add(o);
        ViewBag.danhgias = danhGias;
        return View();
    }

    public ActionResult Discount()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var quanAn = getQuanAn();

        var monAnKhuyenMais = (from ma in db.tbMonAn
                               join makm in db.tbMonAnKhuyenMai on ma.mamon equals makm.mamon
                               where ma.maquanan == quanAn.userid
                               select makm).ToList();

        ViewBag.monAns = quanAn.tbMonAn.ToList();
        ViewBag.maKM = db.tbKhuyenMai.ToList();
        ViewBag.khuyenMais = monAnKhuyenMais;
        return View();
    }

    [HttpPost]
    public ActionResult Discount(tbMonAnKhuyenMai monAnKhuyenMai)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        
        // Validation dữ liệu đầu vào
        if (monAnKhuyenMai.makm == null || monAnKhuyenMai.mamon == null || monAnKhuyenMai.phantramgiam <= 0)
        {
            TempData["DiscountError"] = "Vui lòng điền đầy đủ thông tin khuyến mãi";
            return RedirectToAction("Discount");
        }
        if (monAnKhuyenMai.phantramgiam > 100)
        {
            TempData["DiscountError"] = "Phần trăm giảm giá không thể vượt quá 100%";
            return RedirectToAction("Discount");
        }
        if (monAnKhuyenMai.soluong != null && monAnKhuyenMai.soluong <= 0)
        {
            TempData["DiscountError"] = "Số lượng mã phải lớn hơn 0";
            return RedirectToAction("Discount");
        }

        // Kiểm tra trùng lặp khuyến mãi
        var existing = db.tbMonAnKhuyenMai
            .FirstOrDefault(m => m.mamon == monAnKhuyenMai.mamon && m.makm == monAnKhuyenMai.makm);
        if (existing != null)
        {
            TempData["DiscountError"] = "Món ăn này đã có khuyến mãi này rồi";
            return RedirectToAction("Discount");
        }

        db.tbMonAnKhuyenMai.Add(monAnKhuyenMai);
        db.SaveChanges();
        TempData["DiscountSuccess"] = "Thêm khuyến mãi thành công";
        return RedirectToAction("Discount");
    }

    public ActionResult OrderList()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var quanAn = getQuanAn();
        ViewBag.donHangs = quanAn.tbDonHang.ToList();
        ViewBag.restaurantId = quanAn.userid;
        return View();
    }

    public ActionResult nhandon(int id)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var dh = db.tbDonHang.Find(id);
        if (dh != null) { dh.trangthai = "Đã xác nhận"; db.SaveChanges(); }
        return RedirectToAction("OrderList");
    }

    public ActionResult huydon(int id)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var dh = db.tbDonHang.Find(id);
        if (dh != null) { dh.trangthai = "Đã hủy"; db.SaveChanges(); }
        return RedirectToAction("OrderList");
    }

    /// <summary>
    /// Quán ăn bấm 'Chuẩn bị xong' → cập nhật trạng thái 'Chờ shipper lấy hàng'
    /// + SignalR broadcast đến TOÀN BỘ Shipper đang online (group "shippers")
    /// </summary>
    public async Task<ActionResult> hoantatdon(int id)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var dh = db.tbDonHang.Find(id);
        if (dh == null) return RedirectToAction("OrderList");

        dh.trangthai = "Chờ shipper lấy hàng";
        db.SaveChanges();

        // Load thông tin quán để gửi broadcast
        var quanAn = getQuanAn();
        try
        {
            await _hubContext.Clients.Group("shippers").SendAsync("newPickupOrder", new
            {
                orderId = dh.madh,
                restaurantName = quanAn?.tenquanan ?? "Quán ăn",
                pickupAddress = quanAn?.diachi ?? ""
            });
        }
        catch { /* SignalR broadcast không ảnh hưởng đến luồng chính */ }

        return RedirectToAction("OrderList");
    }


    public ActionResult Profile()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var quanAn = getQuanAn();
        var danhGias = new List<tbDanhGia>();
        foreach (var i in quanAn.tbMonAn)
            foreach (var j in i.tbChiTietDonHang)
                foreach (var o in j.tbDanhGia)
                    danhGias.Add(o);
        ViewBag.danhgias = danhGias;
        ViewBag.quanAn = quanAn;
        return View();
    }

    [HttpPost]
    public ActionResult Profile(tbQuanAn quanAn, IFormFile? fileAnh, string? pwd)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");

        if (fileAnh != null)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "Source/Restaurant/images/avatar");
            Directory.CreateDirectory(uploadsDir);
            var path = Path.Combine(uploadsDir, fileAnh.FileName);
            using var stream = new FileStream(path, FileMode.Create);
            fileAnh.CopyTo(stream);
            quanAn.hinhanh = fileAnh.FileName;
        }

        var quanAnOld = db.tbQuanAn.Include(q => q.tbUser).FirstOrDefault(q => q.userid == getQuanAn().userid);
        if (quanAnOld != null)
        {
            quanAnOld.tenquanan = quanAn.tenquanan;
            if (quanAn.hinhanh != null) quanAnOld.hinhanh = quanAn.hinhanh;
            if (!string.IsNullOrEmpty(pwd)) quanAnOld.tbUser.pwd = pwd;
            quanAnOld.diachi = quanAn.diachi;
            // Hash password nếu có thay đổi
            if (!string.IsNullOrEmpty(pwd) && pwd != quanAnOld.tbUser.pwd)
            {
                quanAnOld.tbUser.pwd = BCrypt.Net.BCrypt.HashPassword(pwd, workFactor: 12);
            }
            db.SaveChanges();
        }
        return RedirectToAction("Profile");
    }

    public ActionResult ProductList()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var quanAn = db.tbQuanAn.Include(q => q.tbMonAns).ThenInclude(m => m.tbDanhMuc)
            .FirstOrDefault(q => q.userid == getQuanAn().userid);
        if (quanAn == null) return RedirectToAction("Login", "Home");

        var datas = new List<DataAnalytic>();
        foreach (var m in quanAn.tbMonAn)
        {
            var data = new DataAnalytic
            {
                maMonAn = m.mamon,
                giaTien = m.giatien,
                tenMonAn = m.tenmon,
                hinhAnh = m.hinhanh,
                tenDanhMuc = m.tbDanhMuc?.tendanhmuc,
                diemDanhGia = 0,
                soDanhGia = 0,
                soLuongBanDuoc = 0
            };

            int totalDiem = 0;
            foreach (var i in m.tbChiTietDonHang)
            {
                data.soLuongBanDuoc += i.soluong ?? 0;
                foreach (var tdg in i.tbDanhGia)
                {
                    data.soDanhGia += 1;
                    totalDiem += tdg.diemdanhgia ?? 0;
                }
            }
            data.diemDanhGia = data.soDanhGia == 0 ? 0 : totalDiem / data.soDanhGia;
            data.conhang = m.conhang;
            datas.Add(data);
        }
        ViewBag.datas = datas;
        return View();
    }

    public ActionResult ProductDetail(int? id)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var monAn = id != null ? db.tbMonAn.Find(id) : new tbMonAn();
        ViewBag.monAn = monAn ?? new tbMonAn();
        return View();
    }

    [HttpPost]
    public ActionResult PostMonAn(tbMonAn monAn, IFormFile? fileAnh)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");

        if (fileAnh != null)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "Source/images/MonAn");
            Directory.CreateDirectory(uploadsDir);
            var path = Path.Combine(uploadsDir, fileAnh.FileName);
            using var stream = new FileStream(path, FileMode.Create);
            fileAnh.CopyTo(stream);
            monAn.hinhanh = fileAnh.FileName;
        }

        if (monAn.mamon == 0)
        {
            monAn.maquanan = getQuanAn().userid;
            db.tbMonAn.Add(monAn);
        }
        else
        {
            var monAnOld = db.tbMonAn.Find(monAn.mamon);
            if (monAnOld != null)
            {
                monAnOld.tenmon = monAn.tenmon;
                monAnOld.mota = monAn.mota;
                monAnOld.giatien = monAn.giatien;
                if (monAn.hinhanh != null) monAnOld.hinhanh = monAn.hinhanh;
                monAnOld.madanhmuc = monAn.madanhmuc;
            }
        }
        db.SaveChanges();
        return RedirectToAction("ProductList");
    }

    public ActionResult XoaMonAn(int? id)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        if (id != null)
        {
            var monAn = db.tbMonAn.Find(id);
            if (monAn != null) db.tbMonAn.Remove(monAn);
            db.SaveChanges();
        }
        return RedirectToAction("ProductList");
    }

    // ─── Task 2c: AJAX Toggle 1-Click Hết hàng nhanh ───
    [HttpPost]
    public async Task<JsonResult> ToggleConHang(int mamon)
    {
        if (!checkLogin())
            return Json(new { success = false, message = "Chưa đăng nhập" });

        var monAn = await db.tbMonAns.FindAsync(mamon);
        if (monAn == null)
            return Json(new { success = false, message = "Món ăn không tồn tại" });

        monAn.conhang = !monAn.conhang;  // toggle
        await db.SaveChangesAsync();

        return Json(new
        {
            success = true,
            conhang = monAn.conhang,
            message = monAn.conhang ? "🔴 Đã bật Còn hàng" : "⚪ Đã tắt (Hết hàng)"
        });
    }

    public tbQuanAn getQuanAn()
    {
        var user = GetCurrentUser();
        if (user == null) return null!;
        return db.tbQuanAn.Include(q => q.tbUser) // Load tbUser để tránh NullReferenceException
            .Include(q => q.tbMonAns).ThenInclude(m => m.tbDanhMuc)
            .Include(q => q.tbMonAns).ThenInclude(m => m.tbChiTietDonHangs).ThenInclude(c => c.tbDanhGias)
            .Include(q => q.tbDonHangs).ThenInclude(d => d.tbThongTinDatHang).ThenInclude(tt => tt.tbKhachHang)
            .FirstOrDefault(q => q.userid == user.userid) ?? null!;
    }

    private bool checkLogin()
    {
        var user = GetCurrentUser();
        return user != null && user.loaitaikhoan.Equals("Quán ăn");
    }

    public ActionResult updateStatus()
    {
        var quanAn = db.tbQuanAn.Find(getQuanAn().userid);
        if (quanAn != null)
        {
            quanAn.trangthai = quanAn.trangthai == "Đóng cửa" ? "Đang mở cửa" : "Đóng cửa";
            db.SaveChanges();
        }
        return RedirectToAction("Index");
    }
}
