using System;
using System.Collections.Generic;

namespace NorthwindDataLayer.Models
{
    public partial class Alphabetical_list_of_product
    {
        public long ProductID { get; set; }
        public string ProductName { get; set; }
        public Nullable<long> SupplierID { get; set; }
        public Nullable<long> CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<short> UnitsInStock { get; set; }
        public Nullable<short> UnitsOnOrder { get; set; }
        public Nullable<short> ReorderLevel { get; set; }
        public bool Discontinued { get; set; }
        public string CategoryName { get; set; }
    }
}
