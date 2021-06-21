using System;

namespace ERP_Model.Models
{
    public class Supplier
    {
        public Guid SupplierGuid { get; set; }

        public string SupplierForName { get; set; }
        public string SupplierLastName { get; set; }

        public string SupplierCompany { get; set; }

        public virtual Address SupplierAddress { get; set; }

        public bool SupplierDeleted { get; set; }
    }
}