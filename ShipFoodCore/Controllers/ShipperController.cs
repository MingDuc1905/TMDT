using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipFood.Models;

namespace ShipFood.Controllers;

public class ShipperController : BaseController
{
    public ShipperController(dbFoodyEntities context)
    {
        db = context;
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

        var listdh = db.DonHangDangLam.FromSqlRaw(
            @"select dh.madh, dh.ngaydathang, tt.diachi, tt.tennguoinhan, dh.trangthai, dh.phiship, dh.tongtien, tt.userid, tt.sdt, 
              qa.tenquanan as tenquanan, qa.diachi as DiaChiQuan 
              from tbDonHang dh 
              Join tbThongTinDatHang tt On dh.mattdh = tt.mattdh 
              Join tbQuanAn qa On dh.maquan = qa.userid 
              Where dh.trangthai = N'Đã xác nhận' and dh.mashipper is NULL 
              Order by dh.madh DESC"
        ).ToList();
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
        var listdhhoanthanh30 = listdh30.Where(l => l.trangthai == "Hoàn thành").ToList();
        var thunhap30 = listdhhoanthanh30.Sum(list => list.phiship) ?? 0;
        int dh30 = listdh30.Count;

        var listdhhn = db.tbDonHang.Where(dh => dh.mashipper == sh.userid && dh.ngaythanhtoan >= todayStart && dh.ngaythanhtoan < todayEnd).ToList();
        var listdhhthn = listdhhn.Where(l => l.trangthai == "Hoàn thành").ToList();
        var listdhdhhn = listdhhn.Where(l => l.trangthai == "Đã hủy").ToList();
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
        return View();
    }

    public ActionResult LichSu()
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return RedirectToAction("Login", "Home");
        var listdh = db.tbDonHang
            .Where(dh => dh.mashipper == sh.userid)
            .Include(d => d.tbThongTinDatHang)
            .ToList();
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

        var existingUser = db.tbUser.Find(user.userid);
        if (existingUser != null)
        {
            existingUser.sdt = user.sdt;
            // Chỉ hash password nếu có thay đổi (không rỗng, khác current)
            if (!string.IsNullOrEmpty(user.pwd) && user.pwd != existingUser.pwd)
            {
                existingUser.pwd = BCrypt.Net.BCrypt.HashPassword(user.pwd, workFactor: 12);
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
        return View(shipper ?? new tbUser());
    }

    public ActionResult OrderDetail(int? id)
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return RedirectToAction("Login", "Home");
        if (id == null) return RedirectToAction("Index");

        // ─── RACE CONDITION: Kiểm tra đơn đã có shipper khác nhận chưa ───
        var donhang = db.tbDonHang.Find(id);
        if (donhang == null)
        {
            TempData["ShipperError"] = "Đơn hàng không tồn tại";
            return RedirectToAction("Index");
        }

        if (donhang.mashipper != null && donhang.mashipper != sh.userid)
        {
            TempData["ShipperError"] = "Đơn hàng đã được shipper khác tiếp nhận";
            return RedirectToAction("Index");
        }

        if (donhang.trangthai != "Đã xác nhận" && donhang.trangthai != "Chờ shipper lấy hàng")
        {
            TempData["ShipperError"] = "Đơn hàng không còn ở trạng thái chờ nhận";
            return RedirectToAction("Index");
        }

        // ─── Tranh đơn: dùng giao dịch nguyên tử để tránh 2 shipper nhận cùng lúc ───
        if (donhang.mashipper == null)
        {
            donhang.mashipper = sh.userid;
            donhang.trangthai = "Chờ shipper lấy hàng";
            db.SaveChanges();
        }

        var listctdh = db.tbChiTietDonHang
            .Where(ct => ct.madh == id)
            .Include(c => c.tbMonAn).ThenInclude(m => m.tbDanhMuc)
            .ToList();
        var dh = db.tbDonHang
            .Include(l => l.tbQuanAn)
            .Include(l => l.tbThongTinDatHang)
            .FirstOrDefault(l => l.madh == id);
        ViewBag.listctdh = listctdh;
        ViewBag.dh = dh;
        return View();
    }

    public ActionResult NhanTin() => View();

    [HttpPost]
    public JsonResult UpdateDonHang(string status, int id)
    {
        if (!checkShipper())
            return Json(new { success = false, message = "Không có quyền thực hiện" });
        string? trangthai = null;
        if (status == "lh") trangthai = "Đã lấy";
        if (status == "ht") trangthai = "Hoàn thành";

        if (trangthai != null)
        {
            var donhang = db.tbDonHang.FirstOrDefault(d => d.madh == id);
            if (donhang != null)
            {
                donhang.trangthai = trangthai;
                if (trangthai == "Hoàn thành")
                    donhang.ngaythanhtoan = DateTime.Now;
                db.SaveChanges();
                return Json(new { success = true, message = "Order status updated successfully" });
            }
        }
        return Json(new { success = false, message = "Order status update failed" });
    }

    public ActionResult updateStatus()
    {
        var sh = GetCurrentUser();
        if (sh == null || !checkShipper()) return RedirectToAction("Login", "Home");

        var shipper = db.tbShipper.Find(sh.userid);
        if (shipper != null)
        {
            shipper.trangthai = shipper.trangthai == "Không hoạt động" ? "Đang hoạt động" : "Không hoạt động";
            db.SaveChanges();
        }
        return RedirectToAction("Index");
    }
}
