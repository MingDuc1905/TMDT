using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShipFood.Models;
namespace ShipFood.Controllers
{
    public class RestaurantController : Controller
    {
        dbFoodyEntities db = new dbFoodyEntities();
        // GET: Restaurant
        public ActionResult Index()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            tbQuanAn QuanAn = getQuanAn();

            int soLuongMonAn = QuanAn.tbMonAn.Count;
            Double tongDoanhThu = (double)QuanAn.tbDonHang.Sum(dh => dh.tongtien).Value;
            int soDonDatHang = QuanAn.tbDonHang.Count;
            List<int?> userids = new List<int?>();
            foreach (var i in QuanAn.tbDonHang)
            {
                if (!userids.Contains(i.tbThongTinDatHang.userid))
                    userids.Add(i.tbThongTinDatHang.userid);
            }
            int soLuongKhachHang = userids.Count;
            int dhChuanBi = QuanAn.tbDonHang.Where(dh => dh.trangthai.Equals("Đang chuẩn bị")).ToList().Count;
            int dhHoanThanh = QuanAn.tbDonHang.Where(dh => dh.trangthai.Equals("Hoàn thành")).ToList().Count;
            int dhHuy = QuanAn.tbDonHang.Where(dh => dh.trangthai.Equals("Đã huỷ")).ToList().Count;

            ViewBag.soLuongMonAn = soLuongMonAn;
            ViewBag.tongDoanhThu = tongDoanhThu;
            ViewBag.soDonDatHang = soDonDatHang;
            ViewBag.soLuongKhachHang = soLuongKhachHang;
            ViewBag.dhChuanBi = dhChuanBi;
            ViewBag.dhHoanThanh = dhHoanThanh;
            ViewBag.dhHuy = dhHuy;

            return View();
        }
        public ActionResult Wallet()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            tbQuanAn QuanAn = getQuanAn();
            List<tbDonHang> donHangs = QuanAn.tbDonHang.ToList();

