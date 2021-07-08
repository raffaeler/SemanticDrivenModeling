using System;
using System.Collections.Generic;

namespace NorthwindDataLayer.Models
{
    public partial class Shipper
    {
        public Shipper()
        {
            this.Orders = new List<Order>();
        }

        public long ShipperID { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
