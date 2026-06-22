using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShipFood.Models;
namespace ShipFood.Controllers
{
    public class ShipperController : Controller
    {
        dbFoodyEntities db = new dbFoodyEntities();
        // GET: Shipper
        public ActionResult Index()
        {
            tbUser sh = (tbUser)Session["user"];
            var listdh = db.Database.SqlQuery<DonHangDangLam>(
               "select dh.madh, dh.ngaydathang, tt.diachi, tt.tennguoinhan, dh.trangthai, dh.phiship, dh.tongtien,tt.userid, tt.sdt, " +
               "qa.tenquanan as tenquanan, qa.diachi as Diachi " +
               "from tbDonHang dh " +
               "Join tbThongTinDatHang tt On dh.mattdh = tt.mattdh " +
               "Join tbQuanAn qa On dh.maquan = qa.userid " +
               "Where dh.trangthai = N'Đã xác nhận' and dh.mashipper is NULL " +
               "Order by dh.madh DESC"
                ).ToList();
            return View(listdh);
        }

        public ActionResult ThuNhap()
        {
            DateTime currentDate = DateTime.Now;
            DateTime thirtyDaysAgo = currentDate.AddDays(-30);
            tbUser sh = (tbUser)Session["user"];
            var shipper = db.tbUser.Find(sh.userid);
            var listdh30 = db.tbDonHang.Where(dh => dh.mashipper == sh.userid && dh.ngaythanhtoan >= thirtyDaysAgo && dh.ngaythanhtoan <= currentDate).ToList();
            var listdhhoanthanh30 = listdh30.Where(l => l.trangthai.Equals("Hoàn thành")).ToList();
            var thunhap30 = listdhhoanthanh30.Sum(list => list.phiship);
            int dh30 = listdh30.Count();

            var listdhhn = db.tbDonHang.Where(dh => dh.mashipper == sh.userid && dh.ngaythanhtoan == currentDate).ToList();
            var listdhhthn = listdhhn.Where(l => l.trangthai.Equals("Hoàn thành")).ToList();
            var listdhdhhn = listdhhn.Where(l => l.trangthai.Equals("Đã hủy")).ToList();
            var thunhaphn = listdhhthn.Sum(list => list.phiship);
            var dhhthn = listdhhthn.Count();
            var dhdhhn = listdhdhhn.Count();
            ViewBag.thunhap30 = thunhap30;
            ViewBag.dh30 = dh30;

            ViewBag.thunhaphn = thunhaphn;
            ViewBag.dhhthn = dhhthn;
            ViewBag.dhdhhn = dhdhhn;
            return View(shipper);
        }
        public ActionResult ThongBao()
        {
            return View();
        }

        public ActionResult LichSu()
        {
            tbUser sh = (tbUser)Session["user"];
            var listdh = db.tbDonHang.Where(dh => dh.mashipper == sh.userid).ToList();

            return View(listdh);
        }
        public ActionResult CaiDat()
        {
            tbUser user = (tbUser)Session["user"];
            var shipper = db.tbUser.Find(user.userid);
            return View(shipper);
        }
        [HttpPost]
        public ActionResult CaiDat([Bind(Include = "userid,sdt,pwd")] tbUser user, string diachi)
        {
            tbUser shipper = (tbUser)Session["user"];
            if (ModelState.IsValid)
            {
                var existingUser = db.tbUser.Find(user.userid);

                if (existingUser != null)
                {
                    existingUser.sdt = user.sdt;
                    existingUser.pwd = user.pwd;
                    db.Entry(existingUser).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                return RedirectToAction("CaiDat");
            }
            return View(shipper);
        }
        public ActionResult ViTien()
        {
            tbUser sh = (tbUser)Session["user"];
            var shipper = db.tbUser.Find(sh.userid);

            var listdonhang = db.tbDonHang.Where(dh => dh.mashipper == sh.userid).ToList();
            ViewBag.listdh = listdonhang;
            return View(shipper);
        }

        public ActionResult OrderDetail(int? id)
        {
            tbUser sh = (tbUser)Session["user"];
            tbDonHang donhang = db.tbDonHang.Find(id);
            if(donhang != null)
            {
                donhang.mashipper = sh.userid;
                db.SaveChanges();             
            }         
            var listctdh = db.tbChiTietDonHang.Where(ct => ct.madh == id).ToList();
            var dh = db.tbDonHang.Where(l => l.madh == id).SingleOrDefault();
            ViewBag.listctdh = listctdh;
            ViewBag.dh = dh;

            return View();
        }

        public ActionResult NhanTin()
        {
            return View();
        }
        [HttpPost]
        public JsonResult UpdateDonHang(string status, int id)
        {
            string trangthai = null;
            if (status != null)
            {
                if (status == "lh")
                    trangthai = "Đã lấy";
                if (status == "ht")
                    trangthai = "Hoàn thành";
                var donhang = db.tbDonHang.Where(d => d.madh == id).SingleOrDefault();
                donhang.trangthai = trangthai;
                db.Entry(donhang).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, message = "Order status updated successfully" });
            }
            else
                return Json(new { success = false, message = "Order status updated fail" });
        }
        public ActionResult updateStatus()
        {
            tbUser sh = (tbUser)Session["user"];
            tbShipper shipper = db.tbShipper.Find(sh.userid);
            if (shipper != null)
            {
                if (shipper.trangthai.Equals("Không hoạt động"))
                    shipper.trangthai = "Đang hoạt động";
                else
                    shipper.trangthai = "Không hoạt động";
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}