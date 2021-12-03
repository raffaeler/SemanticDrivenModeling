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

.NET 5

|                     Method |     Mean |     Error |    StdDev |
|--------------------------- |---------:|----------:|----------:|
|                PlainOrders | 9.702 us | 0.1862 us | 0.1992 us |
|          PlainOnlineOrders | 6.832 us | 0.1318 us | 0.1518 us |
| SemanticOrderToOnlineOrder | 6.108 us | 0.1186 us | 0.1318 us |
| SemanticOnlineOrderToOrder | 6.064 us | 0.0987 us | 0.0923 us |


.NET 6 (exe only)

|                     Method |     Mean |     Error |    StdDev |
|--------------------------- |---------:|----------:|----------:|
|                PlainOrders | 9.110 us | 0.1206 us | 0.1069 us |
|          PlainOnlineOrders | 5.973 us | 0.1042 us | 0.0924 us |
| SemanticOrderToOnlineOrder | 5.029 us | 0.0574 us | 0.0509 us |
| SemanticOnlineOrderToOrder | 5.559 us | 0.1088 us | 0.1253 us |


.NET 6 (entire solution)

|                     Method |     Mean |     Error |    StdDev |
|--------------------------- |---------:|----------:|----------:|
|                PlainOrders | 9.049 us | 0.1805 us | 0.3768 us |
|          PlainOnlineOrders | 5.990 us | 0.1068 us | 0.0999 us |
| SemanticOrderToOnlineOrder | 4.961 us | 0.0982 us | 0.1206 us |
| SemanticOnlineOrderToOrder | 5.301 us | 0.1022 us | 0.0798 us |

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

        //private JsonSerializerOptions _optionsOrdersToOnlineOrders;
        //private JsonSerializerOptions _optionsOnlineOrdersToOrders;
        private JsonSerializerOptions _options;

        private IList<SimpleDomain1.Order> _orders;
        private IList<SimpleDomain2.OnlineOrder> _onlineOrders;

        [GlobalSetup]
        public void Initialize()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            _domain = JsonSerializer.Deserialize<DomainBase>(jsonDomainDefinitions);
            DeserializeMapping();

            //_optionsOrdersToOnlineOrders = new JsonSerializerOptions()
            //{
            //    Converters = { new SemanticConverterFactory(_orderTypeSystem, _orderToOnlineOrderMapping), },
            //};

            //_optionsOnlineOrdersToOrders = new JsonSerializerOptions()
            //{
            //    Converters = { new SemanticConverterFactory(_onlineOrderTypeSystem, _onlineOrderToOrderMapping), },
            //};

            _options = new JsonSerializerOptions()
            {
                Converters = { new SemanticConverterFactory(
                    new[]{ _orderTypeSystem, _onlineOrderTypeSystem},
                    new[] {_orderToOnlineOrderMapping, _onlineOrderToOrderMapping}), },
            };

            _orders = SimpleDomain1.Samples.GetOrders();
            _onlineOrders = SimpleDomain2.Samples.GetOnlineOrders();


            JsonSerializer.Serialize(_orders);
            JsonSerializer.Serialize(_onlineOrders);

            // let the codegen cache the delegates
            JsonSerializer.Serialize(_orders, /*_optionsOrdersToOnlineOrders*/_options);
            JsonSerializer.Serialize(_onlineOrders, /*_optionsOnlineOrdersToOrders*/_options);
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
            var temp = JsonSerializer.Serialize(_orders, /*_optionsOrdersToOnlineOrders*/_options);
        }

        [Benchmark]
        public void SemanticOnlineOrderToOrder()
        {
            var temp = JsonSerializer.Serialize(_onlineOrders, /*_optionsOnlineOrdersToOrders*/_options);
        }
    }
}
