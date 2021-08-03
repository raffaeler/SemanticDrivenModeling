using System;

namespace ERP_Model.Models
{
    public class Address
    {
        public Guid AddressGuid { get; set; }

        public string AddressDescription { get; set; }

        public string AddressStreet { get; set; }

        public string AddressZipCode { get; set; }

        public string AddressCity { get; set; }

        public string AddressCountry { get; set; }

        public string AddressEmail { get; set; }

        public long AddressPhone { get; set; }
        public string AddressLastName { get; set; }
        public string AddressForName { get; set; }
        public string AddressCompany { get; set; }
        public bool AddressDeleted { get; set; }
    }
}