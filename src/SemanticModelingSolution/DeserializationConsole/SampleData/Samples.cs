using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeserializationConsole
{
    public static class Samples
    {
        public static  coderush.Models.Vendor[] GetVendors1() => new coderush.Models.Vendor[]
        {
            new ()
            {
                VendorId = 1,
                Address = "Address1",
                City = "City1",
                ContactPerson = "ContactPerson1",
                Email = "Email1",
                Phone = "Phone1",
                State = "State1",
                VendorName = "VendorName1",
                VendorTypeId = 991,
                ZipCode = "ZipCode1",
            },
            new ()
            {
                VendorId = 2,
                Address = "Address2",
                City = "City2",
                ContactPerson = "ContactPerson2",
                Email = "Email2",
                Phone = "Phone2",
                State = "State2",
                VendorName = "VendorName2",
                VendorTypeId = 992,
                ZipCode = "ZipCode2",
            },
        };

        public static ERP_Model.Models.Supplier[] GetSupplier1() => new ERP_Model.Models.Supplier[]
        {
            new ERP_Model.Models.Supplier
            {
                SupplierGuid= Guid.NewGuid(),
                SupplierCompany = "Company1",
                SupplierAddress= new ERP_Model.Models.Address()
                {
                    AddressGuid = Guid.NewGuid(),
                    AddressCity = "AddressCity1",
                    AddressCompany = "AddressCompany1",
                    AddressCountry = "AddressCountry1",
                    AddressDeleted = false,
                    AddressDescription = "AddressDescription1",
                    AddressEmail = "AddressEmail1",
                    AddressForName = "AddressForName1",
                    AddressLastName = "AddressLastName1",
                    AddressPhone = 1,
                    AddressStreet = "AddressStreet1",
                    AddressZipCode = "AddressZipCode1",
                },
                SupplierDeleted = false,
                SupplierForName= "ForeName1",
                SupplierLastName= "LastName1",
            },
            new ERP_Model.Models.Supplier
            {
                SupplierGuid= Guid.NewGuid(),
                SupplierCompany = "Company2",
                SupplierAddress= new ERP_Model.Models.Address()
                {
                    AddressGuid = Guid.NewGuid(),
                    AddressCity = "AddressCity2",
                    AddressCompany = "AddressCompany2",
                    AddressCountry = "AddressCountry2",
                    AddressDeleted = false,
                    AddressDescription = "AddressDescription2",
                    AddressEmail = "AddressEmail2",
                    AddressForName = "AddressForName2",
                    AddressLastName = "AddressLastName2",
                    AddressPhone = 2,
                    AddressStreet = "AddressStreet2",
                    AddressZipCode = "AddressZipCode2",
                },
                SupplierDeleted = false,
                SupplierForName= "ForeName2",
                SupplierLastName= "LastName2",
            },
        };





    }
}
