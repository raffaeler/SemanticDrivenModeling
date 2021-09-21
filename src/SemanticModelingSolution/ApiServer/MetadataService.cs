using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Microsoft.Extensions.Options;

using SemanticLibrary;

namespace ApiServer
{
    public class MetadataService
    {
        private readonly MetadataConfiguration _metadataConfiguration;
        private DomainBase _domain;

        public MetadataService(IOptions<MetadataConfiguration> metadataOptions)
        {
            _metadataConfiguration = metadataOptions.Value;

            if(_metadataConfiguration.DomainDefinitionsFile == null)
                throw new ArgumentException(nameof(MetadataConfiguration.DomainDefinitionsFile));

            if(_metadataConfiguration.DomainTypesFileMap == null)
                throw new ArgumentException(nameof(MetadataConfiguration.DomainTypesFileMap));

            if (!File.Exists(_metadataConfiguration.DomainDefinitionsFile))
            {
                throw new System.Exception("The configuration does not contain a valid domain definition file for the domain");
            }

            foreach(var value in _metadataConfiguration.DomainTypesFileMap.Values)
            {
                if(!File.Exists(value))
                    throw new System.Exception("The configuration does not contain a valid domain definition file for the domain types");
            }
        }

        public DomainBase Domain
        {
            get
            {
                _domain ??= LoadDomain();
                return _domain;
            }
        }

        public IList<ModelTypeNode> ReadModelTypeNodes(string domainName)
        {
            if (!_metadataConfiguration.DomainTypesFileMap.TryGetValue(domainName, out string filename))
            {
                throw new Exception($"There is no entry in the configuration for {domainName}");
            }

            return ModelTypeNodeExtensions.DeserializeMany(filename, Domain);
        }

        private DomainBase LoadDomain()
        {
            try
            {
                var filename = _metadataConfiguration.DomainDefinitionsFile;
                var domain = JsonSerializer.Deserialize<DomainBase>(filename);
                return domain;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        private ScoredTypeMapping GetMappings(string sourceTypeName,
            IList<ModelTypeNode> source, IList<ModelTypeNode> target)
        {
            var sourceType = source.First(t => t.Type.Name == sourceTypeName);  // i.e. "OnlineOrder"
            var mapping = CreateMappingsFor(sourceType, target);
            return mapping;
        }

        private ScoredTypeMapping CreateMappingsFor(ModelTypeNode source, IList<ModelTypeNode> candidateTargets)
        {
            var matcher = new ConceptMatchingRule(true);
            matcher.ComputeMappings(source, candidateTargets);
            return matcher.CandidateTypes.First();
        }

    }
}
