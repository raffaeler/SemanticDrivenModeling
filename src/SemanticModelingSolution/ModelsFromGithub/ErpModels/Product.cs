using System;

namespace ERP_Model.Models
{
    public class Product
    {
        public Guid ProductGuid { get; set; }

        public string ProductName { get; set; }

        public float ProductPrice { get; set; }

        public string ProductDescription { get; set; }
        public bool ProductDeleted { get; set; }
    }
}