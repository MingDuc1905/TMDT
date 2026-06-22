namespace ShipFood.Models;

public class Cart
{
    public int? userid { get; set; }
    public decimal? tongTien { get; set; }
    public int? maquanan { get; set; }
    public int? maKM { get; set; }
    public List<tbMonAn> monAns { get; set; } = new();
    public tbThongTinDatHang? thongTinDatHang { get; set; }

    public Cart()
    {
        monAns = new List<tbMonAn>();
        tongTien = 0;
    }

    public void themMon(tbMonAn monAn, int soLuong)
    {
        foreach (var i in monAns)
        {
            if (i.mamon == monAn.mamon)
            {
                i.soLuong += soLuong;
                tongTien += i.giatien * soLuong;
                return;
            }
        }
        monAn.soLuong = soLuong;
        monAns.Add(monAn);
        tongTien += monAn.giatien * soLuong;
    }

    public void xoaMon(int maMon)
    {
        foreach (var i in monAns)
        {
            if (i.mamon == maMon)
            {
                monAns.Remove(i);
                tongTien -= i.giatien * i.soLuong;
                return;
            }
        }
    }

    public void giamMon(int? maMonAn)
    {
        foreach (var i in monAns)
        {
            if (i.mamon == maMonAn)
            {
                if (i.soLuong <= 1)
                {
                    monAns.Remove(i);
                    tongTien -= i.giatien;
                    break;
                }
                else
                {
                    i.soLuong -= 1;
                    tongTien -= i.giatien;
                    return;
                }
            }
        }
    }
}
