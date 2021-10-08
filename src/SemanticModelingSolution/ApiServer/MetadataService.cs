using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using MaterializerLibrary;

using Microsoft.Extensions.Options;

using SemanticLibrary;

using SurrogateLibrary;

namespace ApiServer
{
    public class MetadataService
    {
        private readonly MetadataConfiguration _metadataConfiguration;
        private List<TypeSystem<Metadata>> _typeSystems;

        public MetadataService(IOptions<MetadataConfiguration> configurationOptions)
        {
            _metadataConfiguration = configurationOptions.Value;

            if (_metadataConfiguration.DomainDefinitionsFile == null)
                throw new ArgumentException(nameof(MetadataConfiguration.DomainDefinitionsFile));

            if (_metadataConfiguration.TypeSystemFilenames == null ||
                _metadataConfiguration.TypeSystemFilenames.Any(t => !File.Exists(t)))
            {
                throw new Exception("One or more TypeSystem serialization files is missing");
            }

            if (_metadataConfiguration.MappingFilenames == null ||
                _metadataConfiguration.MappingFilenames.Any(m => !File.Exists(m)))
            {
                throw new Exception("One or more Mapping serialization files is missing");
            }

            var jsonDomain = File.ReadAllText(_metadataConfiguration.DomainDefinitionsFile);
            Domain = JsonSerializer.Deserialize<DomainBase>(jsonDomain);

            _typeSystems = new List<TypeSystem<Metadata>>();
            foreach (var file in _metadataConfiguration.TypeSystemFilenames)
            {
                var json = File.ReadAllText(file);
                var typeSystem = JsonSerializer.Deserialize<TypeSystem<Metadata>>(json);
                typeSystem.UpdateCache();
                _typeSystems.Add(typeSystem);
            }

            Mappings = new List<Mapping>();
            foreach (var file in _metadataConfiguration.MappingFilenames)
            {
                var json = File.ReadAllText(file);
                var mapping = JsonSerializer.Deserialize<Mapping>(json);
                mapping.UpdateCache(_typeSystems);
                Mappings.Add(mapping);
            }

            JsonDefaultOptions = new JsonSerializerOptions();
            JsonOptions = new JsonSerializerOptions()
            {
                Converters = { new SemanticConverterFactory(TypeSystems, Mappings) },
            };
        }

        private DomainBase Domain { get; }
        public IList<TypeSystem<Metadata>> TypeSystems => _typeSystems;
        public IList<Mapping> Mappings { get; }
        public JsonSerializerOptions JsonOptions { get; }
        public JsonSerializerOptions JsonDefaultOptions { get; }
        public System.Text.Json.Serialization.JsonConverter JsonConverterFactory => JsonOptions.Converters.First();

        public Mapping CreateAutoMapping(string mapIdentifier, 
            TypeSystem<Metadata> typeSystem1, TypeSystem<Metadata> typeSystem2,
            SurrogateType<Metadata> sourceRootType)
        {
            var matcher = new ConceptMatchingRule(typeSystem1, typeSystem2, false);
            var mappings = matcher.ComputeMappings(mapIdentifier, sourceRootType);
            return mappings.First();
        }



        //    public ScoredTypeMapping DeserializeMapping(DomainBase domain, string mappingFilename)
        //    {
        //        var jsonMapping = File.ReadAllText(mappingFilename);
        //        var mapping = ModelTypeNodeExtensions.DeserializeMapping(jsonMapping, domain);
        //        return mapping;
        //    }


        //    public DomainBase Domain
        //    {
        //        get
        //        {
        //            _domain ??= LoadDomain();
        //            return _domain;
        //        }
        //    }

        //    public IList<ModelTypeNode> ReadModelTypeNodes(string domainName)
        //    {
        //        if (!_metadataConfiguration.DomainTypes.TryGetValue(domainName, out string filename))
        //        {
        //            throw new Exception($"There is no entry in the configuration for {domainName}");
        //        }

        //        return ModelTypeNodeExtensions.DeserializeMany(filename, Domain);
        //    }

        //    private DomainBase LoadDomain()
        //    {
        //        try
        //        {
        //            var filename = _metadataConfiguration.DomainDefinitionsFile;
        //            var domain = JsonSerializer.Deserialize<DomainBase>(filename);
        //            return domain;
        //        }
        //        catch (System.Exception)
        //        {
        //            throw;
        //        }
        //    }

        //    private ScoredTypeMapping GetMappings(string sourceTypeName,
        //        IList<ModelTypeNode> source, IList<ModelTypeNode> target)
        //    {
        //        var sourceType = source.First(t => t.Type.Name == sourceTypeName);  // i.e. "OnlineOrder"
        //        var mapping = CreateMappingsFor(sourceType, target);
        //        return mapping;
        //    }

        //    private ScoredTypeMapping CreateMappingsFor(ModelTypeNode source, IList<ModelTypeNode> candidateTargets)
        //    {
        //        var matcher = new ConceptMatchingRule(true);
        //        matcher.ComputeMappings(source, candidateTargets);
        //        return matcher.CandidateTypes.First();
        //    }

    }
}
