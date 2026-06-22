using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayPal.Api;
using ShipFood.Models;
using ShipFood.Utils;

namespace ShipFood.Controllers
{
    public class CartController : Controller
    {
        static String hoten = null;
        static String quan = null;
        static String diachicuthe = null;
        static String diachiadd = null;
        static String SDT = null;
        static String note = null;
        static int pttt = 1;
        static int? mattdh = null;

        dbFoodyEntities db = new dbFoodyEntities();

        // GET: Cart
        [HttpGet]
        public ActionResult Index()
        {
            if (!checkLogin())
                return RedirectToAction("Login", "Home");
            List<tbKhuyenMai> maKMs = db.tbKhuyenMai.ToList();
            ViewBag.maKMs = maKMs;
            return View();
        }
        [HttpGet]
        public ActionResult Checkout()
        {
            if (!checkLogin())
                return RedirectToAction("Login", "Home");
            Cart cart = (Cart)Session["cart"];
            tbUser user = (tbUser)Session["user"];

            ViewBag.phuongthuctt = db.tbLoaiHinhThanhToan.ToList();
            ViewBag.diachicosan = db.tbThongTinDatHang.Where(tt => tt.userid == user.userid).ToList();
            ViewBag.cart = cart;
            return View();
        }
        [HttpPost]
        public ActionResult Checkout(String hoten, String quan, String diachicuthe, String diachiadd, String SDT, String note, int pttt, int? mattdh)
        {
            CartController.hoten = hoten;
            CartController.quan = quan;
            CartController.diachicuthe = diachicuthe;
            CartController.diachiadd = diachiadd;
            CartController.SDT = SDT;
            CartController.pttt = pttt;
            CartController.mattdh = mattdh;


            if (!checkLogin())
                return RedirectToAction("Login", "Home");
            tbLoaiHinhThanhToan tt = db.tbLoaiHinhThanhToan.Find(pttt);
            // Nếu thanh toán online
            if (tt.tenhinhthuc.Equals("Paypal"))
            {
                return RedirectToAction("PaymentWithPayPal");
                
            }
            // Nếu thanh toán tiền mặt
            else
            {
                return RedirectToAction("SuccessView");
            }

            
        }


