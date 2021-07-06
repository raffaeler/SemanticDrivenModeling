using System;
using System.Collections.Generic;
using System.Text;

namespace LegacyModels.Models3
{
    public class Order
    {
        public string Name { get; set; }
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public CompanyAddress ShipTo { get; set; }
        public CompanyAddress BillTo { get; set; }
    }


    public class CompanyAddress
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
