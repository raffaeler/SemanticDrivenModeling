using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using MaterializerLibrary;

using SemanticLibrary;

using SurrogateLibrary;

namespace MappingConsole
{
    public record A
    {
        [JsonConstructor]
        public A(int x) { }

        public int X { get; init; }

    }

    public record B
    {
        private int x;
        [JsonConstructor]
        public B(int x) { this.x = x; X = (double)x; }

        public double X { get; init; }
    }


    internal class SemanticTests
    {
        private DomainBase _domain;
        private TypeSystem<Metadata> _orderTypeSystem;
        private TypeSystem<Metadata> _onlineOrderTypeSystem;
        private SurrogateType<Metadata> _orderType;
        private SurrogateType<Metadata> _onlineOrderType;
        private Mapping _orderToOnlineOrderMapping;
        private Mapping _onlineOrderToOrderMapping;


        public void Run()
        {
            //Debug.Assert(new A("1", "2").Equals(new A("1", "2")));
            //var aa = new A(1);
            //var j = JsonSerializer.Serialize(aa);
            //var bb = JsonSerializer.Deserialize<B>(j);
            Concept u = new Concept("Undefined", "Used to mark unmappable concepts in the graph");
            var ju = JsonSerializer.Serialize(u);
            var u2 = JsonSerializer.Deserialize<Concept>(ju);
            Debug.Assert(u2 == u);

            //AssignSemantic();
            //ComputeMappings();
            //SerializeMapping();
            //DeserializeMappingAndAssert();

            DeserializeMapping();

            SerializeOrders();
            //SerializeOnlineOrders();

            DeserializeOrders();
            DeserializeOnlineOrders();
        }

        private void AssignSemantic()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            _domain = JsonSerializer.Deserialize<DomainBase>(jsonDomainDefinitions);

            _orderTypeSystem = new TypeSystem<Metadata>();
            _orderType = _orderTypeSystem.GetOrCreate(typeof(SimpleDomain1.Order));
            _orderTypeSystem.UpdateCache();

            _onlineOrderTypeSystem = new TypeSystem<Metadata>();
            _onlineOrderType = _onlineOrderTypeSystem.GetOrCreate(typeof(SimpleDomain2.OnlineOrder));
            _onlineOrderTypeSystem.UpdateCache();

            var analysis = new SemanticAnalysis(_domain);
            analysis.AssignSemantics(_orderType);
            analysis.AssignSemantics(_onlineOrderType);
        }

        private void ComputeMappings()
        {
            var matcher1 = new ConceptMatchingRule(_orderTypeSystem, _onlineOrderTypeSystem, true);
            var orderToOnlineOrderMappings = matcher1.ComputeMappings(_orderType);
            _orderToOnlineOrderMapping = orderToOnlineOrderMappings.First();

            var matcher2 = new ConceptMatchingRule(_onlineOrderTypeSystem, _orderTypeSystem, true);
            var onlineOrderToOrderMappings = matcher2.ComputeMappings(_onlineOrderType);
            _onlineOrderToOrderMapping = onlineOrderToOrderMappings.First();
        }

        private void SerializeMapping()
        {
            var orderTypeSystem = JsonSerializer.Serialize(_orderTypeSystem);
            var onlineOrderTypeSystem = JsonSerializer.Serialize(_onlineOrderTypeSystem);
            var orderToOnlineOrderMappings = JsonSerializer.Serialize(_orderToOnlineOrderMapping);
            var onlineOrderToOrderMappings = JsonSerializer.Serialize(_onlineOrderToOrderMapping);
            File.WriteAllText("V2OrderTypeSystem.json", orderTypeSystem);
            File.WriteAllText("V2OnlineOrderTypeSystem.json", onlineOrderTypeSystem);
            File.WriteAllText("V2Order2OnlineOrderMapping.json", orderToOnlineOrderMappings);
            File.WriteAllText("V2OnlineOrder2OrderMapping.json", onlineOrderToOrderMappings);
        }

        private void DeserializeMapping()
        {
            var orderTypeSystemJson = File.ReadAllText("V2OrderTypeSystem.json");
            var onlineOrderTypeSystemJson = File.ReadAllText("V2OnlineOrderTypeSystem.json");
            var orderToOnlineOrderMappingsJson = File.ReadAllText("V2Order2OnlineOrderMapping.json");
            var onlineOrderToOrderMappingsJson = File.ReadAllText("V2OnlineOrder2OrderMapping.json");
            _orderTypeSystem = JsonSerializer.Deserialize<TypeSystem<Metadata>>(orderTypeSystemJson);
            _onlineOrderTypeSystem = JsonSerializer.Deserialize<TypeSystem<Metadata>>(onlineOrderTypeSystemJson);
            _orderToOnlineOrderMapping = JsonSerializer.Deserialize<Mapping>(orderToOnlineOrderMappingsJson);
            _onlineOrderToOrderMapping = JsonSerializer.Deserialize<Mapping>(onlineOrderToOrderMappingsJson);

            _orderTypeSystem.UpdateCache();
            _onlineOrderTypeSystem.UpdateCache();
            _orderToOnlineOrderMapping.UpdateCache(_orderTypeSystem, _onlineOrderTypeSystem);
            _onlineOrderToOrderMapping.UpdateCache(_onlineOrderTypeSystem, _orderTypeSystem);
        }

