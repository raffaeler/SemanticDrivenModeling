using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using MaterializerLibrary;

using SemanticLibrary;

namespace DeserializationConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            var p = new Program();
            //p.SerializeAll();
            //p.SerializeMappings();

            //new VerifyDeserialization().Test();

            //p.MappingVendorToSupplier();
            //p.MappingSupplierToVendor();

            //p.MappingOrderToOnlineOrderUsingDeserialization();
            //p.MappingOnlineOrderToOrderUsingDeserialization();

            p.MappingOnlineOrderToOrderUsingSerialization();
            //p.MappingOrderToOnlineOrderUsingSerialization();
        }

        public void SerializeAll()
        {
            var domain = new GeneratedCode.Domain();
            var jsonDomainDefinitions = JsonSerializer.Serialize(domain);
            File.WriteAllText("domainDefinitions.json", jsonDomainDefinitions);

            var modelsDomain1 = new DomainTypesGraphVisitor(domain, SimpleDomain1.Types.All).Visit(null, null, null);
            var jsonDomain1 = modelsDomain1.Serialize(domain);
            File.WriteAllText("domain1types.json", jsonDomain1);

            var modelsDomain2 = new DomainTypesGraphVisitor(domain, SimpleDomain2.Types.All).Visit(null, null, null);
            var jsonDomain2 = modelsDomain2.Serialize(domain);
            File.WriteAllText("domain2types.json", jsonDomain2);
        }


        public void SerializeMappings()
        {
            var jsonDomainDefinitions = File.ReadAllText("Serializations\\domainDefinitions.json");
            var jsonDomain1 = File.ReadAllText("Serializations\\domain1types.json");
            var jsonDomain2 = File.ReadAllText("Serializations\\domain2types.json");

            var domain = JsonSerializer.Deserialize<GeneratedCode.Domain>(jsonDomainDefinitions);
            var utilities = new MappingUtilities(domain);
            var m1 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain1, domain);
            var m2 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain2, domain);

            //var mapping = utilities.GetMappings("Order", m1, m2);
            utilities.SerializeMapping(domain, "Order", m1, m2, "OrderToOnlineOrder.json");
            utilities.SerializeMapping(domain, "OnlineOrder", m2, m1, "OnlineOrderToOrder.json");
        }


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

        public void MappingOrderToOnlineOrderUsingDeserialization()
        {
            var domain = new GeneratedCode.Domain();
            var utilities = new MappingUtilities(domain);
            var m1 = utilities.Prepare("SimpleDomain1", SimpleDomain1.Types.All);
            var m2 = utilities.Prepare("SimpleDomain2", SimpleDomain2.Types.All);

            //TestMaterializer("Order", utilities, m1, m2);

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            //var targetObjects = utilities.Transform<SimpleDomain1.Order, SimpleDomain2.OnlineOrder>(
            //    "Order", m1, m2, sourceObjects);

            var targetObjects = utilities.TransformDeserialize<SimpleDomain2.OnlineOrder>(
                "Order", m1, m2, sourceObjects);

            //var targetObjects = utilities.OrderToOnlineOrder(m1, m2, sourceObjects);
        }


        public void MappingOnlineOrderToOrderUsingDeserialization()
        {
            var domain = new GeneratedCode.Domain();
            var utilities = new MappingUtilities(domain);
            var m1 = utilities.Prepare("SimpleDomain1", SimpleDomain1.Types.All);
            var m2 = utilities.Prepare("SimpleDomain2", SimpleDomain2.Types.All);

            //TestMaterializer("OnlineOrder", utilities, m2, m1);

            var sourceObjects = SimpleDomain2.Samples.GetOrders();
            //var targetObjects = utilities.Transform<SimpleDomain2.OnlineOrder, SimpleDomain1.Order>(
            //    "OnlineOrder", m2, m1, sourceObjects);

            var targetObjects = utilities.TransformDeserialize<SimpleDomain1.Order>(
                "OnlineOrder", m2, m1, sourceObjects);
            //var targetObjects = utilities.OnlineOrderToOrder(m2, m1, sourceObjects);

            //var finalJson = utilities.SerializePlain(targetObjects);
        }


        public void TestMaterializer(string sourceItem, MappingUtilities utilities, IList<ModelTypeNode> source, IList<ModelTypeNode> target)
        {
            var sourceModel = source.First(t => t.Type.Name == sourceItem);
            var mapping = utilities.CreateMappingsFor(sourceModel, target);

            var materializer = new Materializer();
            materializer.Materialize(mapping);
        }

        public void MappingOnlineOrderToOrderUsingSerialization()
        {
            var jsonDomainDefinitions = File.ReadAllText("Serializations\\domainDefinitions.json");
            var jsonDomain1 = File.ReadAllText("Serializations\\domain1types.json");
            var jsonDomain2 = File.ReadAllText("Serializations\\domain2types.json");


            var domain = JsonSerializer.Deserialize<GeneratedCode.Domain>(jsonDomainDefinitions);
            var utilities = new MappingUtilities(domain);
            var m1 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain1, domain);
            var m2 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain2, domain);

            var mapping = utilities.GetMappings("OnlineOrder", m2, m1);

            var sourceObjects = SimpleDomain2.Samples.GetOrders();
            var targetObjects = utilities.TransformSerialize<SimpleDomain2.OnlineOrder, SimpleDomain1.Order>(
                mapping, sourceObjects);
        }

        public void MappingOrderToOnlineOrderUsingSerialization()
        {
            var jsonDomainDefinitions = File.ReadAllText("Serializations\\domainDefinitions.json");
            var jsonDomain1 = File.ReadAllText("Serializations\\domain1types.json");
            var jsonDomain2 = File.ReadAllText("Serializations\\domain2types.json");


            var domain = JsonSerializer.Deserialize<GeneratedCode.Domain>(jsonDomainDefinitions);
            var utilities = new MappingUtilities(domain);
            var m1 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain1, domain);
            var m2 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain2, domain);

            var mapping = utilities.GetMappings("Order", m1, m2);

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            var targetObjects = utilities.TransformSerialize<SimpleDomain1.Order, SimpleDomain2.OnlineOrder>(
                mapping, sourceObjects);

            //var targetObjects = utilities.TransformSerialize<SimpleDomain1.Order, SimpleDomain2.OnlineOrder>(
            //    "Order", m1, m2, sourceObjects);
        }

        private ConversionLibrary.IConversion _conversion = null;

        private void Simulate(Utf8JsonWriter writer, SimpleDomain2.OnlineOrder item)
        {
            // 1. read the value from the object
            double value1 = item.OrderLines[0].Net;

            // 2. convert if needed
            string value2 = ((ConversionLibrary.Converters.ToStringConversion)_conversion)
                .From(value1);

            // 3. write to json
            writer.WriteStringValue(value2);

    //        ((Article)inputObject).ExpirationDate = reader.TokenType == JsonTokenType.Null
    //? default(DateTime)
    //: ((ToDateTimeConversion)_conversion).From(reader.GetDateTimeOffset());

        }

    }
}




