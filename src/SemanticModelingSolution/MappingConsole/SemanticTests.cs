using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using MaterializerLibrary;

using SemanticLibrary;

using SurrogateLibrary;

namespace MappingConsole
{
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
            AssignSemantic();
            ComputeMappings();
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
