using System;
using System.Collections.Generic;

namespace NorthwindDataLayer.Models
{
    public partial class Order_Subtotal
    {
        public long OrderID { get; set; }
        public Nullable<decimal> Subtotal { get; set; }
    }
}
