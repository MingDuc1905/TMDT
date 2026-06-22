using NetTopologySuite.Geometries;
using ShipFood.Models;

namespace ShipFood.Utils;

public class TinhToan
{
    public static double? TinhTienShip(Point toaDo1, Point toaDo2)
    {
        double? khoangCach = toaDo1.Distance(toaDo2);
        return khoangCach * 15000;
    }

    public static decimal? TinhTongTien(tbDonHang donHang)
    {
        decimal? sum = 0;
        foreach (var i in donHang.tbChiTietDonHang)
        {
            sum += i.dongia * i.soluong;
        }
        return sum;
    }
}
