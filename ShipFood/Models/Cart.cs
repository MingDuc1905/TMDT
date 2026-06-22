using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShipFood.Models
{
    public class Cart
    {
        public int? userid;
        public Decimal? tongTien;
        public int? maquanan;
        public int? maKM;
        public List<tbMonAn> monAns = new List<tbMonAn>();
        public tbThongTinDatHang thongTinDatHang;
        public Cart()
        {
            monAns = new List<tbMonAn>();
            tongTien = 0;
        }
        
        public void themMon(tbMonAn monAn,int soLuong) {
            foreach(var i in monAns)
            {
                // Nếu món ăn đã tồn tại trong cart
                if(i.mamon == monAn.mamon)
                {
                    i.soLuong += soLuong;
                    tongTien = tongTien + (i.giatien * soLuong);
                    return;
                }
            }
            // Nếu món ăn chưa có trong cart
            monAn.soLuong = soLuong;
            monAns.Add(monAn);
            tongTien = tongTien + (monAn.giatien * soLuong);
            return;
        }
        public void xoaMon(int maMon)
        {
            foreach(var i in monAns)
            {
                if(i.mamon == maMon)
                {
                    monAns.Remove(i);
                    tongTien = tongTien - (i.giatien * i.soLuong);
                    return;
                }
            }
        }
        public void giamMon(int? maMonAn)
        {
            foreach(var i in monAns)
            {
                if(i.mamon == maMonAn)
                {
                    // Nếu món ăn bằng 1 thì xoá nó
                    if(i.soLuong <= 1)
                    {
                        this.monAns.Remove(i);
                        tongTien = tongTien - i.giatien;
                        break ;
                    }
                    // Ngược lại nếu số lượng > 1 thì giảm số lượng đi 1
                    else
                    {
                        i.soLuong -= 1;
                        tongTien = tongTien - i.giatien;
                        return;
                    }
                }
            }
        }
       
    }
}