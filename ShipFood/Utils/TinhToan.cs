using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ShipFood.Models;
namespace ShipFood.Utils
{
    public class TinhToan
    {
        public static Double? TinhTienShip(System.Data.Entity.Spatial.DbGeography toaDo1, System.Data.Entity.Spatial.DbGeography toaDo2)
        {
            Double? khoangCach = toaDo1.Distance(toaDo2);
            return khoangCach * 15000;
        }
        public static Decimal? TinhTongTien(tbDonHang donHang)
        {
            Decimal? sum = 0;
            foreach (var i in donHang.tbChiTietDonHang)
            {
                sum += (i.dongia * i.soluong);
            }
            return sum;
        }
    }
}