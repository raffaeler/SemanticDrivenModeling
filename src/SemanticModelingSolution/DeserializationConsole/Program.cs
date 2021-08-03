using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using SemanticLibrary;

namespace DeserializationConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            var p = new Program();
            //new VerifyDeserialization().Test();

            //p.MappingVendorToSupplier();
            //p.MappingSupplierToVendor();

            //p.MappingOrderToOnlineOrder();
            p.MappingOnlineOrderToOrder();
        }

        public void MappingVendorToSupplier()
        {
            var analyzer = new Analyzer();
            var erp = analyzer.Prepare("ERP", ERP_Model.Types.All);
            var coderushModel = analyzer.Prepare("coderush", coderush.Types.All);
            var northwind = analyzer.Prepare("Northwind", NorthwindDataLayer.Types.All);

            //TestMaterializer("Vendor", analyzer, coderushModel, erp);

            var sourceObjects = Samples.GetVendors1();
            var utilities = new MappingUtilities();
            var targetObjects = utilities.Transform<coderush.Models.Vendor, ERP_Model.Models.Supplier>("Vendor", coderushModel, erp, sourceObjects);
        }

        public void MappingSupplierToVendor()
        {
            var analyzer = new Analyzer();
            var erp = analyzer.Prepare("ERP", ERP_Model.Types.All);
            var coderushModel = analyzer.Prepare("coderush", coderush.Types.All);
            var northwind = analyzer.Prepare("Northwind", NorthwindDataLayer.Types.All);

            //TestMaterializer("Supplier", analyzer, erp, coderushModel);

            var sourceObjects = Samples.GetSupplier1();
            var utilities = new MappingUtilities();
            var targetObjects = utilities.Transform<ERP_Model.Models.Supplier, coderush.Models.Vendor>("Supplier", erp, coderushModel, sourceObjects);
        }

        public void MappingOrderToOnlineOrder()
        {
            var analyzer = new Analyzer();
            var m1 = analyzer.Prepare("SimpleDomain1", SimpleDomain1.Types.All);
            var m2 = analyzer.Prepare("SimpleDomain2", SimpleDomain2.Types.All);

            //TestMaterializer("Order", analyzer, m1, m2);

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            var utilities = new MappingUtilities();
            var targetObjects = utilities.Transform<SimpleDomain1.Order, SimpleDomain2.OnlineOrder>("Order", m1, m2, sourceObjects);

            //var targetObjects = utilities.OrderToOnlineOrder(m1, m2, sourceObjects);
        }


        public void MappingOnlineOrderToOrder()
        {
            var analyzer = new Analyzer();
            var m1 = analyzer.Prepare("SimpleDomain1", SimpleDomain1.Types.All);
            var m2 = analyzer.Prepare("SimpleDomain2", SimpleDomain2.Types.All);

            //TestMaterializer("OnlineOrder", analyzer, m2, m1);

            var sourceObjects = SimpleDomain2.Samples.GetOrders();
            var utilities = new MappingUtilities();

            var targetObjects = utilities.Transform<SimpleDomain2.OnlineOrder, SimpleDomain1.Order>("OnlineOrder", m2, m1, sourceObjects);
            //var targetObjects = utilities.OnlineOrderToOrder(m2, m1, sourceObjects);

        }


        public void TestMaterializer(string sourceItem, Analyzer analyzer,IList<ModelTypeNode> source, IList<ModelTypeNode> target)
        {
            var sourceModel = source.First(t => t.TypeName == sourceItem);
            var mapping = analyzer.CreateMappingsFor(sourceModel, target);

            var materializer = new Materializer();
            materializer.Materialize(mapping);

        }
    }
}