        private void DeserializeMappingAndAssert()
        {
            var orderTypeSystemJson = File.ReadAllText("V2OrderTypeSystem.json");
            var onlineOrderTypeSystemJson = File.ReadAllText("V2OnlineOrderTypeSystem.json");
            var orderToOnlineOrderMappingsJson = File.ReadAllText("V2Order2OnlineOrderMapping.json");
            var onlineOrderToOrderMappingsJson = File.ReadAllText("V2OnlineOrder2OrderMapping.json");
            var orderTypeSystem = JsonSerializer.Deserialize<TypeSystem<Metadata>>(orderTypeSystemJson);
            var onlineOrderTypeSystem = JsonSerializer.Deserialize<TypeSystem<Metadata>>(onlineOrderTypeSystemJson);
            var orderToOnlineOrderMapping = JsonSerializer.Deserialize<Mapping>(orderToOnlineOrderMappingsJson);
            var onlineOrderToOrderMapping = JsonSerializer.Deserialize<Mapping>(onlineOrderToOrderMappingsJson);

            orderTypeSystem.UpdateCache();
            onlineOrderTypeSystem.UpdateCache();
            orderToOnlineOrderMapping.UpdateCache(orderTypeSystem, onlineOrderTypeSystem);
            onlineOrderToOrderMapping.UpdateCache(onlineOrderTypeSystem, orderTypeSystem);
            Debug.Assert(orderTypeSystem == _orderTypeSystem);
            Debug.Assert(onlineOrderTypeSystem == _onlineOrderTypeSystem);
            Debug.Assert(orderToOnlineOrderMapping == _orderToOnlineOrderMapping);
            Debug.Assert(onlineOrderToOrderMapping == _onlineOrderToOrderMapping);
        }

        private void PrintMap(Mapping mapping)
        {
            Console.WriteLine($"Conversion Map: {mapping.Source.FullName} => {mapping.Target.FullName}");
            foreach (var m in mapping.Mappings
                .Select(m => (source: m.Source.GetLeafPathAlt(), target: m.Target.GetLeafPathAlt()))
                .OrderBy(m => m.source))
            {
                Console.Write($"{m.source.PadRight(50)}");
                Console.Write($"{m.target.PadRight(50)}");
                Console.WriteLine();
            }
       
            Console.WriteLine();
        }

        private void SerializeOrders()
        {
            var settings = new JsonSerializerOptions()
            {
                //Converters = { new SemanticConverterFactory(_orderTypeSystem, _orderToOnlineOrderMapping), },
                Converters = { new SemanticConverterFactory(
                    new[]{ _orderTypeSystem, _onlineOrderTypeSystem},
                    new[] {_orderToOnlineOrderMapping, _onlineOrderToOrderMapping}), },
            };

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            var json = JsonSerializer.Serialize(sourceObjects, settings);
            var targetObjects = (IEnumerable<SimpleDomain2.OnlineOrder>)
                JsonSerializer.Deserialize(json, typeof(SimpleDomain2.OnlineOrder[]));
        }

        private void SerializeOnlineOrders()
        {
            var settings = new JsonSerializerOptions()
            {
                //Converters = { new SemanticConverterFactory(_onlineOrderTypeSystem, _onlineOrderToOrderMapping), },
                Converters = { new SemanticConverterFactory(
                    new[]{ _orderTypeSystem, _onlineOrderTypeSystem},
                    new[] {_orderToOnlineOrderMapping, _onlineOrderToOrderMapping}), },
            };

            var sourceObjects = SimpleDomain2.Samples.GetOnlineOrders();
            var json = JsonSerializer.Serialize(sourceObjects, settings);
            var targetObjects = (IEnumerable<SimpleDomain1.Order>)
                JsonSerializer.Deserialize(json, typeof(SimpleDomain1.Order[]));
        }

        private void DeserializeOrders()
        {
            PrintMap(_onlineOrderToOrderMapping);

            var settings = new JsonSerializerOptions()
            {
                //Converters = { new SemanticConverterFactory(_orderTypeSystem, _onlineOrderToOrderMapping), },
                Converters = { new SemanticConverterFactory(
                    new[]{ _orderTypeSystem, _onlineOrderTypeSystem},
                    new[] {_orderToOnlineOrderMapping, _onlineOrderToOrderMapping}), },
            };

            var sourceObjects = SimpleDomain2.Samples.GetOnlineOrders();
            var json = JsonSerializer.Serialize(sourceObjects);

            var targetObjects = (IEnumerable<SimpleDomain1.Order>)
                JsonSerializer.Deserialize(json, typeof(SimpleDomain1.Order[]), settings);
        }

        private void DeserializeOnlineOrders()
        {
            var settings = new JsonSerializerOptions()
            {
                //Converters = { new SemanticConverterFactory(_onlineOrderTypeSystem, _orderToOnlineOrderMapping), },
                Converters = { new SemanticConverterFactory(
                    new[]{ _orderTypeSystem, _onlineOrderTypeSystem},
                    new[] {_orderToOnlineOrderMapping, _onlineOrderToOrderMapping}), },
            };

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            var json = JsonSerializer.Serialize(sourceObjects);

            var targetObjects = (IEnumerable<SimpleDomain2.OnlineOrder>)
                JsonSerializer.Deserialize(json, typeof(SimpleDomain2.OnlineOrder[]), settings);
        }

    }
}
