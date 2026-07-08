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
        if (QuanAn == null)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogWarning("Restaurant user logged in but no tbQuanAn record found for userId {UserId}", GetCurrentUser()?.userid);
            return RedirectToAction("Logout", "Home");
        }

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
                                         join b in db.tbBienTheMonAn on ma.mamon equals b.mamon
                                         join ctdh in db.tbChiTietDonHang on b.id equals ctdh.mamon
                                         where ma.maquanan == quanAn.userid && dm.madanhmuc == idDanhMuc
                                         select ctdh.soluong).Sum() ?? 0,
                    doanhThu = (double?)(from dm in db.tbDanhMuc
                                         join ma in db.tbMonAn on dm.madanhmuc equals ma.madanhmuc
                                         join b in db.tbBienTheMonAn on ma.mamon equals b.mamon
                                         join ctdh in db.tbChiTietDonHang on b.id equals ctdh.mamon
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

            var bientheIds = m.tbBienTheMonAn.Select(b => b.id).ToList();
            var chiTietDHs = db.tbChiTietDonHang.Where(ct => ct.mamon != null && bientheIds.Contains(ct.mamon.Value)).ToList();
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
                               join b in db.tbBienTheMonAn on ma.mamon equals b.mamon
                               join makm in db.tbMonAnKhuyenMai on b.id equals makm.mamon
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

    public async Task<ActionResult> nhandon(int id)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var dh = db.tbDonHang.Find(id);
        if (dh != null)
        {
            dh.trangthai = "Đã xác nhận";
            db.SaveChanges();

            // SignalR broadcast real-time đến khách hàng
            try
            {
                await _hubContext.Clients.Group($"order_{id}").SendAsync("orderStatusChanged", id, "Đã xác nhận", DateTime.Now.ToString("HH:mm"));
            }
            catch { }
        }
        return RedirectToAction("OrderList");
    }

    public async Task<ActionResult> huydon(int id)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var dh = db.tbDonHang.Find(id);
        if (dh != null)
        {
            var oldStatus = dh.trangthai;
            dh.trangthai = "Đã hủy";

            // ─── MoMo Refund: Nếu đơn đã thanh toán qua MoMo, tự động hoàn tiền ───
            bool isMoMoPayment = dh.hinhthucthanhtoan == 3 || dh.hinhthucthanhtoan == 5;
            if (isMoMoPayment && dh.tongtien > 0 && (oldStatus == "Đã đặt" || oldStatus == "Đã xác nhận" || oldStatus == "Đã thanh toán"))
            {
                try
                {
                    var moMoService = HttpContext.RequestServices.GetRequiredService<ShipFood.Services.MoMoService>();
                    // Đọc momo_trans_id đã lưu từ IPN callback (nếu có)
                    long? transId = null;
                    if (!string.IsNullOrEmpty(dh.momo_trans_id) && long.TryParse(dh.momo_trans_id, out var parsedTransId))
                    {
                        transId = parsedTransId;
                    }
                    var refundResult = await moMoService.RefundAsync(
                        orderId: $"FS{dh.madh}_{DateTime.Now:yyyyMMddHHmmss}",
                        amount: (long)(dh.tongtien * 1000),
                        description: $"Hoàn tiền đơn hàng FastShip #{dh.madh}",
                        transId: transId
                    );
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
                    if (refundResult.IsSuccess)
                    {
                        logger.LogInformation("MoMo refund successful for order #{OrderId}, amount: {Amount}", dh.madh, dh.tongtien);
                    }
                    else
                    {
                        logger.LogWarning("MoMo refund failed for order #{OrderId}: {Message}", dh.madh, refundResult.Message);
                    }
                }
                catch (Exception refundEx)
                {
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
                    logger.LogError(refundEx, "MoMo refund failed for order #{OrderId}", dh.madh);
                }
            }

            db.SaveChanges();

            // SignalR broadcast real-time đến khách hàng
            try
            {
                await _hubContext.Clients.Group($"order_{id}").SendAsync("orderStatusChanged", id, "Đã hủy", DateTime.Now.ToString("HH:mm"));
            }
            catch { }
        }
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
        var quanAn = getQuanAn();            try
            {
                await _hubContext.Clients.Group("shippers").SendAsync("newPickupOrder", new
                {
                    orderId = dh.madh,
                    restaurantName = quanAn?.tenquanan ?? "Quán ăn",
                    pickupAddress = quanAn?.diachi ?? ""
                });

                // Phase 4: Broadcast real-time đến khách hàng
                await _hubContext.Clients.Group($"order_{dh.madh}").SendAsync("orderStatusChanged", dh.madh, "Chờ shipper lấy hàng", DateTime.Now.ToString("HH:mm"));
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
            quanAnOld.diachi = quanAn.diachi;
            if (!string.IsNullOrEmpty(pwd))
            {
                quanAnOld.tbUser.pwd = pwd;
            }
            db.SaveChanges();
        }
        return RedirectToAction("Profile");
    }

    public ActionResult ProductList()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var quanAn = db.tbQuanAn.Include(q => q.tbMonAns).ThenInclude(m => m.tbDanhMuc)
            .Include(q => q.tbMonAns).ThenInclude(m => m.tbBienTheMonAns)
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
                // Giá được quản lý qua tbBienTheMonAn, không sửa trực tiếp ở tbMonAn
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
            if (monAn != null)
            {
                // ⚠️ Soft delete: chỉ đánh dấu isDeleted = true, không xóa cứng
                // Giúp bảo toàn lịch sử hóa đơn trong tbChiTietDonHang
                monAn.isDeleted = true;
            }
            db.SaveChanges();
        }
        return RedirectToAction("ProductList");
    }

    // ─── Phản hồi đánh giá ───
    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult ReplyReview(int madg, string phanHoi)
    {
        if (!checkLogin()) return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (string.IsNullOrWhiteSpace(phanHoi) || phanHoi.Length < 2)
            return Json(new { success = false, message = "Phản hồi phải có ít nhất 2 ký tự" });
        if (phanHoi.Length > 500)
            return Json(new { success = false, message = "Phản hồi không được quá 500 ký tự" });

        var danhGia = db.tbDanhGia.Find(madg);
        if (danhGia == null)
            return Json(new { success = false, message = "Đánh giá không tồn tại" });

        danhGia.phanHoiCuaQuan = phanHoi.Trim();
        db.SaveChanges();

        return Json(new { success = true, message = "Đã gửi phản hồi thành công" });
    }

    // ─── Task 2c: AJAX Toggle 1-Click Hết hàng nhanh ───
    [HttpPost]
    public async Task<JsonResult> ToggleConHang(int mamon)
    {
        var roleCheck = CheckRoleJson("Quán ăn");
        if (roleCheck != null) return roleCheck;

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
            .Include(q => q.tbMonAns).ThenInclude(m => m.tbBienTheMonAns).ThenInclude(b => b.tbChiTietDonHangs).ThenInclude(c => c.tbDanhGias)
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
