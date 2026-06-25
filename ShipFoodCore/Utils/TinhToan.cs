using ShipFood.Models;

namespace ShipFood.Utils;

public class TinhToan
{
    public static decimal? TinhTienShip(decimal? khoangCach)
    {
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
