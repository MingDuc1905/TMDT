using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShipFood.Models
{
    public class DataAnalytic
    {
        public int maMonAn { set; get; }
        public String tenMonAn { set; get; }
        public String hinhAnh { get; set; }
        public String tenDanhMuc { get; set; }
        public Nullable<int> diemDanhGia { get; set; }
        public Nullable<int> soDanhGia { get; set; }
        public Nullable<decimal> giaTien { set; get; }
        public int? soLuongBanDuoc { set; get; }
    }
}