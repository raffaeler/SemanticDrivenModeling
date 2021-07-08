using System;
using System.Collections.Generic;

namespace NorthwindDataLayer.Models
{
    public partial class Sales_by_Category
    {
        public long CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string ProductName { get; set; }
        public Nullable<decimal> ProductSales { get; set; }
    }
}
