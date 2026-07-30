// ============================================================
// 🏪 RestaurantController — Quản lý quán ăn | Restaurant Management
// ============================================================
// Ý nghĩa: Xử lý toàn bộ nghiệp vụ dành cho chủ quán ăn (Restaurant role)
// Chức năng: Dashboard KPI, đơn hàng (nhận/hủy/hoàn tất), món ăn CRUD,
//            doanh thu, khuyến mãi, đánh giá, profile, wallet, QR Scanner,
//            Apriori insights, realtime SignalR
// KEYWORDS: restaurant, quan an, order, menu, product, discount, review,
//           analytics, apriori, KPI, doanh thu
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   Trigger: User (role Quán ăn) đăng nhập và truy cập /Restaurant/*
//   Calls →: BaseController (GetCurrentUser, getQuanAn, checkLogin, CheckRoleJson)
//            RecommendationService (GetRestaurantAprioriInsights)
//            Chats Hub (IHubContext — newPickupOrder, kpiRefresh, orderStatusChanged)
//            Models: tbQuanAn, tbDonHang, tbMonAn, tbChiTietDonHang, tbDanhGia
//            Views: Index (Dashboard), OrderList, ProductList, Analytics, Wallet
//   Called by ←: HomeController (Login redirect) / SignalR (khi có đơn mới từ PaymentController)
//   Flow: Login → Dashboard (KPI+apriori) → OrderList → nhandon → SignalR → Shipper thấy
//        hoantatdon → SignalR newPickupOrder → Shipper FREE-PICK nhận
//        ToggleConHang (AJAX) → toggle conhang → UI update ngay
//   SignalR: Lắng nghe 'newOrder' từ PaymentController (có đơn mới)
//            Broadcast 'kpiRefresh' khi nhận/hủy/hoàn tất đơn
// ============================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using ShipFood.Hubs;
using ShipFood.Models;
using ShipFood.Services;
using ShipFood.Utils;

namespace ShipFood.Controllers;

public class RestaurantController : BaseController
{
    private readonly IWebHostEnvironment _env;
    private readonly IHubContext<Chats> _hubContext;
    private readonly RecommendationService _recommendationService;
    private readonly IMemoryCache _cache;

    public RestaurantController(dbFoodyEntities context, IWebHostEnvironment env, IHubContext<Chats> hubContext, RecommendationService recommendationService, IMemoryCache cache)
    {
        db = context;
        _env = env;
        _hubContext = hubContext;
        _recommendationService = recommendationService;
        _cache = cache;
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
        ViewBag.dhChuanBi = QuanAn.tbDonHang.Count(dh => dh.trangthai == OrderStatus.DangChuanBi);
        ViewBag.dhHoanThanh = QuanAn.tbDonHang.Count(dh => dh.trangthai == OrderStatus.HoanThanh);
        ViewBag.dhHuy = QuanAn.tbDonHang.Count(dh => dh.trangthai == OrderStatus.DaHuy);

        // ─── Apriori: Phân tích cặp món bán chéo cho chủ quán ───
        // ponytail: try-catch để RecommendationService crash không làm hỏng dashboard
        try
        {
            ViewBag.AprioriInsights = await _recommendationService.GetRestaurantAprioriInsights(QuanAn.userid, 5);
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogWarning(ex, "Apriori insights failed for restaurant {Id}", QuanAn.userid);
            ViewBag.AprioriInsights = new List<object>();
        }

        return View();
    }

    public ActionResult Wallet()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var user = GetCurrentUser();
        if (user == null) return RedirectToAction("Login", "Home");

        // ponytail: direct query tbDonHang — không dùng getQuanAn() (tránh load ALL monAns + ALL orders)
        // ponytail: không Take() — wallet ph?i hi?n t?t c? don (không m?t don)
        var donHangs = db.tbDonHang
            .Where(dh => dh.maquan == user.userid && dh.trangthai != OrderStatus.DaHuy)
            .OrderByDescending(dh => dh.ngaydathang)
            .ToList();
        ViewBag.donHangs = donHangs;
        ViewBag.soDu = Math.Round((double?)donHangs.Sum(dh => dh.tongtien) ?? 0, 1);
        ViewBag.vitien = user.vitien ?? 0;
        ViewBag.WalletSuccess = TempData["WalletSuccess"];
        ViewBag.WalletError = TempData["WalletError"];
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult NapTien(decimal soTien)
    {
        if (!checkLogin()) return Json(new { success = false });
        var user = GetCurrentUser();

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

        // ponytail: Nap tien qua chuyen khoan — tao pending deposit, cho SePay xac nhan
        var depositCode = $"FASTSHIPNAP{user!.userid}_{DateTime.Now:yyyyMMddHHmmss}";
        // 🔴 FIX Bug #5: Lưu deposit record vào DB
        try
        {
            db.tbTinNhans.Add(new tbTinNhan
            {
                noidung = $"DEPOSIT|{soTien}|{depositCode}|Đang chờ xác nhận",
                makh = user!.userid
            });
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogWarning(ex, "Failed to save deposit record for user {UserId}", user?.userid);
        }

        TempData["WalletPending"] = $"Quét mã QR để chuyển {soTien:N0}đ vào ví.";
        TempData["WalletQR"] = $"https://img.vietqr.io/image/970415-102878588446-print.png?amount={(long)soTien}&addInfo={Uri.EscapeDataString("SEVQR " + depositCode)}&accountName={Uri.EscapeDataString("BUI MINH DUC")}";
        return RedirectToAction("Wallet");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult RutTien(decimal soTien)
    {
        if (!checkLogin()) return Json(new { success = false });
        var user = GetCurrentUser();

        if (soTien < 10000)
        {
            TempData["WalletError"] = "Số tiền rút tối thiểu là 10,000đ";
            return RedirectToAction("Wallet");
        }

        var dbUser = db.tbUser.Find(user!.userid);
        if (dbUser != null)
        {
            if ((dbUser.vitien ?? 0) < soTien)
            {
                TempData["WalletError"] = $"Số dư không đủ. Hiện tại: {dbUser.vitien:N0}đ";
                return RedirectToAction("Wallet");
            }
            dbUser.vitien -= soTien;
            db.SaveChanges();
            // ponytail: Fix Item 22 — audit trail cho rut tien
            db.tbTinNhans.Add(new tbTinNhan
            {
                noidung = $"WITHDRAW|{soTien}|{dbUser.vitien}|{user!.userid}",
                makh = user.userid
            });
            db.SaveChanges();
            TempData["WalletSuccess"] = $"Rút {soTien:N0}đ thành công. Số dư mới: {dbUser.vitien:N0}đ";
        }
        return RedirectToAction("Wallet");
    }

    public async Task<ActionResult> Analytics()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");

        try
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Home");

            // ═══ Cache: data analytics ít thay đổi → cache 5 phút ═══
            // ponytail: IMemoryCache giúp tránh query DB mỗi lần load
            var cacheKey = $"Analytics_{user.userid}";
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, object>? cached))
            {
                ViewBag.datas = cached!["datas"];
                ViewBag.dataDanhMucs = cached!["dataDanhMucs"];
                ViewBag.doanhThu = cached!["doanhThu"];
                return View();
            }

            // ═══ Query 1: Món ăn + Biến thể + Danh mục (1 query duy nhất) ═══
            // ponytail: không dùng getQuanAn() — tránh load ALL orders, chỉ lấy dữ liệu cần
            var monAns = await db.tbMonAn
                .Where(m => m.maquanan == user.userid && !m.isDeleted)
                .Include(m => m.tbDanhMuc)
                .Include(m => m.tbBienTheMonAns)
                .ToListAsync();

            var bienTheIds = monAns
                .SelectMany(m => m.tbBienTheMonAns.Select(b => b.id))
                .Distinct()
                .ToList();

            // ═══ Query 2: Chi tiết đơn hàng + Đánh giá (1 query, Include tbBienTheMonAn + tbDanhGias) ═══
            // ponytail: Include tbBienTheMonAn ?? nhóm v? tbMonAn.mamon (không Include = null → sai data)
            var chiTietDHs = bienTheIds.Any()
                ? await db.tbChiTietDonHang
                    .Where(ct => ct.mamon != null && bienTheIds.Contains(ct.mamon.Value))
                    .Include(ct => ct.tbBienTheMonAn)
                    .Include(ct => ct.tbDanhGias)
                    .ToListAsync()
                : new List<tbChiTietDonHang>();

            // Nhóm chi tiết đơn theo tbMonAn.mamon (xuyên qua tbBienTheMonAn.mamon → tbMonAn.mamon)
            var chiTietTheoMon = chiTietDHs
                .GroupBy(ct => ct.tbBienTheMonAn?.mamon)
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key!.Value, g => g.ToList());

            // ═══ Tính DataAnalytic cho từng món (toàn bộ trong RAM) ═══
            var datas = new List<DataAnalytic>();
            foreach (var m in monAns)
            {
                var cts = chiTietTheoMon.GetValueOrDefault(m.mamon, new List<tbChiTietDonHang>());
                int totalDiem = 0;
                int soDanhGia = 0;
                int soLuongBan = 0;

                foreach (var ct in cts)
                {
                    soLuongBan += ct.soluong ?? 0;
                    foreach (var dg in ct.tbDanhGias)
                    {
                        soDanhGia++;
                        totalDiem += dg.diemdanhgia ?? 0;
                    }
                }

                datas.Add(new DataAnalytic
                {
                    maMonAn = m.mamon,
                    tenMonAn = m.tenmon,
                    hinhAnh = m.hinhanh,
                    tenDanhMuc = m.tbDanhMuc?.tendanhmuc,
                    giaTien = m.giatien,
                    soLuongBanDuoc = soLuongBan,
                    soDanhGia = soDanhGia,
                    diemDanhGia = soDanhGia > 0 ? totalDiem / soDanhGia : 0
                });
            }

            datas = datas.OrderByDescending(d => d.soLuongBanDuoc).ToList();
            ViewBag.datas = datas;

            // ═══ Tính DataAnalyticDanhMuc từ monAns + chiTietDHs (trong RAM) ═══
            var dataDanhMucs = monAns
                .Where(m => m.tbDanhMuc != null)
                .GroupBy(m => m.tbDanhMuc!.madanhmuc)
                .Select(g =>
                {
                    var firstDm = g.First().tbDanhMuc!;
                    var monIds = g.Select(m => m.mamon).ToHashSet();
                    var cts = chiTietDHs
                        .Where(ct => ct.tbBienTheMonAn != null && monIds.Contains(ct.tbBienTheMonAn.mamon))
                        .ToList();

                    return new DataAnalyticDanhMuc
                    {
                        maDanhMuc = firstDm.madanhmuc,
                        tenDanhMuc = firstDm.tendanhmuc,
                        hinhAnh = firstDm.hinhanh,
                        soLuongMonAn = g.Count(),
                        tongSoLuongBanRa = cts.Sum(ct => ct.soluong ?? 0),
                        // ponytail: dùng LINQ-to-Objects Sum (decimal * int → decimal → double?)
                        doanhThu = cts.Any()
                            ? (double?)(cts.Sum(ct => (ct.soluong ?? 0) * (ct.dongia ?? 0)))
                            : 0
                    };
                })
                .OrderByDescending(d => d.doanhThu)
                .ToList();

            ViewBag.dataDanhMucs = dataDanhMucs;

            // ═══ Query 3: Doanh thu — 1 query nhẹ, trừ đơn hủy ═══
            // ponytail: PostgreSQL-safe — SumAsync với ?? bên ngoài (ko bên trong expression)
            var doanhThu = (double?)(await db.tbDonHang
                .Where(dh => dh.maquan == user.userid && dh.trangthai != OrderStatus.DaHuy)
                .SumAsync(dh => (decimal?)dh.tongtien) ?? 0);
            ViewBag.doanhThu = doanhThu;

            // ═══ Ghi vào cache (5 phút) ═══
            var cacheData = new Dictionary<string, object>
            {
                ["datas"] = datas,
                ["dataDanhMucs"] = dataDanhMucs,
                ["doanhThu"] = ViewBag.doanhThu
            };
            _cache.Set(cacheKey, cacheData, TimeSpan.FromMinutes(5));

            return View();
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogError(ex, "Analytics CRASHED for restaurant {UserId}", GetCurrentUser()?.userid);
            // ponytail: redirect v? Restaurant Index, KHÔNG qua Home (tránh l?c sang view customer)
            TempData["ErrMsg"] = "Không thể tải trang phân tích. Vui lòng thử lại sau.";
            return RedirectToAction("Index", "Restaurant");
        }
    }

    public ActionResult Review()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        try
        {
            var quanAn = getQuanAn();
            if (quanAn == null) return RedirectToAction("Logout", "Home");

            var maMonQuan = quanAn.tbMonAn.Select(m => m.mamon).ToList();
            if (!maMonQuan.Any())
            {
                ViewBag.danhgias = new List<tbDanhGia>();
                return View();
            }

            // ponytail: PostgreSQL-safe — JOIN thay vì navigation chain nullable
            // Bước 1: Lấy review IDs qua 2 JOIN (không dùng navigation chain)
            var reviewIds = (from d in db.tbDanhGia
                             join ct in db.tbChiTietDonHang on d.mactdh equals ct.mactdh
                             join bt in db.tbBienTheMonAn on ct.mamon equals bt.id
                             where maMonQuan.Contains(bt.mamon)
                             select d.madg).Distinct().ToList();

            // Bước 2: Load reviews với Include từ danh sách IDs
            var danhGias = db.tbDanhGia
                .Where(d => reviewIds.Contains(d.madg))
                .Include(d => d.tbChiTietDonHang).ThenInclude(ct => ct.tbDonHang).ThenInclude(dh => dh.tbThongTinDatHang).ThenInclude(tt => tt.tbKhachHang)
                .Include(d => d.tbChiTietDonHang).ThenInclude(ct => ct.tbBienTheMonAn).ThenInclude(b => b.tbMonAn)
                .OrderByDescending(d => d.madg)
                .ToList();

            ViewBag.danhgias = danhGias;
            return View();
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogError(ex, "Review CRASHED: {Message}", ex.Message);
            TempData["ErrMsg"] = "Không thể tải đánh giá. Vui lòng thử lại sau.";
            return RedirectToAction("Index");
        }
    }

    public ActionResult Discount()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var quanAn = getQuanAn();
        // ponytail: fix NullReferenceException — getQuanAn() tra ve null! khi khong co tbQuanAn record
        if (quanAn == null)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogWarning("Discount: getQuanAn() returned null for user {UserId}", GetCurrentUser()?.userid);
            TempData["DiscountError"] = "Không tìm thấy thông tin quán ăn. Vui lòng liên hệ admin.";
            return RedirectToAction("Index");
        }

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
    [ValidateAntiForgeryToken]
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

    // ponytail: Accept FromDate/ToDate + status query params for server-side filtering
    // ponytail: fix — thêm status ? server-side ?? b? l?c còn ?úng sau reload
    public ActionResult OrderList(DateTime? fromDate, DateTime? toDate, string? status)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var user = GetCurrentUser();
        if (user == null) return RedirectToAction("Login", "Home");

        // ponytail: direct query — không dùng getQuanAn() (tránh load ALL monAns)
        var query = db.tbDonHang
            .Include(d => d.tbThongTinDatHang)
            .Where(dh => dh.maquan == user.userid)
            .AsQueryable();

        // Date filter (server-side SQL WHERE)
        if (fromDate.HasValue)
            query = query.Where(dh => dh.ngaydathang >= fromDate.Value.Date);
        if (toDate.HasValue)
            query = query.Where(dh => dh.ngaydathang <= toDate.Value.Date.AddDays(1));

        // ═══ Status filter (server-side) — trước ?ây ch? l?c client-side DataTable ═══
        // ponytail: filter server ?? b?o toàn b? l?c sau reload, ko m?t filter khi submit date
        if (!string.IsNullOrEmpty(status))
            query = query.Where(dh => dh.trangthai == status);

        ViewBag.donHangs = query.OrderByDescending(dh => dh.ngaydathang).ToList();
        ViewBag.restaurantId = user.userid;
        ViewBag.fromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.toDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.currentStatus = status ?? "";
        return View();
    }

    public async Task<ActionResult> nhandon(int id)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var quanAn = getQuanAn();
        // ponytail: fix NullReferenceException — getQuanAn() co the null
        if (quanAn == null)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogWarning("nhandon: getQuanAn() returned null");
            return RedirectToAction("OrderList");
        }
        var dh = db.tbDonHang.Include(d => d.tbThongTinDatHang).FirstOrDefault(d => d.madh == id && d.maquan == quanAn.userid);
        if (dh != null)
        {
            dh.trangthai = OrderStatus.DaXacNhan;
            db.SaveChanges();

            // ═══ Auto-sinh tin nhắn khi quán xác nhận đơn ═══
            try
            {
                db.tbTinNhans.Add(new tbTinNhan
                {
                    madh = dh.madh,
                    noidung = OrderStatus.AutoMessages[OrderStatus.DaXacNhan],
                    makh = dh.tbThongTinDatHang?.userid,
                    mashipper = null
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
                logger.LogWarning(ex, "nhandon auto-message failed for order #{OrderId}", dh?.madh);
            }

            // SignalR broadcast real-time đến khách hàng
            try
            {
                await _hubContext.Clients.Group($"order_{id}").SendAsync("orderStatusChanged", id, OrderStatus.DaXacNhan, DateTime.Now.ToString("HH:mm"));
                // Dashboard realtime: push KPI refresh đến restaurant dashboard
                await _hubContext.Clients.Group($"restaurant_{quanAn.userid}").SendAsync("kpiRefresh");
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
                logger.LogWarning(ex, "nhandon SignalR broadcast failed for order #{OrderId}", id);
            }
        }
        return RedirectToAction("OrderList");
    }

    public async Task<ActionResult> huydon(int id)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var quanAn = getQuanAn();
        // ponytail: fix NullReferenceException — getQuanAn() co the null
        if (quanAn == null)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogWarning("huydon: getQuanAn() returned null");
            return RedirectToAction("OrderList");
        }
        var dh = db.tbDonHang.FirstOrDefault(d => d.madh == id && d.maquan == quanAn.userid);
        if (dh != null)
        {
            dh.trangthai = OrderStatus.DaHuy;
            db.SaveChanges();

            // 🔴 FIX Bug #4: Null check cho dh.mattdh trước khi dùng
            // ═══ Auto-sinh tin nhắn khi hủy đơn ═══
            try
            {
                int? customerUserId = null;
                if (dh.mattdh != null)
                {
                    var ttdh = db.tbThongTinDatHangs.Find(dh.mattdh);
                    customerUserId = ttdh?.userid;
                }
                db.tbTinNhans.Add(new tbTinNhan
                {
                    madh = dh.madh,
                    noidung = OrderStatus.AutoMessages[OrderStatus.DaHuy],
                    makh = customerUserId,
                    mashipper = null
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
                logger.LogWarning(ex, "huydon auto-message failed for order #{OrderId}", dh?.madh);
            }

            // SignalR broadcast real-time đến khách hàng
            try
            {
                await _hubContext.Clients.Group($"order_{id}").SendAsync("orderStatusChanged", id, OrderStatus.DaHuy, DateTime.Now.ToString("HH:mm"));
                // Dashboard realtime: push KPI refresh đến restaurant dashboard
                await _hubContext.Clients.Group($"restaurant_{quanAn.userid}").SendAsync("kpiRefresh");
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
                logger.LogWarning(ex, "huydon SignalR broadcast failed for order #{OrderId}", id);
            }
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

        dh.trangthai = OrderStatus.ChoShipper;
        db.SaveChanges();

        // ═══ Auto-sinh tin nhắn khi quán chuẩn bị xong ═══
        try
        {
            int? customerUserId = null;
            if (dh.mattdh != null)
            {
                var ttdh = db.tbThongTinDatHangs.Find(dh.mattdh);
                customerUserId = ttdh?.userid;
            }
            db.tbTinNhans.Add(new tbTinNhan
            {
                madh = dh.madh,
                noidung = OrderStatus.AutoMessages[OrderStatus.ChoShipper],
                makh = customerUserId,
                mashipper = null
            });
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogWarning(ex, "hoantatdon auto-message failed for order #{OrderId}", dh?.madh);
        }

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

            // Broadcast real-time đến khách hàng
            await _hubContext.Clients.Group($"order_{dh.madh}").SendAsync("orderStatusChanged", dh.madh, OrderStatus.ChoShipper, DateTime.Now.ToString("HH:mm"));
            // Dashboard realtime: push KPI refresh đến restaurant dashboard
            await _hubContext.Clients.Group($"restaurant_{quanAn?.userid ?? 0}").SendAsync("kpiRefresh");
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogWarning(ex, "hoantatdon SignalR broadcast failed for order #{OrderId}", dh?.madh);
        }

        return RedirectToAction("OrderList");
    }


    public ActionResult Profile()
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var quanAn = getQuanAn();
        if (quanAn == null) return RedirectToAction("Login", "Home");

        // ponytail: PostgreSQL-safe — JOIN thay vì navigation chain nullable (same fix as Review)
        var maMonQuan = quanAn.tbMonAn.Select(m => m.mamon).ToList();
        var reviewIds = maMonQuan.Any()
            ? (from d in db.tbDanhGia
               join ct in db.tbChiTietDonHang on d.mactdh equals ct.mactdh
               join bt in db.tbBienTheMonAn on ct.mamon equals bt.id
               where maMonQuan.Contains(bt.mamon)
               select d.madg).Distinct().ToList()
            : new List<int>();

        var danhGias = reviewIds.Any()
            ? db.tbDanhGia
                .Where(d => reviewIds.Contains(d.madg))
                .Include(d => d.tbChiTietDonHang).ThenInclude(ct => ct.tbDonHang).ThenInclude(dh => dh.tbThongTinDatHang).ThenInclude(tt => tt.tbKhachHang)
                .Include(d => d.tbChiTietDonHang).ThenInclude(ct => ct.tbBienTheMonAn).ThenInclude(b => b.tbMonAn)
                .OrderByDescending(d => d.madg)
                .ToList()
            : new List<tbDanhGia>();

        ViewBag.danhgias = danhGias;
        ViewBag.quanAn = quanAn;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Profile(tbQuanAn quanAn, IFormFile? fileAnh, string? pwd)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");

        if (fileAnh != null)
        {
            // ponytail: Fix Item 13 — path traversal protection
            var uploadsDir = Path.Combine(_env.WebRootPath, "Source/Restaurant/images/avatar");
            Directory.CreateDirectory(uploadsDir);
            var safeFileName = Path.GetFileName(fileAnh.FileName);
            var path = Path.Combine(uploadsDir, safeFileName);
            using var stream = new FileStream(path, FileMode.Create);
            fileAnh.CopyTo(stream);
            quanAn.hinhanh = safeFileName;
        }

        // ponytail: fix NullReferenceException — lay userid truoc, tranh getQuanAn() null
        var currentQuanAn = getQuanAn();
        if (currentQuanAn == null)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogWarning("Profile POST: getQuanAn() returned null");
            return RedirectToAction("Profile");
        }
        var quanAnOld = db.tbQuanAn.Include(q => q.tbUser).FirstOrDefault(q => q.userid == currentQuanAn.userid);
        if (quanAnOld != null)
        {
            quanAnOld.tenquanan = quanAn.tenquanan;
            if (quanAn.hinhanh != null) quanAnOld.hinhanh = quanAn.hinhanh;
            quanAnOld.diachi = quanAn.diachi;
            // ponytail: plain-text password
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
        try
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Home");

            // ponytail: PostgreSQL-safe — chia Include 3 levels thành 2 queries
            // Bước 1: Load monAn + tbDanhMuc + tbBienTheMonAns (2 levels, tránh Cartesian explosion)
            var monAns = db.tbMonAn
                .Where(m => m.maquanan == user.userid && !m.isDeleted)
                .Include(m => m.tbDanhMuc)
                .Include(m => m.tbBienTheMonAns)
                .ToList();

            // Bước 2: Load chiTietDonHangs + tbDanhGias riêng biệt
            var bienTheIds = monAns.SelectMany(m => m.tbBienTheMonAns.Select(b => b.id)).ToList();
            var chiTietDHs = bienTheIds.Any()
                ? db.tbChiTietDonHang
                    .Where(ct => ct.mamon != null && bienTheIds.Contains(ct.mamon.Value))
                    .Include(ct => ct.tbDanhGias)
                    .ToList()
                : new List<tbChiTietDonHang>();

            var datas = new List<DataAnalytic>();
            foreach (var m in monAns)
            {
                var bienTheIdsForMon = m.tbBienTheMonAns.Select(b => b.id).ToHashSet();
                var chiTietMonAn = chiTietDHs.Where(ct => ct.mamon != null && bienTheIdsForMon.Contains(ct.mamon.Value)).ToList();

                int totalDiem = 0;
                int soDanhGia = 0;
                int soLuongBan = 0;

                foreach (var ct in chiTietMonAn)
                {
                    soLuongBan += ct.soluong ?? 0;
                    foreach (var dg in ct.tbDanhGias)
                    {
                        soDanhGia++;
                        totalDiem += dg.diemdanhgia ?? 0;
                    }
                }

                datas.Add(new DataAnalytic
                {
                    maMonAn = m.mamon,
                    giaTien = m.giatien,
                    tenMonAn = m.tenmon,
                    hinhAnh = m.hinhanh,
                    tenDanhMuc = m.tbDanhMuc?.tendanhmuc,
                    diemDanhGia = soDanhGia > 0 ? totalDiem / soDanhGia : 0,
                    soDanhGia = soDanhGia,
                    soLuongBanDuoc = soLuongBan,
                    conhang = m.conhang
                });
            }
            ViewBag.datas = datas;
            return View();
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogError(ex, "ProductList CRASHED: {Message}", ex.Message);
            TempData["ErrMsg"] = "Không thể tải danh sách món ăn. Vui lòng thử lại sau.";
            return RedirectToAction("Index");
        }
    }

    public ActionResult ProductDetail(int? id)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var monAn = id != null ? db.tbMonAn.Find(id) : new tbMonAn();
        ViewBag.monAn = monAn ?? new tbMonAn();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult PostMonAn(tbMonAn monAn, IFormFile? fileAnh,
        decimal? giatien, string? sizeM, decimal? giaM,
        string? sizeL, decimal? giaL,
        string? sizeXL, decimal? giaXL)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");

        if (fileAnh != null)
        {
            // ponytail: Fix Item 13 — path traversal protection
            var uploadsDir = Path.Combine(_env.WebRootPath, "Source/images/MonAn");
            Directory.CreateDirectory(uploadsDir);
            var safeFileName = Path.GetFileName(fileAnh.FileName);
            var path = Path.Combine(uploadsDir, safeFileName);
            using var stream = new FileStream(path, FileMode.Create);
            fileAnh.CopyTo(stream);
            monAn.hinhanh = safeFileName;
        }

        if (monAn.mamon == 0)
        {
            // ponytail: fix NullReferenceException — getQuanAn() co the null
            var quanAnForProduct = getQuanAn();
            if (quanAnForProduct == null)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
                logger.LogWarning("PostMonAn: getQuanAn() returned null");
                return RedirectToAction("ProductList");
            }
            monAn.maquanan = quanAnForProduct.userid;
            db.tbMonAn.Add(monAn);
            db.SaveChanges();

            // ─── Tạo biến thể (size) mặc định ───
            db.tbBienTheMonAn.Add(new tbBienTheMonAn
            {
                mamon = monAn.mamon,
                size = "M",
                giatien = giatien ?? 0
            });

            // Size L nếu có
            if (!string.IsNullOrEmpty(sizeL) && giaL > 0)
            {
                db.tbBienTheMonAn.Add(new tbBienTheMonAn
                {
                    mamon = monAn.mamon,
                    size = sizeL,
                    giatien = giaL
                });
            }
            // Size XL nếu có
            if (!string.IsNullOrEmpty(sizeXL) && giaXL > 0)
            {
                db.tbBienTheMonAn.Add(new tbBienTheMonAn
                {
                    mamon = monAn.mamon,
                    size = sizeXL,
                    giatien = giaXL
                });
            }
        }
        else
        {
            var monAnOld = db.tbMonAn.Find(monAn.mamon);
            if (monAnOld != null)
            {
                monAnOld.tenmon = monAn.tenmon;
                monAnOld.mota = monAn.mota;
                if (monAn.hinhanh != null) monAnOld.hinhanh = monAn.hinhanh;
                monAnOld.madanhmuc = monAn.madanhmuc;

                // Cập nhật biến thể M (mặc định)
                var bienTheM = db.tbBienTheMonAn.FirstOrDefault(b => b.mamon == monAn.mamon && b.size == "M");
                if (bienTheM != null && giatien > 0)
                    bienTheM.giatien = giatien;
                else if (bienTheM == null && giatien > 0)
                    db.tbBienTheMonAn.Add(new tbBienTheMonAn { mamon = monAn.mamon, size = "M", giatien = giatien });

                // Cập nhật size L
                UpdateOrCreateBienThe(monAn.mamon, sizeL, giaL);
                // Cập nhật size XL
                UpdateOrCreateBienThe(monAn.mamon, sizeXL, giaXL);
            }
        }
        db.SaveChanges();
        return RedirectToAction("ProductList");
    }

    /// <summary>
    /// Helper: tạo mới hoặc cập nhật biến thể theo size
    /// </summary>
    private void UpdateOrCreateBienThe(int mamon, string? size, decimal? gia)
    {
        if (string.IsNullOrEmpty(size) || gia == null || gia <= 0) return;
        var existing = db.tbBienTheMonAn.FirstOrDefault(b => b.mamon == mamon && b.size == size);
        if (existing != null)
            existing.giatien = gia;
        else
            db.tbBienTheMonAn.Add(new tbBienTheMonAn { mamon = mamon, size = size, giatien = gia });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult XoaMonAn(int id)
    {
        if (!checkLogin()) return RedirectToAction("Login", "Home");
        var monAn = db.tbMonAn.Find(id);
        if (monAn != null)
        {
            // ⚠️ Soft delete: chỉ đánh dấu isDeleted = true, không xóa cứng
            // Giúp bảo toàn lịch sử hóa đơn trong tbChiTietDonHang
            monAn.isDeleted = true;
        }
        db.SaveChanges();
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
    [ValidateAntiForgeryToken]
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
        // ponytail: Không load tbChiTietDonHangs→tbDanhGias qua chain — quá nặng (hàng nghìn records)
        // Các trang cần data detail (Analytics, Review) tự query riêng qua DbSet
        return db.tbQuanAn.Include(q => q.tbUser)
            .Include(q => q.tbMonAns).ThenInclude(m => m.tbDanhMuc)
            .Include(q => q.tbMonAns).ThenInclude(m => m.tbBienTheMonAns)
            .Include(q => q.tbDonHangs).ThenInclude(d => d.tbThongTinDatHang).ThenInclude(tt => tt.tbKhachHang)
            .FirstOrDefault(q => q.userid == user.userid) ?? null!;
    }

    private bool checkLogin()
    {
        var user = GetCurrentUser();
        // ponytail: dung == thay .Equals() de tranh NullReferenceException khi loaitaikhoan null
        return user != null && user.loaitaikhoan == "Quán ăn";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult updateStatus()
    {
        // ponytail: fix NullReferenceException — getQuanAn() co the null
        var currentUserForStatus = getQuanAn();
        if (currentUserForStatus == null)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RestaurantController>>();
            logger.LogWarning("updateStatus: getQuanAn() returned null");
            return RedirectToAction("Index");
        }
        var quanAn = db.tbQuanAn.Find(currentUserForStatus.userid);
        if (quanAn != null)
        {
            quanAn.trangthai = quanAn.trangthai == OrderStatus.DongCua ? OrderStatus.DangMoCua : OrderStatus.DongCua;
            db.SaveChanges();
        }
        return RedirectToAction("Index");
    }
}
