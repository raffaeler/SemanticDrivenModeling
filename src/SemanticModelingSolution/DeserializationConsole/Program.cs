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
            //new VerifyDeserialization().Test();

            //var v = new VendorTests();
            //v.MappingVendorToSupplier();
            //v.MappingSupplierToVendor();

            //p.SerializeAll();
            //p.SerializeMappings();

            p.OnlineOrder_To_Order_UsingDeserialization();
            p.Order_To_OnlineOrder_UsingDeserialization();

            p.OnlineOrder_To_Order_UsingSerialization();
            p.Order_To_OnlineOrder_UsingSerialization();
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
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            var jsonDomain1 = File.ReadAllText("Metadata\\domain1types.json");
            var jsonDomain2 = File.ReadAllText("Metadata\\domain2types.json");

            var domain = JsonSerializer.Deserialize<GeneratedCode.Domain>(jsonDomainDefinitions);
            var utilities = new MappingUtilities(domain);
            var m1 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain1, domain);
            var m2 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain2, domain);

            //var mapping = utilities.GetMappings("Order", m1, m2);
            utilities.SerializeMapping(domain, "Order", m1, m2, "OrderToOnlineOrder.json");
            utilities.SerializeMapping(domain, "OnlineOrder", m2, m1, "OnlineOrderToOrder.json");
        }

        public ScoredTypeMapping PrepareOrderMappings()
        {
            var domain = new GeneratedCode.Domain();
            var utilities = new MappingUtilities(domain);
            var m1 = utilities.Prepare("SimpleDomain1", SimpleDomain1.Types.All);
            var m2 = utilities.Prepare("SimpleDomain2", SimpleDomain2.Types.All);

            var mappingFromOrder = utilities.GetMappings("Order", m1, m2);
            return mappingFromOrder;
        }

        public ScoredTypeMapping PrepareOnlineOrderMappings()
        {
            var domain = new GeneratedCode.Domain();
            var utilities = new MappingUtilities(domain);
            var m1 = utilities.Prepare("SimpleDomain1", SimpleDomain1.Types.All);
            var m2 = utilities.Prepare("SimpleDomain2", SimpleDomain2.Types.All);

            var mappingFromOnlineOrder = utilities.GetMappings("OnlineOrder", m2, m1);
            return mappingFromOnlineOrder;
        }

        public void OnlineOrder_To_Order_UsingDeserialization()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            var jsonMapping = File.ReadAllText("Metadata\\OnlineOrderToOrder.json");

            var domain = JsonSerializer.Deserialize<GeneratedCode.Domain>(jsonDomainDefinitions);
            var mapping = ModelTypeNodeExtensions.DeserializeMapping(jsonMapping, domain);
            var jsonOptions = GetJsonOptions(mapping);

            var sourceObjects = SimpleDomain2.Samples.GetOnlineOrders();
            var sourceJson = JsonSerializer.Serialize(sourceObjects);
            var targetObjects = JsonSerializer.Deserialize<SimpleDomain1.Order[]>(sourceJson, jsonOptions);
        }

        public void Order_To_OnlineOrder_UsingDeserialization()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            var jsonMapping = File.ReadAllText("Metadata\\OrderToOnlineOrder.json");

            var domain = JsonSerializer.Deserialize<GeneratedCode.Domain>(jsonDomainDefinitions);
            var mapping = ModelTypeNodeExtensions.DeserializeMapping(jsonMapping, domain);
            var jsonOptions = GetJsonOptions(mapping);

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            var sourceJson = JsonSerializer.Serialize(sourceObjects);
            var targetObjects = JsonSerializer.Deserialize<SimpleDomain2.OnlineOrder[]>(sourceJson, jsonOptions);
        }

        public void OnlineOrder_To_Order_UsingSerialization()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            var jsonMapping = File.ReadAllText("Metadata\\OnlineOrderToOrder.json");

            var domain = JsonSerializer.Deserialize<GeneratedCode.Domain>(jsonDomainDefinitions);
            var mapping = ModelTypeNodeExtensions.DeserializeMapping(jsonMapping, domain);
            var jsonOptions = GetJsonOptions(mapping);

            var sourceObjects = SimpleDomain2.Samples.GetOnlineOrders();
            var targetJson = JsonSerializer.Serialize(sourceObjects, jsonOptions);
            var targetObjects = JsonSerializer.Deserialize<SimpleDomain1.Order[]>(targetJson);
        }

        public void Order_To_OnlineOrder_UsingSerialization()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            var jsonMapping = File.ReadAllText("Metadata\\OrderToOnlineOrder.json");

            var domain = JsonSerializer.Deserialize<GeneratedCode.Domain>(jsonDomainDefinitions);
            var mapping = ModelTypeNodeExtensions.DeserializeMapping(jsonMapping, domain);
            var jsonOptions = GetJsonOptions(mapping);

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            var targetJson = JsonSerializer.Serialize(sourceObjects, jsonOptions);
            var targetObjects = JsonSerializer.Deserialize<SimpleDomain2.OnlineOrder[]>(targetJson);
        }

        private JsonSerializerOptions GetJsonOptions(ScoredTypeMapping mapping) => new JsonSerializerOptions()
            {
                Converters = { new SemanticConverterFactory(mapping), },
            };


    }
}




