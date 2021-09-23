using SemanticLibrary;

using System;
using System.Collections.Generic;
using System.IO;
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

        public virtual JsonSerializerOptions CreateSettings(ScoredTypeMapping scoredTypeMapping)
            => new JsonSerializerOptions()
            {
                //WriteIndented = true,
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

        public IEnumerable<TTarget> TransformDeserialize<TTarget>(ScoredTypeMapping mapping,
            IEnumerable<object> sourceObjects)
        {
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

        public IEnumerable<TTarget> TransformSerialize<TSource, TTarget>(ScoredTypeMapping mapping,
            IEnumerable<TSource> sourceObjects)
        {
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

        public ScoredTypeMapping CreateMappingsFor(ModelTypeNode source, IList<ModelTypeNode> candidateTargets)
        {
            var matcher = new ConceptMatchingRule(true);
            matcher.ComputeMappings(source, candidateTargets);
            return matcher.CandidateTypes.First();
        }

        /// <summary>
        /// Serialize the domain
        /// </summary>
        public void SerializeDomain(DomainBase domain, string filename)
        {
            var jsonDomainDefinitions = JsonSerializer.Serialize(domain);
            File.WriteAllText(filename, jsonDomainDefinitions);
        }

        /// <summary>
        /// Creates and serialize a list of ModelTypeModel, given the System.Type(s)
        /// </summary>
        public void SerializeTypeModel(DomainBase domain, Type[] types, string filename)
        {
            var modelsDomain = new DomainTypesGraphVisitor(domain, types).Visit(null, null, null);
            var jsonDomain = modelsDomain.Serialize(domain);
            File.WriteAllText(filename, jsonDomain);
        }

        /// <summary>
        /// Calculates the automatic mappings and serialize them on disk
        /// </summary>
        public void SerializeMapping(DomainBase domain, string sourceTypeName,
            IList<ModelTypeNode> source, IList<ModelTypeNode> target, string mappingFilename)
        {
            var utilities = new MappingUtilities(domain);

            var mapping = utilities.GetMappings(sourceTypeName, source, target);
            var jsonMapping = mapping.SerializeMapping(domain);
            //var mappingClone = ModelTypeNodeExtensions.DeserializeMapping(jsonMapping, domain);
            File.WriteAllText(mappingFilename, jsonMapping);
        }

        /// <summary>
        /// Deserialize a list of mappings
        /// </summary>
        public ScoredTypeMapping DeserializeMapping(DomainBase domain, string mappingFilename)
        {
            var jsonMapping = File.ReadAllText(mappingFilename);
            var mapping = ModelTypeNodeExtensions.DeserializeMapping(jsonMapping, domain);
            return mapping;
        }
    }
}
