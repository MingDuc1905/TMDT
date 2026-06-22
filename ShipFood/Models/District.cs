using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShipFood.Models
{
    public class District
    {
        public String nameDistrict { get; set; }
        public District(String name)
        {
            this.nameDistrict = name;
        }
    }
}