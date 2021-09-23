using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MaterializerLibrary;

namespace DeserializationConsole
{
    internal class VendorTests
    {
        public void MappingVendorToSupplier()
        {
            var domain = new GeneratedCode.Domain();
            var utilities = new MappingUtilities(domain);

            var erp = utilities.Prepare("ERP", ERP_Model.Types.All);
            var coderushModel = utilities.Prepare("coderush", coderush.Types.All);
            var northwind = utilities.Prepare("Northwind", NorthwindDataLayer.Types.All);

            //TestMaterializer("Vendor", utilities, coderushModel, erp);

            var sourceObjects = Samples.GetVendors1();
            //var targetObjects = utilities.Transform<coderush.Models.Vendor, ERP_Model.Models.Supplier>(
            //    "Vendor", coderushModel, erp, sourceObjects);

            var targetObjects = utilities.TransformDeserialize<ERP_Model.Models.Supplier>(
                "Vendor", coderushModel, erp, sourceObjects);
        }

        public void MappingSupplierToVendor()
        {
            var domain = new GeneratedCode.Domain();
            var utilities = new MappingUtilities(domain);

            var erp = utilities.Prepare("ERP", ERP_Model.Types.All);
            var coderushModel = utilities.Prepare("coderush", coderush.Types.All);
            var northwind = utilities.Prepare("Northwind", NorthwindDataLayer.Types.All);

            //TestMaterializer("Supplier", utilities, erp, coderushModel);

            var sourceObjects = Samples.GetSupplier1();
            //var targetObjects = utilities.Transform<ERP_Model.Models.Supplier, coderush.Models.Vendor>(
            //    "Supplier", erp, coderushModel, sourceObjects);

            var targetObjects = utilities.TransformDeserialize<coderush.Models.Vendor>(
                "Supplier", erp, coderushModel, sourceObjects);
        }


    }
}
