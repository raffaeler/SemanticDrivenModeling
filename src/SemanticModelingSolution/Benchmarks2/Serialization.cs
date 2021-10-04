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


V2

|                     Method |     Mean |     Error |    StdDev |
|--------------------------- |---------:|----------:|----------:|
|                PlainOrders | 9.702 us | 0.1862 us | 0.1992 us |
|          PlainOnlineOrders | 6.832 us | 0.1318 us | 0.1518 us |
| SemanticOrderToOnlineOrder | 6.108 us | 0.1186 us | 0.1318 us |
| SemanticOnlineOrderToOrder | 6.064 us | 0.0987 us | 0.0923 us |

*/

namespace Benchmarks
{
    public class Serialization
    {
        private DomainBase _domain;
        private TypeSystem<Metadata> _orderTypeSystem;
        private TypeSystem<Metadata> _onlineOrderTypeSystem;
        private Mapping _orderToOnlineOrderMapping;
        private Mapping _onlineOrderToOrderMapping;

        private JsonSerializerOptions _optionsOrdersToOnlineOrders;
        private JsonSerializerOptions _optionsOnlineOrdersToOrders;

        private IList<SimpleDomain1.Order> _orders;
        private IList<SimpleDomain2.OnlineOrder> _onlineOrders;

        [GlobalSetup]
        public void Initialize()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            _domain = JsonSerializer.Deserialize<DomainBase>(jsonDomainDefinitions);
            DeserializeMapping();

            _optionsOrdersToOnlineOrders = new JsonSerializerOptions()
            {
                Converters = { new SemanticConverterFactory(_orderTypeSystem, _orderToOnlineOrderMapping), },
            };

            _optionsOnlineOrdersToOrders = new JsonSerializerOptions()
            {
                Converters = { new SemanticConverterFactory(_onlineOrderTypeSystem, _onlineOrderToOrderMapping), },
            };

            _orders = SimpleDomain1.Samples.GetOrders();
            _onlineOrders = SimpleDomain2.Samples.GetOnlineOrders();


            JsonSerializer.Serialize(_orders);
            JsonSerializer.Serialize(_onlineOrders);

            // let the codegen cache the delegates
            JsonSerializer.Serialize(_orders, _optionsOrdersToOnlineOrders);
            JsonSerializer.Serialize(_onlineOrders, _optionsOnlineOrdersToOrders);
        }

        private void DeserializeMapping()
        {
            var orderTypeSystemJson = File.ReadAllText("Metadata\\V2OrderTypeSystem.json");
            var onlineOrderTypeSystemJson = File.ReadAllText("Metadata\\V2OnlineOrderTypeSystem.json");
            var orderToOnlineOrderMappingsJson = File.ReadAllText("Metadata\\V2Order2OnlineOrderMapping.json");
            var onlineOrderToOrderMappingsJson = File.ReadAllText("Metadata\\V2OnlineOrder2OrderMapping.json");
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
            var temp = JsonSerializer.Serialize(_orders);
        }

        [Benchmark]
        public void PlainOnlineOrders()
        {
            var temp = JsonSerializer.Serialize(_onlineOrders);
        }

        [Benchmark]
        public void SemanticOrderToOnlineOrder()
        {
            var temp = JsonSerializer.Serialize(_orders, _optionsOrdersToOnlineOrders);
        }

        [Benchmark]
        public void SemanticOnlineOrderToOrder()
        {
            var temp = JsonSerializer.Serialize(_onlineOrders, _optionsOnlineOrdersToOrders);
        }
    }
}
