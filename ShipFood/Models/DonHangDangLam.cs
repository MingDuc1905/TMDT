using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShipFood.Models
{
    public class DonHangDangLam
    {
        public int madh { get; set; }
        public Nullable<System.DateTime> ngaydathang { get; set; }
        public string tennguoinhan { get; set; }
        public string diachi { get; set; }
        public string tenquanan { get; set; }
        public string Diachi { get; set; }
        public Nullable<decimal> phiship { get; set; }
        public Nullable<decimal> tongtien { get; set; }
        public string trangthai { get; set; }
        public string sdt { get; set; }
        public Nullable<int> userid { get; set; }
    }
}