        [HttpGet]
        public ActionResult ThemMonAn(int maMonAn, int soLuong)
        {
            if (!checkLogin())
                return RedirectToAction("Login", "Home");
            Cart cart = (Cart)Session["cart"];
            tbMonAn monAn = db.tbMonAn.Find(maMonAn);
            // Nếu món ăn thêm vào thuộc quán khác với các món của quán tồn tại trong giỏ hàng thì xoá cái cũ
            // Thì khởi tạo lại giỏ hàng mới
            if (cart.maquanan == null)
            {
                cart.maquanan = monAn.maquanan;
            }
            else if (cart.maquanan != monAn.maquanan)
            {
                cart = new Cart();
                cart.maquanan = monAn.maquanan;
            }
            cart.themMon(monAn, soLuong);
            Session["cart"] = cart;
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult TangSoLuong(int maMonAn, int soLuong)
        {
            if (!checkLogin())
                return RedirectToAction("Login", "Home");
            Cart cart = (Cart)Session["cart"];
            tbMonAn monAn = db.tbMonAn.Find(maMonAn);
            if (cart.maquanan == null)
            {
                cart.maquanan = monAn.maquanan;
            }
            else if (cart.maquanan != monAn.maquanan)
            {
                cart = new Cart();
            }
            cart.themMon(monAn, soLuong);
            Session["cart"] = cart;
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult GiamSoLuong(int maMonAn)
        {
            if (!checkLogin())
                return RedirectToAction("Login", "Home");
            Cart cart = (Cart)Session["cart"];
            cart.giamMon(maMonAn);
            Session["cart"] = cart;
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult XoaMon(int maMonAn)
        {
            if (!checkLogin())
                return RedirectToAction("Login", "Home");
            Cart cart = (Cart)Session["cart"];
            cart.xoaMon(maMonAn);
            return RedirectToAction("Index");
        }

        public Boolean checkLogin()
        {
            if (Session["user"] == null)
            {
                return false;
            }
            return true;
        }
        public ActionResult LichSuDatHang()
        {
            if (!checkLogin())
                return RedirectToAction("Login", "Home");
            tbUser user = (tbUser)Session["user"];
            List<tbDonHang> donHangs = db.tbDonHang.Where(dh => dh.tbThongTinDatHang.userid == user.userid).OrderBy(dh => dh.ngaydathang).ToList();
            donHangs.Reverse();
            ViewBag.donHangs = donHangs;
            return View();
        }
        public ActionResult ChiTietDonHang(int? id)
        {
            if (!checkLogin())
                return RedirectToAction("Login", "Home");
            if (id == null)
                return RedirectToAction("LichSuDatHang");
            ViewBag.donHang = db.tbDonHang.Find(id);
            return View();
        }
        decimal? tongtien()
        {
            Cart cart = (Cart)Session["cart"];
            decimal? rs = 0;
            foreach (var sp in cart.monAns)
            {
                rs += Math.Round((decimal)sp.giatien / 24000) * sp.soLuong;
            }
            return rs;
        }
        public List<tbMonAn> getListCart()
        {
            Cart gh = (Cart)Session["cart"];
            return gh.monAns;
        }
        public String convertPrice(decimal? price)
        {
            return Math.Round((decimal)price / 24000).ToString();
        }


        public ActionResult FailureView()
        {
            return View();
        }
        public ActionResult SuccessView()
        {
            String hoten = CartController.hoten;
            String quan = CartController.quan; 
            String diachicuthe = CartController.diachicuthe;
            String diachiadd = CartController.diachiadd;
            String SDT = CartController.SDT; 
            String note = CartController.note;
            int pttt = CartController.pttt;
            int? mattdh = CartController.mattdh;


            tbUser user = (tbUser)Session["user"];
            Cart cart = (Cart)Session["cart"];
            // Khởi tạo thông tin đơn hàng
            tbThongTinDatHang ttdh = new tbThongTinDatHang();
            if (mattdh != null)
            {
                ttdh = db.tbThongTinDatHang.Find(mattdh);
            }
            else
            {
                ttdh.userid = user.userid;
                ttdh.sdt = SDT;
                ttdh.diachi = diachiadd + ", " + diachicuthe + ", " + quan + ", " + " TP Đà Nẵng";
                ttdh.toado = DbGeography.FromText("POINT(90 90)");
                ttdh.tennguoinhan = hoten;
                db.tbThongTinDatHang.Add(ttdh);
                db.SaveChanges();
            }
            //
            tbDonHang dh = new tbDonHang();
            dh.maquan = cart.maquanan;
            dh.mattdh = ttdh.mattdh;
            dh.ngaydathang = DateTime.Now;
            dh.trangthai = "Đã đặt";
            dh.tongtien = (Decimal?)cart.tongTien + 15000;
            dh.hinhthucthanhtoan = pttt;
            dh.ghichu = note;
            dh.phiship = 15000;

            db.tbDonHang.Add(dh);
            db.SaveChanges();

            foreach (var i in cart.monAns)
            {
                tbChiTietDonHang ctdh = new tbChiTietDonHang();
                ctdh.madh = dh.madh;
                ctdh.mamon = i.mamon;
                ctdh.soluong = i.soLuong;
                ctdh.dongia = i.giatien;
                db.tbChiTietDonHang.Add(ctdh);
                db.SaveChanges();
            }

            Session["cart"] = new Cart();

            return View();

        }
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            //getting the apiContext  
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Cart/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                return View("FailureView");
            }
            //on successful payment, show success page to user.  
            return RedirectToAction("SuccessView");
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  )
            foreach (var i in getListCart())
            {
                itemList.items.Add(new Item()
                {
                    name = i.tenmon,
                    currency = "USD",
                    price = convertPrice(i.giatien),
                    quantity = i.soLuong.ToString(),
                    sku = i.mamon.ToString()
                });
            }

            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "1",
                subtotal = tongtien().ToString()
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = (tongtien() + 1).ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            var paypalOrderId = DateTime.Now.Ticks;
            transactionList.Add(new Transaction()
            {
                description = $"Invoice #{paypalOrderId}",
                invoice_number = paypalOrderId.ToString(), //Generate an Invoice No    
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }
    }
}