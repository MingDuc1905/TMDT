using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShipFood.Models;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Data.Entity;

namespace ShipFood.Controllers
{
    public class AdminController : Controller
    {
        dbFoodyEntities db = new dbFoodyEntities();
        // GET: Admin
        public ActionResult Index()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            return View();
        }
        public ActionResult Order()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            var litsdh = db.tbDonHang.ToList();
            return View(litsdh);
        }
        public ActionResult OrderDetail(int? id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            tbDonHang donhang = db.tbDonHang.Find(id);
            var chitietdh = db.tbChiTietDonHang.Where(ct => ct.madh == id).ToList();
            ViewBag.chitietdonhang = chitietdh;
            return View(donhang);
        }

        public ActionResult Category(int? id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }

            if (id.HasValue)
            {
                tbDanhMuc bd = db.tbDanhMuc.Find(id);
                db.tbDanhMuc.Remove(bd);
                db.SaveChanges();
            }
            var listdm = db.tbDanhMuc.ToList();
            return View(listdm);
        }

        public ActionResult CreateCategory()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> CreateCategory([Bind(Include = "madanhmuc,tendanhmuc,mota")] tbDanhMuc tbDanhMuc, HttpPostedFileBase hinhanh)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            if (ModelState.IsValid)
            {
                if (hinhanh != null && hinhanh.ContentLength > 0)
                {
                    // Lưu hình ảnh mới vào thư mục trên máy chủ
                    var fileName = Path.GetFileName(hinhanh.FileName);
                    var path = Path.Combine(Server.MapPath("~/Source/images/Danhmuc"), fileName);
                    hinhanh.SaveAs(path);
                    // Cập nhật tên hình ảnh vào cơ sở dữ liệu
                    tbDanhMuc.hinhanh = fileName;

                }
                else
                {
                    tbDanhMuc.hinhanh = " ";
                }
                db.tbDanhMuc.Add(tbDanhMuc);
                await db.SaveChangesAsync();
                return RedirectToAction("Category");
            }
            else
            {
                return View(tbDanhMuc);
            }

        }
        public async Task<ActionResult> EditCategory(int? id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbDanhMuc tbDanhMuc = await db.tbDanhMuc.FirstOrDefaultAsync(x => x.madanhmuc == id);
            if (tbDanhMuc == null)
            {
                return HttpNotFound();
            }
            return View(tbDanhMuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> EditCategory([Bind(Include = "madanhmuc,tendanhmuc,mota")] tbDanhMuc tbDanhMuc, HttpPostedFileBase hinhanh)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            if (ModelState.IsValid)
            {
                var existingDanhMuc = await db.tbDanhMuc.FirstOrDefaultAsync(x => x.madanhmuc == tbDanhMuc.madanhmuc);
                if (hinhanh != null && hinhanh.ContentLength > 0)
                {
                    // Lưu hình ảnh mới vào thư mục trên máy chủ
                    var fileName = Path.GetFileName(hinhanh.FileName);
                    var path = Path.Combine(Server.MapPath("~/Source/images/Danhmuc"), fileName);
                    hinhanh.SaveAs(path);

                    var oldImagePath = Server.MapPath("~/Source/images/Danhmuc/" + tbDanhMuc.hinhanh);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                    // Cập nhật tên hình ảnh vào cơ sở dữ liệu
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
            else
            {
                return View(tbDanhMuc);
            }

        }

        public ActionResult GetListCategory(string timkiem)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            IQueryable<tbDanhMuc> list = db.tbDanhMuc;
            if (!string.IsNullOrEmpty(timkiem))
            {
                String text = timkiem.ToLower();
                list = list.Where(p => p.tendanhmuc.ToLower().Contains(text) || p.mota.ToLower().Contains(text));
            }
            var listdm = list.ToList();
            return PartialView("_ListCategory", listdm);
        }
        public ActionResult QuanLyQuanAn()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            List<tbQuanAn> quanAns = db.tbQuanAn.ToList();
            ViewBag.datas = quanAns;
            return View();
        }
        public ActionResult QuanLyKhachHang()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            List<tbKhachHang> khachHangs = db.tbKhachHang.ToList();
            ViewBag.datas = khachHangs;
            return View();
        }
        public ActionResult QuanLyQuanTriVien()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            List<tbAdmin> admins = db.tbAdmin.ToList();
            ViewBag.datas = admins;
            return View();
        }
        public ActionResult QuanLyShipper()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            List<tbShipper> shippers = db.tbShipper.ToList();
            ViewBag.datas = shippers;
            return View();
        }
        public ActionResult PostTaiKhoan(int? id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            String hoten = "";
            String quan = "";
            String diachicuthe = "";
            String diachiadd = "";
            String fileAnh = null;
            tbUser user = new tbUser();
            if (id != null)
            {
                user = db.tbUser.Find(id);
                if (user.loaitaikhoan.Equals("Khách hàng"))
                {
                    hoten = user.tbKhachHang.tenkh;
                }
                else if (user.loaitaikhoan.Equals("Quán ăn"))
                {
                    hoten = user.tbQuanAn.tenquanan;
                    String[] txt = user.tbQuanAn.diachi.Split(new string[] { ", " }, StringSplitOptions.None);
                    diachiadd = txt[0];
                    diachicuthe = txt[1];
                    quan = txt[2];
                    fileAnh = user.tbQuanAn.hinhanh;
                }
                else if (user.loaitaikhoan.Equals("Shipper"))
                {
                    fileAnh = user.tbShipper.hinhanh;
                    hoten = user.tbShipper.tenshipper;
                    String[] txt = user.tbShipper.diachi.Split(new string[] { ", " }, StringSplitOptions.None);
                    diachiadd = txt[0];
                    diachicuthe = txt[1];
                    quan = txt[2];

                }
                else if (user.loaitaikhoan.Equals("Admin"))
                {
                    hoten = user.tbAdmin.tenadmin;
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
        public ActionResult PostTaiKhoan(tbUser user, String hoten, String quan, String diachicuthe, String diachiadd, HttpPostedFileBase fileAnh)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            if (fileAnh != null)
            {
                String rootFolder = Server.MapPath("/Source/images/Avatar");
                String pathImage = rootFolder + fileAnh.FileName;
                fileAnh.SaveAs(pathImage);
            }
            if (user.userid == null)
            {
                user.trangthai = 1;
                user.vitien = 0;
                db.tbUser.Add(user);
                db.SaveChanges();
                if (user.loaitaikhoan.Equals("Khách hàng"))
                {
                    tbKhachHang o = new tbKhachHang();
                    o.userid = user.userid;
                    o.tenkh = hoten;
                    db.tbKhachHang.Add(o);


                    tbThongTinDatHang ttdh = new tbThongTinDatHang();
                    ttdh.diachi = diachiadd + ", " + diachicuthe + ", " + quan + ", " + " TP Đà Nẵng";
                    ttdh.sdt = user.sdt;
                    ttdh.tennguoinhan = hoten;
                    ttdh.userid = user.userid;
                    db.tbThongTinDatHang.Add(ttdh);

                    db.SaveChanges();
                }
                else if (user.loaitaikhoan.Equals("Quán ăn"))
                {
                    tbQuanAn quanAn = new tbQuanAn();
                    quanAn.userid = user.userid;
                    quanAn.tenquanan = hoten;
                    quanAn.diachi = diachiadd + ", " + diachicuthe + ", " + quan + ", " + " TP Đà Nẵng";
                    quanAn.toado = null;
                    quanAn.soluotdanhgia = 0;
                    quanAn.diemdanhgia = 0;
                    quanAn.trangthai = "Đóng cửa";
                    quanAn.hinhanh = fileAnh.FileName;
                    db.tbQuanAn.Add(quanAn);
                    db.SaveChanges();
                }
                else if (user.loaitaikhoan.Equals("Shipper"))
                {
                    tbShipper o = new tbShipper();
                    o.userid = user.userid;
                    o.tenshipper = hoten;
                    o.diachi = diachiadd + ", " + diachicuthe + ", " + quan + ", " + " TP Đà Nẵng";
                    o.toado = null;
                    o.soluotdanhgia = 0;
                    o.diemdanhgia = 0;
                    o.trangthai = "Không hoạt động";
                    o.hinhanh = fileAnh.FileName;
                    db.tbShipper.Add(o);
                    db.SaveChanges();
                }
                else if (user.loaitaikhoan.Equals("Admin"))
                {
                    tbAdmin o = new tbAdmin();
                    o.userid = user.userid;
                    o.tenadmin = hoten;
                    db.tbAdmin.Add(o);
                    db.SaveChanges();
                }
            }
            else
            {
                tbUser userOld = db.tbUser.Find(user.userid);
                userOld.pwd = user.pwd;
                userOld.sdt = user.sdt;
                userOld.email = user.email;
                if (user.loaitaikhoan.Equals("Khách hàng"))
                {
                    tbKhachHang o = db.tbKhachHang.Find(user.userid);
                    o.tenkh = hoten;

                    db.SaveChanges();
                }
                else if (user.loaitaikhoan.Equals("Quán ăn"))
                {
                    tbQuanAn quanAn = db.tbQuanAn.Find(user.userid);
                    quanAn.tenquanan = hoten;
                    quanAn.diachi = diachiadd + ", " + diachicuthe + ", " + quan + ", " + " TP Đà Nẵng";
                    if (fileAnh != null)
                        quanAn.hinhanh = fileAnh.FileName;
                    db.SaveChanges();
                }
                else if (user.loaitaikhoan.Equals("Shipper"))
                {
                    tbShipper o = db.tbShipper.Find(user.userid);
                    o.tenshipper = hoten;
                    o.diachi = diachiadd + ", " + diachicuthe + ", " + quan + ", " + " TP Đà Nẵng";
                    if (fileAnh != null)
                    {
                        o.hinhanh = fileAnh.FileName;
                    }
                    db.SaveChanges();
                }
                else if (user.loaitaikhoan.Equals("Admin"))
                {
                    tbAdmin o = db.tbAdmin.Find(user.userid);
                    o.tenadmin = hoten;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("PostTaiKhoan");
        }
        public ActionResult Duyet(int? id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            tbUser user = db.tbUser.Find(id);
            user.trangthai = 1;
            db.SaveChanges();
            if (user.loaitaikhoan.Equals("Shipper"))
            {
                return RedirectToAction("QuanLyShipper");
            }
            else if (user.loaitaikhoan.Equals("Quán ăn"))
            {
                return RedirectToAction("QuanLyQuanAn");
            }
            return RedirectToAction("Index");
        }
        public ActionResult Huy(int? id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            tbUser user = db.tbUser.Find(id);
            user.trangthai = 3;
            db.SaveChanges();
            if (user.loaitaikhoan.Equals("Shipper"))
            {
                return RedirectToAction("QuanLyShipper");
            }
            else if (user.loaitaikhoan.Equals("Quán ăn"))
            {
                return RedirectToAction("QuanLyQuanAn");
            }
            return RedirectToAction("Index");
        }
        public ActionResult LockOrUnLock(int? id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            tbUser user = db.tbUser.Find(id);
            if (user.trangthai == 1)
            {
                user.trangthai = 2;
            }
            else if (user.trangthai == 2)
            {
                user.trangthai = 1;
            }
            db.SaveChanges();
            if (user.loaitaikhoan.Equals("Shipper"))
            {
                return RedirectToAction("QuanLyShipper");
            }
            else if (user.loaitaikhoan.Equals("Quán ăn"))
            {
                return RedirectToAction("QuanLyQuanAn");
            }
            else if (user.loaitaikhoan.Equals("Admin"))
            {
                return RedirectToAction("QuanLyQuanTriVien");
            }
            else if (user.loaitaikhoan.Equals("Khách hàng"))
            {
                return RedirectToAction("QuanLyKhachHang");
            }
            return RedirectToAction("Index");
        }
        public Boolean checkLogin()
        {
            if (Session["user"] == null)
                return false;
            tbUser user = (tbUser)Session["user"];
            if (!user.loaitaikhoan.Equals("Admin"))
                return false;
            return true;
        }
    }
}