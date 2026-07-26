using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShipFood.Hubs;
using ShipFood.Models;
using ShipFood.Services;
using ShipFood.Utils;

namespace ShipFood.Controllers;

public class ShipperController : BaseController
{
    private readonly IHubContext<Chats> _hubContext;
    private readonly EDeliveryService _eDelivery;

    public ShipperController(dbFoodyEntities context, IHubContext<Chats> hubContext, EDeliveryService eDelivery)
    {
        db = context;
        _hubContext = hubContext;
        _eDelivery = eDelivery;
    }

    private bool checkShipper()
    {
        var user = GetCurrentUser();
        return user != null && user.loaitaikhoan.Equals("Shipper");
    }

    public ActionResult Index()
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return RedirectToAction("Login", "Home");

        // LINQ thay vì FromSqlRaw — tránh PostgreSQL column mapping lỗi
        List<DonHangDangLam> listdh;
        try
        {
            listdh = db.tbDonHang
                .AsNoTracking()
                .Where(dh => dh.trangthai == "Chờ shipper lấy hàng" && dh.mashipper == null)
                .Join(db.tbThongTinDatHang, dh => dh.mattdh, tt => tt.mattdh, (dh, tt) => new { dh, tt })
                .Join(db.tbQuanAn, x => x.dh.maquan, qa => qa.userid, (x, qa) => new DonHangDangLam
                {
                    madh = x.dh.madh,
                    ngaydathang = x.dh.ngaydathang,
                    diachi = x.tt.diachi ?? "",
                    tennguoinhan = x.tt.tennguoinhan ?? "",
                    trangthai = x.dh.trangthai ?? "",
                    phiship = x.dh.phiship ?? 0,
                    tongtien = x.dh.tongtien ?? 0,
                    userid = x.tt.userid ?? 0,
                    sdt = x.tt.sdt ?? "",
                    tenquanan = qa.tenquanan ?? "",
                    DiaChiQuan = qa.diachi ?? ""
                })
                .OrderByDescending(d => d.madh)
                .ToList();
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ShipperController>>();
            logger.LogError(ex, "Shipper dashboard Index failed: {Error}", ex.Message);
            listdh = new List<DonHangDangLam>();
        }

        // Data for redesigned view
        var shipperInfo = db.tbShipper.Find(sh.userid);
        ViewBag.ShipperInfo = shipperInfo;
        ViewBag.ShipperUserId = sh.userid;

        // ─── Load thêm các orders của shipper này (không chỉ FREE-PICK) ───
        var todayStart = DateTime.Now.Date;
        var myOrders = db.tbDonHang
            .Include(d => d.tbQuanAn)
            .Where(dh => dh.mashipper == sh.userid)
            .OrderByDescending(dh => dh.ngaydathang)
            .Take(20)
            .ToList();
        ViewBag.MyOrders = myOrders;

        var todayOrders = db.tbDonHang.Count(dh => dh.mashipper == sh.userid && dh.ngaydathang >= todayStart);
        var todayIncome = db.tbDonHang
            .Where(dh => dh.mashipper == sh.userid && dh.trangthai == OrderStatus.HoanThanh
                && dh.ngaythanhtoan != null && dh.ngaythanhtoan >= todayStart)
            .Sum(dh => (decimal?)dh.phiship) ?? 0;
        ViewBag.TodayOrders = todayOrders;
        ViewBag.TodayIncome = todayIncome;