            ViewBag.donHangs = donHangs;
            ViewBag.soDu = Math.Round((double)donHangs.Sum(dh => dh.tongtien), 1);
            return View();
        }
        public ActionResult Analytics()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            tbQuanAn quanAn = getQuanAn();
            List<DataAnalytic> datas = new List<DataAnalytic>();
            List<DataAnalyticDanhMuc> dataDanhMucs = new List<DataAnalyticDanhMuc>();
            List<int> idDanhMucs = new List<int>();
            foreach (var m in quanAn.tbMonAn)
            {
                int idDanhMuc = m.tbDanhMuc.madanhmuc;
                if (!idDanhMucs.Contains(idDanhMuc))
                {
                    DataAnalyticDanhMuc dataDanhMuc = new DataAnalyticDanhMuc();
                    dataDanhMuc.maDanhMuc = idDanhMuc;
                    dataDanhMuc.hinhAnh = m.tbDanhMuc.hinhanh;
                    dataDanhMuc.tenDanhMuc = m.tbDanhMuc.tendanhmuc;
                    dataDanhMuc.soLuongMonAn = (from dm in db.tbDanhMuc
                                                join ma in db.tbMonAn on dm.madanhmuc equals ma.madanhmuc
                                                where ma.maquanan == quanAn.userid && dm.madanhmuc == idDanhMuc
                                                select ma).Count();
                    dataDanhMuc.tongSoLuongBanRa = (from dm in db.tbDanhMuc
                                                    join ma in db.tbMonAn on dm.madanhmuc equals ma.madanhmuc
                                                    join ctdh in db.tbChiTietDonHang on ma.mamon equals ctdh.mamon
                                                    where ma.maquanan == quanAn.userid && dm.madanhmuc == idDanhMuc
                                                    select ctdh.soluong).Sum();
                    dataDanhMuc.doanhThu = (double?)(from dm in db.tbDanhMuc
                                                     join ma in db.tbMonAn on dm.madanhmuc equals ma.madanhmuc
                                                     join ctdh in db.tbChiTietDonHang on ma.mamon equals ctdh.mamon
                                                     where ma.maquanan == quanAn.userid && dm.madanhmuc == idDanhMuc
                                                     select ctdh.soluong * ctdh.dongia).Sum();
                    dataDanhMucs.Add(dataDanhMuc);
                    idDanhMucs.Add(idDanhMuc);
                }
            }
            foreach (var m in quanAn.tbMonAn)
            {
                DataAnalytic data = new DataAnalytic();
                data.maMonAn = m.mamon;
                data.giaTien = m.giatien;
                data.tenMonAn = m.tenmon;
                data.hinhAnh = m.hinhanh;
                data.tenDanhMuc = m.tbDanhMuc.tendanhmuc;
                data.diemDanhGia = 0;
                data.soDanhGia = 0;
                List<tbChiTietDonHang> chiTietDHs = m.tbChiTietDonHang.Where(ct => ct.mamon == data.maMonAn).ToList();
                data.soLuongBanDuoc = 0;
                foreach (var i in chiTietDHs)
                {
                    data.soLuongBanDuoc += i.soluong;
                    foreach (var tdg in i.tbDanhGia)
                    {
                        data.soDanhGia += 1;
                        data.diemDanhGia += tdg.diemdanhgia;
                    }
                    data.diemDanhGia = data.soDanhGia == 0 ? 0 : data.diemDanhGia / data.soDanhGia;
                }
                datas.Add(data);
            }
            Double tongDoanhThu = (double)quanAn.tbDonHang.Sum(dh => dh.tongtien).Value;

            datas = datas.OrderByDescending(d => d.soLuongBanDuoc).ToList();
            ViewBag.datas = datas;
            ViewBag.doanhThu = tongDoanhThu;
            ViewBag.dataDanhMucs = dataDanhMucs;
            return View();
        }
        public ActionResult Review()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }

            List<tbDanhGia> danhGias = new List<tbDanhGia>();
            tbQuanAn quanAn = getQuanAn();
            foreach (var i in quanAn.tbMonAn)
            {
                foreach (var j in i.tbChiTietDonHang)
                {
                    foreach (var o in j.tbDanhGia)
                    {
                        danhGias.Add(o);
                    }
                }
            }
            ViewBag.danhgias = danhGias;
            return View();
        }
        public ActionResult Discount()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }

            tbQuanAn quanAn = getQuanAn();
            List<tbMonAnKhuyenMai> monAnKhuyenMais = new List<tbMonAnKhuyenMai>();
            monAnKhuyenMais = (from ma in db.tbMonAn
                               join makm in db.tbMonAnKhuyenMai on ma.mamon equals makm.mamon
                               where ma.maquanan == quanAn.userid
                               select makm).ToList();

            List<tbKhuyenMai> khuyenMais = db.tbKhuyenMai.ToList();
            List<tbMonAn> monAns = quanAn.tbMonAn.ToList();

            ViewBag.monAns = monAns;
            ViewBag.maKM = khuyenMais;
            ViewBag.khuyenMais = monAnKhuyenMais;
            return View();
        }
        [HttpPost]
        public ActionResult Discount(tbMonAnKhuyenMai monAnKhuyenMai)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            db.tbMonAnKhuyenMai.Add(monAnKhuyenMai);
            db.SaveChanges();
            return RedirectToAction("Discount");
        }
        public ActionResult OrderList()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            tbQuanAn quanAn = getQuanAn();
            List<tbDonHang> donHangs = quanAn.tbDonHang.ToList();
            ViewBag.donHangs = donHangs;
            return View();
        }
        public ActionResult nhandon(int id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }

            tbDonHang dh = db.tbDonHang.Find(id);
            dh.trangthai = "Đã xác nhận";
            db.SaveChanges();
            return RedirectToAction("OrderList");
        }
        public ActionResult huydon(int id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }

            tbDonHang dh = db.tbDonHang.Find(id);
            dh.trangthai = "Đã hủy";
            db.SaveChanges();
            return RedirectToAction("OrderList");
        }
        public ActionResult Profile()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }

            tbQuanAn quanAn = getQuanAn();

            List<tbDanhGia> danhGias = new List<tbDanhGia>();
            foreach (var i in quanAn.tbMonAn)
            {
                foreach (var j in i.tbChiTietDonHang)
                {
                    foreach (var o in j.tbDanhGia)
                    {
                        danhGias.Add(o);
                    }
                }
            }
            ViewBag.danhgias = danhGias;

            ViewBag.quanAn = quanAn;
            return View();
        }
        [HttpPost]
        public ActionResult Profile(tbQuanAn quanAn, HttpPostedFileBase fileAnh, String pwd)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }

            if (fileAnh != null)
            {
                String rootFolder = Server.MapPath("/Source/Restaurant/images/avatar");
                String pathImage = rootFolder + fileAnh.FileName;
                fileAnh.SaveAs(pathImage);
                quanAn.hinhanh = fileAnh.FileName;
            }
            tbQuanAn quanAnOld = db.tbQuanAn.Find(getQuanAn().userid);
            quanAnOld.tenquanan = quanAn.tenquanan;
            if (quanAn.hinhanh == null)
                quanAnOld.hinhanh = quanAn.hinhanh;
            if (pwd == null || pwd == "")
                quanAnOld.tbUser.pwd = pwd;
            quanAnOld.diachi = quanAn.diachi;
            db.SaveChanges();
            return RedirectToAction("Profile");

        }
        public ActionResult ProductList()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }

            tbQuanAn quanAn = db.tbQuanAn.Find(getQuanAn().userid);
            List<DataAnalytic> datas = new List<DataAnalytic>();
            foreach (var m in quanAn.tbMonAn)
            {
                DataAnalytic data = new DataAnalytic();
                data.maMonAn = m.mamon;
                data.giaTien = m.giatien;
                data.tenMonAn = m.tenmon;
                data.hinhAnh = m.hinhanh;
                data.tenDanhMuc = m.tbDanhMuc.tendanhmuc;
                data.diemDanhGia = 0;
                data.soDanhGia = 0;
                List<tbChiTietDonHang> chiTietDHs = m.tbChiTietDonHang.Where(ct => ct.mamon == data.maMonAn).ToList();
                data.soLuongBanDuoc = 0;
                foreach (var i in chiTietDHs)
                {
                    data.soLuongBanDuoc += i.soluong;
                    foreach (var tdg in i.tbDanhGia)
                    {
                        data.soDanhGia += 1;
                        data.diemDanhGia += tdg.diemdanhgia;
                    }
                    data.diemDanhGia = data.soDanhGia == 0 ? 0 : data.diemDanhGia / data.soDanhGia;
                }
                datas.Add(data);
            }

            ViewBag.datas = datas;
            return View();
        }
        public ActionResult ProductDetail(int? id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }

            tbMonAn monAn = new tbMonAn();
            if (id != null)
            {
                monAn = db.tbMonAn.Find(id);
            }
            ViewBag.monAn = monAn;
            return View();
        }
        [HttpPost]
        public ActionResult PostMonAn(tbMonAn monAn, HttpPostedFileBase fileAnh)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }

            if (fileAnh != null)
            {
                String rootFolder = Server.MapPath("/Source/images/MonAn/");
                String pathImage = rootFolder + fileAnh.FileName;
                fileAnh.SaveAs(pathImage);
                monAn.hinhanh = fileAnh.FileName;
            }

            // Thêm
            if (monAn.mamon == 0)
            {
                monAn.maquanan = getQuanAn().userid;
                db.tbMonAn.Add(monAn);
            }
            // Update
            else
            {
                tbMonAn monAnOld = db.tbMonAn.Find(monAn.mamon);
                monAnOld.tenmon = monAn.tenmon;
                monAnOld.mota = monAn.mota;
                monAnOld.giatien = monAn.giatien;
                if (monAn.hinhanh != null)
                    monAnOld.hinhanh = monAn.hinhanh;
                monAnOld.madanhmuc = monAn.madanhmuc;
            }
            db.SaveChanges();
            return RedirectToAction("ProductList");
        }
        public ActionResult XoaMonAn(int? id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Home", "Login");
            }
            if (id != null)
            {
                db.tbMonAn.Remove(db.tbMonAn.Find(id));
            }
            db.SaveChanges();
            return RedirectToAction("ProductList");
        }


        public tbQuanAn getQuanAn()
        {
            if (!checkLogin())
            {
                RedirectToAction("Home", "Login");
                return null;
            }
            tbUser user = (tbUser)Session["user"];
            tbQuanAn QuanAn = db.tbQuanAn.Find(user.userid);
            return QuanAn;
        }
        public Boolean checkLogin()
        {
            if (Session["user"] == null)
                return false;
            tbUser user = (tbUser)Session["user"];
            if (!user.loaitaikhoan.Equals("Quán ăn"))
                return false;
            return true;
        }

        public ActionResult updateStatus()
        {
            tbQuanAn quanAn = db.tbQuanAn.Find(getQuanAn().userid);
            if (quanAn != null)
            {
                if (quanAn.trangthai.Equals("Đóng cửa"))
                    quanAn.trangthai = "Đang mở cửa";
                else
                    quanAn.trangthai = "Đóng cửa";
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}