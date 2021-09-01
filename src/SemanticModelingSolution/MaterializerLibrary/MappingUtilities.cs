using SemanticLibrary;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MaterializerLibrary
{
    public class MappingUtilities
    {
        private readonly DomainBase _domain;
        private readonly JsonSerializerOptions _jsonVanillaOptions;
        private Dictionary<string, Type[]> _models = new Dictionary<string, Type[]>();

        public MappingUtilities(DomainBase domain)
        {
            this._domain = domain;
            _jsonVanillaOptions = new JsonSerializerOptions()
            {
#if DEBUG
                WriteIndented = true,
#endif
                //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            };
        }

        public DomainBase Domain => _domain;

        public IDictionary<string, IList<ModelTypeNode>> ModelTypeNodes { get; set; } =
            new Dictionary<string, IList<ModelTypeNode>>();


        public string SerializePlain<T>(IEnumerable<T> item) => JsonSerializer.Serialize(item, SettingsVanilla);
        public string SerializeByTransform<T>(IEnumerable<T> item, JsonSerializerOptions options)
            => JsonSerializer.Serialize(item, options);

        public object DeserializePlain(string json, Type type) => JsonSerializer.Deserialize(json, type);
        public object DeserializeByTransform(string json, Type type, JsonSerializerOptions options) => JsonSerializer.Deserialize(json, type, options);

        protected virtual JsonSerializerOptions SettingsVanilla => _jsonVanillaOptions;

        protected virtual JsonSerializerOptions CreateSettings(ScoredTypeMapping scoredTypeMapping)
            => new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new SemanticConverterFactory(scoredTypeMapping),
                },
            };

        public IList<ModelTypeNode> Prepare(string friendlyName, Type[] modelTypes)
        {
            var visitor = new DomainTypesGraphVisitor(Domain, modelTypes);
            var models = visitor.Visit(null, null, null);

            _models[friendlyName] = modelTypes;
            ModelTypeNodes[friendlyName] = models;
            return models;
        }

        public ScoredTypeMapping CreateMappingsFor(ModelTypeNode source, IList<ModelTypeNode> candidateTargets)
        {
            var matcher = new ConceptMatchingRule(true);
            matcher.ComputeMappings(source, candidateTargets);
            return matcher.CandidateTypes.First();
        }


        public IEnumerable<TTarget> TransformDeserialize<TSource, TTarget>(string sourceTypeName,
            IList<ModelTypeNode> source, IList<ModelTypeNode> target,
            IEnumerable<TSource> sourceObjects)
        {
            var mapping = GetMappings(sourceTypeName, source, target);
            var settings = CreateSettings(mapping);

            var json = SerializePlain(sourceObjects);
            var clone = JsonSerializer.Deserialize<TSource[]>(json);
            var targetObjects = (IEnumerable<TTarget>)DeserializeByTransform(json, typeof(TTarget[]), settings);
            return targetObjects;
        }

        public IEnumerable<TTarget> TransformDeserialize<TTarget>(string sourceTypeName,
            IList<ModelTypeNode> source, IList<ModelTypeNode> target,
            IEnumerable<object> sourceObjects)
        {
            var mapping = GetMappings(sourceTypeName, source, target);
            var settings = CreateSettings(mapping);

            var json = SerializePlain(sourceObjects);
            var targetObjects = (IEnumerable<TTarget>)DeserializeByTransform(json, typeof(TTarget[]), settings);
            return targetObjects;
        }

        public IEnumerable<TTarget> TransformSerialize<TSource, TTarget>(string sourceTypeName,
            IList<ModelTypeNode> source, IList<ModelTypeNode> target,
            IEnumerable<TSource> sourceObjects)
        {
            var mapping = GetMappings(sourceTypeName, source, target);
            var settings = CreateSettings(mapping);

            var json = SerializeByTransform<TSource>(sourceObjects, settings);
            var targetObjects = (IEnumerable<TTarget>)DeserializePlain(json, typeof(TTarget[]));
            return targetObjects;
        }

        public ScoredTypeMapping GetMappings(string sourceTypeName, IList<ModelTypeNode> source, IList<ModelTypeNode> target)
        {
            var sourceType = source.First(t => t.Type.Name == sourceTypeName);  // i.e. "OnlineOrder"
            var mapping = CreateMappingsFor(sourceType, target);
            return mapping;
        }

    }
}
