using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using MaterializerLibrary;

using SemanticLibrary;

namespace DeserializationConsole.ConsoleTests
{
    internal class OtherTests
    {
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

/*
        //TestMaterializer("Order", utilities, m1, m2);
        //TestMaterializer("OnlineOrder", utilities, m2, m1);
        public void TestMaterializer(string sourceItem, MappingUtilities utilities, IList<ModelTypeNode> source, IList<ModelTypeNode> target)
        {
            var sourceModel = source.First(t => t.Type.Name == sourceItem);
            var mapping = utilities.CreateMappingsFor(sourceModel, target);

            var materializer = new Materializer();
            materializer.Materialize(mapping);
        }


        public void MappingOnlineOrderToOrderUsingDeserialization()
        {
            var domain = new GeneratedCode.Domain();
            var utilities = new MappingUtilities(domain);
            var m1 = utilities.Prepare("SimpleDomain1", SimpleDomain1.Types.All);
            var m2 = utilities.Prepare("SimpleDomain2", SimpleDomain2.Types.All);

            var sourceObjects = SimpleDomain2.Samples.GetOnlineOrders();
            //var targetObjects = utilities.Transform<SimpleDomain2.OnlineOrder, SimpleDomain1.Order>(
            //    "OnlineOrder", m2, m1, sourceObjects);

            var targetObjects = utilities.TransformDeserialize<SimpleDomain1.Order>(
                "OnlineOrder", m2, m1, sourceObjects);
            //var targetObjects = utilities.OnlineOrderToOrder(m2, m1, sourceObjects);

            //var finalJson = utilities.SerializePlain(targetObjects);
        }

        public void MappingOrderToOnlineOrderUsingDeserialization()
        {
            var domain = new GeneratedCode.Domain();
            var utilities = new MappingUtilities(domain);
            var m1 = utilities.Prepare("SimpleDomain1", SimpleDomain1.Types.All);
            var m2 = utilities.Prepare("SimpleDomain2", SimpleDomain2.Types.All);

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            //var targetObjects = utilities.Transform<SimpleDomain1.Order, SimpleDomain2.OnlineOrder>(
            //    "Order", m1, m2, sourceObjects);

            var targetObjects = utilities.TransformDeserialize<SimpleDomain2.OnlineOrder>(
                "Order", m1, m2, sourceObjects);

            //var targetObjects = utilities.OrderToOnlineOrder(m1, m2, sourceObjects);
        }

        public void MappingOnlineOrderToOrderUsingSerialization()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            var jsonDomain1 = File.ReadAllText("Metadata\\domain1types.json");
            var jsonDomain2 = File.ReadAllText("Metadata\\domain2types.json");
            var jsonMapping = File.ReadAllText("Metadata\\OnlineOrderToOrder.json");


            var domain = JsonSerializer.Deserialize<GeneratedCode.Domain>(jsonDomainDefinitions);
            var mapping = ModelTypeNodeExtensions.DeserializeMapping(jsonMapping, domain);
            var utilities = new MappingUtilities(domain);

            //var m1 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain1, domain);
            //var m2 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain2, domain);
            //var mapping = utilities.GetMappings("OnlineOrder", m2, m1);

            var sourceObjects = SimpleDomain2.Samples.GetOnlineOrders();
            var targetObjects = utilities.TransformSerialize<SimpleDomain2.OnlineOrder, SimpleDomain1.Order>(
                mapping, sourceObjects);
        }

        public void MappingOrderToOnlineOrderUsingSerialization()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            var jsonDomain1 = File.ReadAllText("Metadata\\domain1types.json");
            var jsonDomain2 = File.ReadAllText("Metadata\\domain2types.json");
            var jsonMapping = File.ReadAllText("Metadata\\OrderToOnlineOrder.json");


            var domain = JsonSerializer.Deserialize<GeneratedCode.Domain>(jsonDomainDefinitions);
            var mapping = ModelTypeNodeExtensions.DeserializeMapping(jsonMapping, domain);
            var utilities = new MappingUtilities(domain);

            //var m1 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain1, domain);
            //var m2 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain2, domain);
            //var mapping = utilities.GetMappings("Order", m1, m2);

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            var targetObjects = utilities.TransformSerialize<SimpleDomain1.Order, SimpleDomain2.OnlineOrder>(
                mapping, sourceObjects);

            //var targetObjects = utilities.TransformSerialize<SimpleDomain1.Order, SimpleDomain2.OnlineOrder>(
            //    "Order", m1, m2, sourceObjects);
        }
*/

    }
}
