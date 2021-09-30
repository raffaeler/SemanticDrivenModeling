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
        public B(int x) { this.x = x;  X = (double)x; }

        public double X { get; init; }
    }


    internal class SemanticTests
    {
        private DomainBase _domain;
        private TypeSystem<Metadata> _sourceTypeSystem;
        private TypeSystem<Metadata> _targetTypeSystem;
        private SurrogateType<Metadata> _source;
        private SurrogateType<Metadata> _target;
        private Mapping _currentMapping;


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
            SerializeWithMapping();
        }

        private void AssignSemantic()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            _domain = JsonSerializer.Deserialize<DomainBase>(jsonDomainDefinitions);

            _sourceTypeSystem = new TypeSystem<Metadata>();
            _source = _sourceTypeSystem.GetOrCreate(typeof(SimpleDomain1.Order));
            _sourceTypeSystem.UpdateCache();

            _targetTypeSystem = new TypeSystem<Metadata>();
            _target = _targetTypeSystem.GetOrCreate(typeof(SimpleDomain2.OnlineOrder));
            _targetTypeSystem.UpdateCache();

            var analysis = new SemanticAnalysis2(_domain);
            analysis.AssignSemantics(_source);
            analysis.AssignSemantics(_target);
        }

        private void ComputeMappings()
        {
            var matcher = new ConceptMatchingRule(_sourceTypeSystem, _targetTypeSystem, true);
            var mappings = matcher.ComputeMappings(_source);
            _currentMapping = mappings.First();
        }

        private void SerializeMapping()
        {
            var orderTypeSystem = JsonSerializer.Serialize(_sourceTypeSystem);
            var onlineOrderTypeSystem = JsonSerializer.Serialize(_targetTypeSystem);
            var mapping = JsonSerializer.Serialize(_currentMapping);
            File.WriteAllText("V2OrderTypeSystem.json", orderTypeSystem);
            File.WriteAllText("V2OnlineOrderTypeSystem.json", onlineOrderTypeSystem);
            File.WriteAllText("V2Order2OnlineOrderMapping.json", mapping);
        }

        private void DeserializeMapping()
        {
            var orderTypeSystem = File.ReadAllText("V2OrderTypeSystem.json");
            var onlineOrderTypeSystem = File.ReadAllText("V2OnlineOrderTypeSystem.json");
            var mapping = File.ReadAllText("V2Order2OnlineOrderMapping.json");
            _sourceTypeSystem = JsonSerializer.Deserialize<TypeSystem<Metadata>>(orderTypeSystem);
            _targetTypeSystem = JsonSerializer.Deserialize<TypeSystem<Metadata>>(onlineOrderTypeSystem);
            _currentMapping = JsonSerializer.Deserialize<Mapping>(mapping);

            _sourceTypeSystem.UpdateCache();
            _targetTypeSystem.UpdateCache();
            _currentMapping.UpdateCache(_sourceTypeSystem, _targetTypeSystem);
        }

        private void DeserializeMappingAndAssert()
        {
            var orderTypeSystem = File.ReadAllText("V2OrderTypeSystem.json");
            var onlineOrderTypeSystem = File.ReadAllText("V2OnlineOrderTypeSystem.json");
            var mapping = File.ReadAllText("V2Order2OnlineOrderMapping.json");
            var sourceTypeSystem = JsonSerializer.Deserialize<TypeSystem<Metadata>>(orderTypeSystem);
            var targetTypeSystem = JsonSerializer.Deserialize<TypeSystem<Metadata>>(onlineOrderTypeSystem);
            var currentMapping = JsonSerializer.Deserialize<Mapping>(mapping);

            sourceTypeSystem.UpdateCache();
            targetTypeSystem.UpdateCache();
            currentMapping.UpdateCache(sourceTypeSystem, targetTypeSystem);
            Debug.Assert(sourceTypeSystem == _sourceTypeSystem);
            Debug.Assert(sourceTypeSystem == _sourceTypeSystem);
            Debug.Assert(currentMapping == _currentMapping);

            var diffProps = sourceTypeSystem.Properties.Except(_sourceTypeSystem.Properties).ToArray();
        }

        private void SerializeWithMapping()
        {
            var settings = new JsonSerializerOptions()
            {
                Converters = { new SemanticConverterFactory(_sourceTypeSystem, _targetTypeSystem, _currentMapping), },
            };

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            var json = JsonSerializer.Serialize(sourceObjects, settings);
            var targetObjects = (IEnumerable<SimpleDomain2.OnlineOrder>)
                JsonSerializer.Deserialize(json, typeof(SimpleDomain2.OnlineOrder[]));
        }


    }
}