        return View(listdh);
    }

    public ActionResult ThuNhap()
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return RedirectToAction("Login", "Home");

        DateTime currentDate = DateTime.Now;
        DateTime todayStart = currentDate.Date;
        DateTime todayEnd = todayStart.AddDays(1);
        DateTime thirtyDaysAgo = todayStart.AddDays(-30);
        var shipper = db.tbUser.Find(sh.userid);

        var listdh30 = db.tbDonHang.Where(dh => dh.mashipper == sh.userid && dh.ngaythanhtoan >= thirtyDaysAgo && dh.ngaythanhtoan <= currentDate).ToList();
        var listdhhoanthanh30 = listdh30.Where(l => l.trangthai == OrderStatus.HoanThanh).ToList();
        var thunhap30 = listdhhoanthanh30.Sum(list => list.phiship) ?? 0;
        int dh30 = listdh30.Count;

        var listdhhn = db.tbDonHang.Where(dh => dh.mashipper == sh.userid && dh.ngaythanhtoan >= todayStart && dh.ngaythanhtoan < todayEnd).ToList();
        var listdhhthn = listdhhn.Where(l => l.trangthai == OrderStatus.HoanThanh).ToList();
        var listdhdhhn = listdhhn.Where(l => l.trangthai == OrderStatus.DaHuy).ToList();
        var thunhaphn = listdhhthn.Sum(list => list.phiship) ?? 0;
        var dhhthn = listdhhthn.Count;
        var dhdhhn = listdhdhhn.Count;

        ViewBag.thunhap30 = thunhap30;
        ViewBag.dh30 = dh30;
        ViewBag.thunhaphn = thunhaphn;
        ViewBag.dhhthn = dhhthn;
        ViewBag.dhdhhn = dhdhhn;

        return View(shipper ?? new tbUser());
    }

    public ActionResult ThongBao()
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return RedirectToAction("Login", "Home");
        var orders = db.tbDonHang
            .Where(dh => dh.mashipper == sh.userid)
            .OrderByDescending(dh => dh.ngaydathang)
            .Take(20)
            .ToList();
        ViewBag.Notifications = orders;
        return View();
    }

    public ActionResult LichSu()
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return RedirectToAction("Login", "Home");
        // 🔴 FIX Bug #3: Pagination — dùng tham số page từ querystring, ko hardcode
        int pageSize = 50;
        int page = Request.Query.TryGetValue("page", out var pageVal) && int.TryParse(pageVal, out var p) ? p : 1;
        if (page < 1) page = 1;
        var listdh = db.tbDonHang
            .Where(dh => dh.mashipper == sh.userid)
            .Include(d => d.tbThongTinDatHang)
            .OrderByDescending(d => d.madh)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)db.tbDonHang.Count(dh => dh.mashipper == sh.userid) / pageSize);
        return View(listdh);
    }

    public ActionResult CaiDat()
    {
        var user = GetCurrentUser();
        if (user == null || !checkShipper()) return RedirectToAction("Login", "Home");
        var shipper = db.tbUser.Include(s => s.tbShipper).FirstOrDefault(s => s.userid == user.userid);
        return View(shipper ?? new tbUser());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult CaiDat(tbUser user, string? diachi)
    {
        var shipper = GetCurrentUser();
        if (shipper == null || !checkShipper()) return RedirectToAction("Login", "Home");

        var existingUser = db.tbUser.Find(shipper.userid); // Fix IDOR: use session ID
        if (existingUser != null)
        {
            existingUser.sdt = user.sdt;
            // Chỉ cập nhật password nếu có thay đổi (không rỗng)
            // ponytail: plain-text password
            if (!string.IsNullOrEmpty(user.pwd))
            {
                existingUser.pwd = user.pwd;
            }
            db.SaveChanges();
        }
        return RedirectToAction("CaiDat");
    }

    public ActionResult ViTien()
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return RedirectToAction("Login", "Home");
        var shipper = db.tbUser.Find(sh.userid);
        var listdonhang = db.tbDonHang.Where(dh => dh.mashipper == sh.userid).ToList();
        ViewBag.listdh = listdonhang;
        ViewBag.NapTienSuccess = TempData["NapTienSuccess"];
        ViewBag.NapTienError = TempData["NapTienError"];
        ViewBag.RutTienSuccess = TempData["RutTienSuccess"];
        ViewBag.RutTienError = TempData["RutTienError"];
        return View(shipper ?? new tbUser());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult NapTien(decimal soTien)
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (soTien < 10000)
        {
            TempData["NapTienError"] = "Số tiền nạp tối thiểu là 10,000đ";
            return RedirectToAction("ViTien");
        }
        if (soTien > 100000000)
        {
            TempData["NapTienError"] = "Số tiền nạp tối đa là 100,000,000đ";
            return RedirectToAction("ViTien");
        }

        // ponytail: Fix Item 7 — dung config BANK_ACCOUNT thay vi hardcode
        var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var bankId = configuration["BANK_ID"] ?? "970436";
        var bankAccountNo = configuration["BANK_ACCOUNT_NO"] ?? "1234567890";
        var bankAccountName = configuration["BANK_ACCOUNT_NAME"] ?? "FASTSHIP CO., LTD";
        var binCode = ShipFood.Helpers.BankHelper.GetVietQrBinCode(bankId);
        
        var depositCode = $"FASTSHIPNAP{sh.userid}_{DateTime.Now:yyyyMMddHHmmss}";
        TempData["NapTienPending"] = $"Quét mã QR để chuyển {soTien:N0}đ vào ví.";
        TempData["NapTienQR"] = $"https://img.vietqr.io/image/{binCode}-{bankAccountNo}-print.png?amount={(long)soTien}&addInfo={Uri.EscapeDataString("SEVQR " + depositCode)}&accountName={Uri.EscapeDataString(bankAccountName)}";
        TempData["NapTienSoTien"] = soTien.ToString();
        TempData["NapTienDepositCode"] = depositCode;
        return RedirectToAction("ViTien");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult RutTien(decimal soTien)
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return Json(new { success = false, message = "Vui lòng đăng nhập" });

        if (soTien < 10000)
        {
            TempData["RutTienError"] = "Số tiền rút tối thiểu là 10,000đ";
            return RedirectToAction("ViTien");
        }

        var user = db.tbUser.Find(sh.userid);
        if (user != null)
        {
            if ((user.vitien ?? 0) < soTien)
            {
                TempData["RutTienError"] = $"Số dư không đủ. Hiện tại: {user.vitien:N0}đ";
                return RedirectToAction("ViTien");
            }

            user.vitien -= soTien;
            db.SaveChanges();

            db.tbTinNhans.Add(new tbTinNhan
            {
                noidung = $"💸 Rút tiền: -{soTien:N0}đ. Số dư mới: {user.vitien:N0}đ",
                makh = sh.userid,
                mashipper = sh.userid
            });
            db.SaveChanges();

            TempData["RutTienSuccess"] = $"Yêu cầu rút {soTien:N0}đ đã được ghi nhận! Vui lòng chờ admin xử lý.";
        }
        return RedirectToAction("ViTien");
    }

    /// <summary>
    /// ═══ FIX 1: OrderDetail CHỈ HIỂN THỊ — không claim đơn ═══
    /// Shipper muốn nhận đơn phải bấm nút "Nhận đơn" riêng (POST ClaimOrder)
    /// </summary>
    public async Task<ActionResult> OrderDetail(int? id)
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return RedirectToAction("Login", "Home");
        if (id == null) return RedirectToAction("Index");

        // Chỉ kiểm tra quyền xem — không claim
        var dh = db.tbDonHang
            .Include(l => l.tbQuanAn)
            .Include(l => l.tbThongTinDatHang)
            .FirstOrDefault(l => l.madh == id);

        if (dh == null)
        {
            TempData["ShipperError"] = "Đơn hàng không tồn tại";
            return RedirectToAction("Index");
        }

        var listctdh = db.tbChiTietDonHang
            .Where(ct => ct.madh == id)
            .Include(c => c.tbBienTheMonAn!).ThenInclude(b => b.tbMonAn!).ThenInclude(m => m.tbDanhMuc)
            .ToList();
        ViewBag.listctdh = listctdh;
        ViewBag.dh = dh;
        return View();
    }

    /// <summary>
    /// ═══ FIX 1b: Action POST riêng ?? shipper nh?n ?n (không claim t? OrderDetail GET) ═══
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> ClaimOrder(int id)
    {
        if (!checkShipper())
            return Json(new { success = false, message = "Vui lòng đăng nhập" });

        var sh = GetCurrentUser();
        if (sh == null)
            return Json(new { success = false, message = "Không tìm thấy thông tin shipper" });

        // ─── Atomic SQL: đảm bảo chỉ 1 shipper claim thành công ───
        var updatedRows = db.Database.ExecuteSqlRaw(
            @"UPDATE ""tbDonHang"" SET ""mashipper"" = {0}, ""trangthai"" = '" + OrderStatus.ChoShipper + @"'
              WHERE ""madh"" = {1} AND ""mashipper"" IS NULL
              AND (""trangthai"" = '" + OrderStatus.DaXacNhan + @"' OR ""trangthai"" = '" + OrderStatus.ChoShipper + @"')",
            sh.userid, id);

        if (updatedRows == 0)
        {
            var donhangCheck = db.tbDonHang.Find(id);
            if (donhangCheck == null)
                return Json(new { success = false, message = "Đơn hàng không tồn tại" });
            if (donhangCheck.mashipper != null && donhangCheck.mashipper != sh.userid)
                return Json(new { success = false, message = "Đơn hàng đã được shipper khác tiếp nhận" });
            if (donhangCheck.trangthai != OrderStatus.DaXacNhan && donhangCheck.trangthai != OrderStatus.ChoShipper)
                return Json(new { success = false, message = "Đơn hàng không còn ở trạng thái chờ nhận" });
        }

        // 🔴 FIX Bug #2: Auto-message cho customer khi shipper claim đơn
        try
        {
            var dh = db.tbDonHang.Include(d => d.tbThongTinDatHang).FirstOrDefault(d => d.madh == id);
            if (dh?.tbThongTinDatHang?.userid != null)
            {
                db.tbTinNhans.Add(new tbTinNhan
                {
                    madh = id,
                    noidung = OrderStatus.AutoMessages[OrderStatus.DaNhan],
                    makh = dh.tbThongTinDatHang.userid,
                    mashipper = null // system message
                });
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ShipperController>>();
            logger.LogWarning(ex, "ClaimOrder auto-message failed for order #{OrderId}", id);
        }

        // SignalR: broadcast cho các shipper khác
        try
        {
            await _hubContext.Clients.Group("shippers").SendAsync("orderAccepted", id, sh.userid);
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ShipperController>>();
            logger.LogWarning(ex, "SignalR orderAccepted broadcast failed for order #{OrderId}", id);
        }

        return Json(new { success = true, message = "Nhận đơn thành công!" });
    }

    /// <summary>
    /// ═══ E-DELIVERY: Danh sách đơn hàng có mã QR để quét ═══
    /// </summary>
    public ActionResult QRDelivery()
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return RedirectToAction("Login", "Home");

        var myOrders = db.tbDonHang
            .Include(d => d.tbQuanAn)
            .Include(d => d.tbThongTinDatHang)
            .Where(dh => dh.mashipper == sh.userid)
            .OrderByDescending(dh => dh.ngaydathang)
            .Take(50)
            .ToList();
        ViewBag.MyOrders = myOrders;

        return View();
    }

    public ActionResult NhanTin() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> UpdateDonHang(string status, int id)
    {
        if (!checkShipper())
            return Json(new { success = false, message = "Không có quyền thực hiện" });

        var sh = GetCurrentUser();
        if (sh == null)
            return Json(new { success = false, message = "Không tìm thấy thông tin shipper" });

        string? trangthai = null;
        if (status == "lh") trangthai = OrderStatus.DaLay;
        if (status == "ht") trangthai = OrderStatus.HoanThanh;
        if (status == "dg") trangthai = OrderStatus.DangGiao;

        if (trangthai != null)
        {
            var donhang = db.tbDonHang.Include(d => d.tbThongTinDatHang).FirstOrDefault(d => d.madh == id);

            // ═══ FIX 2: Chỉ shipper được phân công mới được update ───
            if (donhang != null && donhang.mashipper != sh.userid)
            {
                return Json(new { success = false, message = "Bạn không phải shipper phụ trách đơn hàng này" });
            }
            if (donhang != null)
            {
                // 🟡 FIX Bug #9: Dùng OrderStatus constants + transition map
                var oldStatus = donhang.trangthai;
                if (!OrderStatus.IsValidTransition(trangthai, oldStatus))
                {
                    return Json(new { success = false, message = $"Không thể chuyển từ '{oldStatus}' sang '{trangthai}'" });
                }

                donhang.trangthai = trangthai;

                // 🔴 FIX Bug #1: Dùng transaction + kiểm tra trạng thái để tránh race condition
                if (trangthai == OrderStatus.HoanThanh && oldStatus != OrderStatus.HoanThanh)
                {
                    using var transaction = db.Database.BeginTransaction();
                    try
                    {
                        donhang.ngaythanhtoan = DateTime.Now;

                        // Chỉ cộng tiền nếu chưa hoàn thành trước đó (tránh double-credit)
                        var prevCheck = db.tbDonHang.AsNoTracking().FirstOrDefault(d => d.madh == id);
                        if (prevCheck != null && prevCheck.trangthai != OrderStatus.HoanThanh)
                        {
                            if (donhang.phiship > 0)
                            {
                                var shipperUser = db.tbUser.Find(donhang.mashipper);
                                if (shipperUser != null)
                                {
                                    shipperUser.vitien += donhang.phiship;
                                }
                            }
                        }

                        db.SaveChanges();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }

                    // ─── E-Delivery: Auto-sinh E-Waybill ───
                    try { await _eDelivery.GenerateEWaybill(id); }
                    catch (Exception ex)
                    {
                        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ShipperController>>();
                        logger.LogWarning(ex, "E-Waybill generation failed for order #{OrderId}", id);
                    }
                }
                else
                {
                    db.SaveChanges();
                }

                // ═══ Auto-sinh tin nhắn khi chuyển trạng thái ═══
                try
                {
                    if (OrderStatus.AutoMessages.TryGetValue(trangthai, out var autoMsg))
                    {
                        db.tbTinNhans.Add(new tbTinNhan
                        {
                            madh = id,
                            noidung = autoMsg,
                            makh = donhang.tbThongTinDatHang?.userid,
                            mashipper = null // system message
                        });
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception msgEx)
                {
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ShipperController>>();
                    logger.LogWarning(msgEx, "Failed to create auto message for order #{OrderId}", id);
                }

                // SignalR broadcast real-time đến khách hàng
                try
                {
                    await _hubContext.Clients.Group($"order_{id}").SendAsync("orderStatusChanged", id, trangthai, DateTime.Now.ToString("HH:mm"));
                }
                catch (Exception ex)
                {
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ShipperController>>();
                    logger.LogWarning(ex, "SignalR broadcast failed for order #{OrderId}", id);
                }

                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
        }
        return Json(new { success = false, message = "Order status update failed" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult updateStatus()
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return RedirectToAction("Login", "Home");

        var shipper = db.tbShipper.Find(sh.userid);
        if (shipper != null)
        {
            shipper.trangthai = shipper.trangthai == OrderStatus.KhongHoatDong ? OrderStatus.DangHoatDong : OrderStatus.KhongHoatDong;
            db.SaveChanges();
        }
        return RedirectToAction("Index");
    }
}
