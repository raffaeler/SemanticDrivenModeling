﻿using System;
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

|                     Method |      Mean |     Error |    StdDev |
|--------------------------- |----------:|----------:|----------:|
|                PlainOrders | 10.944 us | 0.2182 us | 0.3266 us |
|          PlainOnlineOrders |  7.648 us | 0.1466 us | 0.1440 us |
| SemanticOrderToOnlineOrder |  6.732 us | 0.1290 us | 0.1380 us |
| SemanticOnlineOrderToOrder |  6.923 us | 0.0940 us | 0.0833 us |
*/

namespace Benchmarks
{
    public class Serialization
    {
        private ScoredTypeMapping _onlineOrderToOrder;
        private ScoredTypeMapping _orderToOnlineOrder;

        private JsonSerializerOptions _optionsOrdersToOnlineOrders;
        private JsonSerializerOptions _optionsOnlineOrdersToOrders;

        private IList<SimpleDomain1.Order> _orders;
        private IList<SimpleDomain2.OnlineOrder> _onlineOrders;

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

            _orders = SimpleDomain1.Samples.GetOrders();
            _onlineOrders = SimpleDomain2.Samples.GetOnlineOrders();

            _optionsOrdersToOnlineOrders = utilities.CreateSettings(_orderToOnlineOrder);
            _optionsOnlineOrdersToOrders = utilities.CreateSettings(_onlineOrderToOrder);

            JsonSerializer.Serialize(_orders);
            JsonSerializer.Serialize(_onlineOrders);

            // let the codegen cache the delegates
            JsonSerializer.Serialize(_orders, _optionsOrdersToOnlineOrders);
            JsonSerializer.Serialize(_onlineOrders, _optionsOnlineOrdersToOrders);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
        }

        [Benchmark]
        public void PlainOrders()
        {
            JsonSerializer.Serialize(_orders);
        }

        [Benchmark]
        public void PlainOnlineOrders()
        {
            JsonSerializer.Serialize(_onlineOrders);
        }

        [Benchmark]
        public void SemanticOrderToOnlineOrder()
        {
            JsonSerializer.Serialize(_orders, _optionsOrdersToOnlineOrders);
        }

        [Benchmark]
        public void SemanticOnlineOrderToOrder()
        {
            JsonSerializer.Serialize(_onlineOrders, _optionsOnlineOrdersToOrders);
        }
    }
}
