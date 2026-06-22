using ShipFood.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShipFood.Controllers
{
    public class HomeController : Controller
    {
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
        // GET: Home
        dbFoodyEntities db = new dbFoodyEntities();
        public ActionResult Index(String txtSearch, int? idDM)
        {
            List<tbQuanAn> quanAns = db.tbQuanAn.ToList();
            if(txtSearch != null)
            {
                string searchKeyNormalized = RemoveDiacritics(txtSearch.ToLower());

                quanAns = quanAns.Where(qa =>
                    RemoveDiacritics(qa.tenquanan.ToLower()).Contains(searchKeyNormalized)
                    || RemoveDiacritics(qa.tbUser.username.ToLower()).Contains(searchKeyNormalized)
                    || qa.tbMonAn.Any(ma => RemoveDiacritics(ma.tenmon.ToLower()).Contains(searchKeyNormalized))
                ).ToList();
                ViewBag.txtSearch = txtSearch;    
            }
            if(idDM != null)
            {
                quanAns = quanAns.Where(qa => qa.tbMonAn.Where(ma => ma.madanhmuc == idDM).ToList().Count != 0).ToList();
                ViewBag.idDM = idDM;
            }
            ViewBag.quanAns = quanAns;
            return View();
        }
        public ActionResult DetailRestaurant(int id,int? idDM,String searchKey)
        {
            tbQuanAn quanAn = db.tbQuanAn.Where(t => t.userid == id).FirstOrDefault();
            if (quanAn == null)
                return HttpNotFound();
            var danhSachMonAn = db.tbMonAn.Where(m => m.maquanan == id).ToList();
            if (idDM != null)
                danhSachMonAn = danhSachMonAn.Where(ma => ma.madanhmuc == idDM).ToList();
            if(searchKey!= null)
            {
                string searchKeyNormalized = RemoveDiacritics(searchKey.ToLower());
                danhSachMonAn = danhSachMonAn.Where(ma => RemoveDiacritics(ma.tenmon.ToLower()).Contains(searchKeyNormalized)).ToList();

            }
            //  var thucDon = db.tbDanhMucs.Where(t => t.madanhmuc == id).ToList();
            var thucDon = db.tbDanhMuc.Where(d =>
                db.tbMonAn.Any(m => m.maquanan == id && m.madanhmuc == d.madanhmuc)).ToList(); 
            // Chỉ định kiểu dữ liệu của danh sách là tbDanhMuc

            ViewBag.ThucDon = thucDon;
            ViewBag.DanhSachMonAn = danhSachMonAn;
            ViewBag.maquan = id;
            ViewBag.searchKey = searchKey;
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
            List<tbUser> users = db.tbUser.Where(u => u.username.Equals(user.username) && u.pwd.Equals(user.pwd)).ToList();
            if (users.Count != 0)
            {
                tbUser userFind = users[0];
                if(userFind.trangthai == 0)
                {
                    ViewBag.LoginFail = "Tài khoản chưa được duyệt";
                    return  View();
                }else if(userFind.trangthai == 2)
                {
                    ViewBag.LoginFail = "Tài khoản đã bị khóa";
                    return View();
                }
                Cart cart = new Cart();
                cart.userid = userFind.userid;
                Session["cart"] = cart;
                Session["user"] = userFind;

                if (userFind.loaitaikhoan.Equals("Khách hàng"))
                {
                    return RedirectToAction("Index", "Home");
                }
                else if (userFind.loaitaikhoan.Equals("Shipper"))
                {
                    return RedirectToAction("Index", "Shipper");
                }
                else if (userFind.loaitaikhoan.Equals("Quán ăn"))
                {
                    return RedirectToAction("Index", "Restaurant");
                }
                if (userFind.loaitaikhoan.Equals("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            else
            {
                ViewBag.LoginFail = "Đăng nhập thất bại";
                return View();
            }
            return RedirectToAction("Index");
        }
        public ActionResult Signup()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Signup(tbUser user,String repeatpw,String diachi,String hoten)
        {
            /*try
            {*/
                if (user.pwd != repeatpw)
                {
                    ViewBag.err = "Xác nhận mật khẩu sai";
                    return View();
                }
                List<tbUser> users = db.tbUser.Where(
                    u => u.username.Equals(user.username)
                    ).ToList();
                if (users.Count != 0)
                {
                    ViewBag.err = "Tên tài khoản đã tồn tại";
                    return View();
                }
                if(user.loaitaikhoan.Equals("Khách hàng"))
                {
                    user.vitien = 0;
                    user.trangthai = 1;
                    db.tbUser.Add(user);
                    db.SaveChanges();

                    tbKhachHang khachHang = new tbKhachHang();
                    khachHang.userid = user.userid;
                    khachHang.tenkh = hoten;
                    db.tbKhachHang.Add(khachHang);
                    db.SaveChanges();
                }
                else if(user.loaitaikhoan.Equals("Quán ăn"))
                {
                    user.vitien = 0;
                    user.trangthai = 0;
                    db.tbUser.Add(user);
                    db.SaveChanges();

                    tbQuanAn o = new tbQuanAn();
                    o.userid = user.userid;
                    o.tenquanan = hoten;
                    o.diachi = diachi;
                    o.soluotdanhgia = 0;
                    o.diemdanhgia = 0;
                    o.trangthai = "Đóng cửa";
                    db.tbQuanAn.Add(o);
                    db.SaveChanges();
                }
                else if(user.loaitaikhoan.Equals("Shipper"))
                {
                    user.vitien = 0;
                    user.trangthai = 0;
                    db.tbUser.Add(user);
                    db.SaveChanges();

                    tbShipper o = new tbShipper();
                    o.userid = user.userid;
                    o.tenshipper = hoten;
                    o.diachi = diachi;
                    o.soluotdanhgia = 0;
                    o.diemdanhgia = 0;
                    o.trangthai = "Không hoạt động";
                    db.tbShipper.Add(o);
                    db.SaveChanges();
                }
                return RedirectToAction("Login");
            /*}
            catch
            {
                ViewBag.err = "Lỗi";
                return View();
            }*/
        }
        public ActionResult Forgot()
        {

            return View();
        }
        public ActionResult Logout()
        {
            Session.Remove("user");
            return RedirectToAction("Index");
        }

        public ActionResult DanhMuc()
        {
            var danhmuc = db.tbDanhMuc.ToList();
            return View(danhmuc);
        }

        public ActionResult SanPham(int id)
        {
            var dsmonan = db.tbMonAn.Where(n => n.tbDanhMuc.madanhmuc == id).ToList();
            tbDanhMuc tendanhmuc = db.tbDanhMuc.Find(id);
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
}