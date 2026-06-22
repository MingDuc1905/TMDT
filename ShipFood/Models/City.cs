using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShipFood.Models
{
    public class City
    {
        public City()
        {
            nameCity = "Đà Nẵng";
            districts = new List<District>();
            String[] names = { "Hải Châu" ,"Thanh Khê" , "Sơn Trà", "Ngũ Hành Sơn", "Liên Chiểu", "Cẩm Lệ" , "Hòa Vang" };
            foreach(String n in names)
            {
                District district = new District(n);
                districts.Add(district);
            }

        }
        public String nameCity { get; set; }
        public List<District> districts { get; set; }
    }
}