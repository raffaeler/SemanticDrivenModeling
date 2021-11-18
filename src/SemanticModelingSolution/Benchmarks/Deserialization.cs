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
using SurrogateLibrary;

/*

Using reflection (.NET 5):

|                     Method |         Mean |      Error |     StdDev |
|--------------------------- |-------------:|-----------:|-----------:|
|                PlainOrders |     18.83 us |   0.376 us |   0.386 us |
|          PlainOnlineOrders |     11.42 us |   0.202 us |   0.189 us |
| SemanticOrderToOnlineOrder | 18,175.24 us | 235.091 us | 196.312 us |
| SemanticOnlineOrderToOrder | 21,196.31 us | 304.601 us | 284.924 us | 

Using code generation (.NET 5):

|                     Method |     Mean |    Error |   StdDev |
|--------------------------- |---------:|---------:|---------:|
|                PlainOrders | 18.16 us | 0.363 us | 0.496 us |
|          PlainOnlineOrders | 10.23 us | 0.172 us | 0.205 us |
| SemanticOrderToOnlineOrder | 32.91 us | 0.638 us | 0.655 us |
| SemanticOnlineOrderToOrder | 28.59 us | 0.519 us | 0.433 us |

Using code generation (.NET 6):

|                     Method |      Mean |     Error |    StdDev |
|--------------------------- |----------:|----------:|----------:|
|                PlainOrders | 15.984 us | 0.2327 us | 0.2177 us |
|          PlainOnlineOrders |  9.308 us | 0.1468 us | 0.1373 us |
| SemanticOrderToOnlineOrder | 27.519 us | 0.4234 us | 0.3754 us |
| SemanticOnlineOrderToOrder | 25.577 us | 0.4833 us | 0.4521 us |

*/

namespace Benchmarks
{
    public class Deserialization
    {
        private DomainBase _domain;
        private TypeSystem<Metadata> _orderTypeSystem;
        private TypeSystem<Metadata> _onlineOrderTypeSystem;
        private Mapping _orderToOnlineOrderMapping;
        private Mapping _onlineOrderToOrderMapping;

        private JsonSerializerOptions _optionsOrdersToOnlineOrders;
        private JsonSerializerOptions _optionsOnlineOrdersToOrders;

        private string _orders;
        private string _onlineOrders;

        [GlobalSetup]
        public void Initialize()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            _domain = JsonSerializer.Deserialize<DomainBase>(jsonDomainDefinitions);
            DeserializeMapping();

            _optionsOrdersToOnlineOrders = new JsonSerializerOptions()
            {
                Converters = { new SemanticConverterFactory(_onlineOrderTypeSystem, _orderToOnlineOrderMapping), },
            };

            _optionsOnlineOrdersToOrders = new JsonSerializerOptions()
            {
                Converters = { new SemanticConverterFactory(_orderTypeSystem, _onlineOrderToOrderMapping), },
            };

            var sourceOrders = SimpleDomain1.Samples.GetOrders();
            _orders = JsonSerializer.Serialize(sourceOrders);

            var sourceOnlineOrders = SimpleDomain2.Samples.GetOnlineOrders();
            _onlineOrders = JsonSerializer.Serialize(sourceOnlineOrders);

            JsonSerializer.Deserialize<SimpleDomain1.Order[]>(_orders);
            JsonSerializer.Deserialize<SimpleDomain2.OnlineOrder[]>(_onlineOrders);

            // let the codegen cache the delegates
            JsonSerializer.Deserialize<SimpleDomain2.OnlineOrder[]>(_orders, _optionsOrdersToOnlineOrders);
            JsonSerializer.Deserialize<SimpleDomain1.Order[]>(_onlineOrders, _optionsOnlineOrdersToOrders);
        }

        private void DeserializeMapping()
        {
            var orderTypeSystemJson = File.ReadAllText("Metadata\\OrderTypeSystem.json");
            var onlineOrderTypeSystemJson = File.ReadAllText("Metadata\\OnlineOrderTypeSystem.json");
            var orderToOnlineOrderMappingsJson = File.ReadAllText("Metadata\\Order2OnlineOrderMapping.json");
            var onlineOrderToOrderMappingsJson = File.ReadAllText("Metadata\\OnlineOrder2OrderMapping.json");
            _orderTypeSystem = JsonSerializer.Deserialize<TypeSystem<Metadata>>(orderTypeSystemJson);
            _onlineOrderTypeSystem = JsonSerializer.Deserialize<TypeSystem<Metadata>>(onlineOrderTypeSystemJson);
            _orderToOnlineOrderMapping = JsonSerializer.Deserialize<Mapping>(orderToOnlineOrderMappingsJson);
            _onlineOrderToOrderMapping = JsonSerializer.Deserialize<Mapping>(onlineOrderToOrderMappingsJson);

            _orderTypeSystem.UpdateCache();
            _onlineOrderTypeSystem.UpdateCache();
            _orderToOnlineOrderMapping.UpdateCache(_orderTypeSystem, _onlineOrderTypeSystem);
            _onlineOrderToOrderMapping.UpdateCache(_onlineOrderTypeSystem, _orderTypeSystem);
        }


        [GlobalCleanup]
        public void Cleanup()
        {
        }

        [Benchmark]
        public void PlainOrders()
        {
            var temp = JsonSerializer.Deserialize<SimpleDomain1.Order[]>(_orders);
        }

        [Benchmark]
        public void PlainOnlineOrders()
        {
            var temp = JsonSerializer.Deserialize<SimpleDomain2.OnlineOrder[]>(_onlineOrders);
        }

        [Benchmark]
        public void SemanticOrderToOnlineOrder()
        {
            var temp = JsonSerializer.Deserialize<SimpleDomain2.OnlineOrder[]>(_orders, _optionsOrdersToOnlineOrders);
        }

        [Benchmark]
        public void SemanticOnlineOrderToOrder()
        {
            var temp = JsonSerializer.Deserialize<SimpleDomain1.Order[]>(_onlineOrders, _optionsOnlineOrdersToOrders);
        }
    }
}
