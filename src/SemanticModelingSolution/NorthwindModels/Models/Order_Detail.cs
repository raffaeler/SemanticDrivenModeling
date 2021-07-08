using System;
using System.Collections.Generic;

namespace NorthwindDataLayer.Models
{
    public partial class Order_Detail
    {
        public long OrderID { get; set; }
        public long ProductID { get; set; }
        public decimal UnitPrice { get; set; }
        public short Quantity { get; set; }
        public float Discount { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
