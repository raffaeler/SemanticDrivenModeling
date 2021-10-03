using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using MaterializerLibrary;

using SemanticLibrary;

/*
V1

|                     Method |         Mean |      Error |     StdDev |
|--------------------------- |-------------:|-----------:|-----------:|
|                PlainOrders |     17.95 us |   0.352 us |   0.312 us |
|          PlainOnlineOrders |     10.23 us |   0.140 us |   0.124 us |
| SemanticOrderToOnlineOrder | 15,496.51 us | 198.866 us | 186.019 us |
| SemanticOnlineOrderToOrder | 16,823.67 us | 270.981 us | 240.217 us |
*/

namespace Benchmarks
{
    public class Deserialization
    {
        private ScoredTypeMapping _onlineOrderToOrder;
        private ScoredTypeMapping _orderToOnlineOrder;

        private JsonSerializerOptions _optionsOrdersToOnlineOrders;
        private JsonSerializerOptions _optionsOnlineOrdersToOrders;

        private string _orders;
        private string _onlineOrders;

        [GlobalSetup]
        public void Initialize()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            var jsonDomain1 = File.ReadAllText("Metadata\\domain1types.json");
            var jsonDomain2 = File.ReadAllText("Metadata\\domain2types.json");

            var domain = JsonSerializer.Deserialize<DomainBase>(jsonDomainDefinitions);
            var utilities = new MappingUtilities(domain);
            //var m1 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain1, domain);
            //var m2 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain2, domain);
            _orderToOnlineOrder = utilities.DeserializeMapping(domain, "Metadata\\OrderToOnlineOrder.json");
            _onlineOrderToOrder = utilities.DeserializeMapping(domain, "Metadata\\OnlineOrderToOrder.json");

            var orders = SimpleDomain1.Samples.GetOrders();
            _orders = JsonSerializer.Serialize(orders);
            var onlineOrders = SimpleDomain2.Samples.GetOnlineOrders();
            _onlineOrders = JsonSerializer.Serialize(onlineOrders);

            _optionsOrdersToOnlineOrders = utilities.CreateSettings(_orderToOnlineOrder);
            _optionsOnlineOrdersToOrders = utilities.CreateSettings(_onlineOrderToOrder);

            JsonSerializer.Deserialize<SimpleDomain1.Order[]>(_orders);
            JsonSerializer.Deserialize<SimpleDomain2.OnlineOrder[]>(_onlineOrders);

            // let the codegen cache the delegates
            JsonSerializer.Deserialize<SimpleDomain2.OnlineOrder[]>(_orders, _optionsOrdersToOnlineOrders);
            JsonSerializer.Deserialize<SimpleDomain1.Order[]>(_onlineOrders, _optionsOnlineOrdersToOrders);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
        }

        [Benchmark]
        public void PlainOrders()
        {
            JsonSerializer.Deserialize<SimpleDomain1.Order[]>(_orders);
        }

        [Benchmark]
        public void PlainOnlineOrders()
        {
            JsonSerializer.Deserialize<SimpleDomain2.OnlineOrder[]>(_onlineOrders);
        }

        [Benchmark]
        public void SemanticOrderToOnlineOrder()
        {
            JsonSerializer.Deserialize<SimpleDomain2.OnlineOrder[]>(_orders, _optionsOrdersToOnlineOrders);
        }

        [Benchmark]
        public void SemanticOnlineOrderToOrder()
        {
            JsonSerializer.Deserialize<SimpleDomain1.Order[]>(_onlineOrders, _optionsOnlineOrdersToOrders);
        }
    }
}
