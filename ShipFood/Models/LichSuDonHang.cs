using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShipFood.Models
{
    public class LichSuDonHang
    {
        public int madh { get; set; }
        public Nullable<System.DateTime> ngaydathang { get; set; }
        public string trangthai { get; set; }
        public string diachi { get; set; }
        public string tennguoinhan { get; set; }
        public Nullable<decimal> phiship { get; set; }
        public Nullable<int> mashipper { get; set; }
    }
}