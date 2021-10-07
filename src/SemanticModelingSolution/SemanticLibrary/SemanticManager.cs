using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using SurrogateLibrary;

namespace SemanticLibrary
{
    public class SemanticManager
    {
        public DomainBase DomainLoad(string filename)
        {
            var jsonDomainDefinitions = File.ReadAllText(filename);
            return JsonSerializer.Deserialize<DomainBase>(jsonDomainDefinitions);
        }

        public TypeSystem<Metadata> TypeSystemCreate(string identifier, params Type[] domainTypes)
        {
            var typeSystem = new TypeSystem<Metadata>(identifier);
            foreach (var domainType in domainTypes)
            {
                var surrogateType = typeSystem.GetOrCreate(domainType);
            }
            
            typeSystem.UpdateCache();
            return typeSystem;
        }

        public Mapping CreateAutoMapping(TypeSystem<Metadata> typeSystem1, TypeSystem<Metadata> typeSystem2,
            SurrogateType<Metadata> sourceRootType)
        {
            var matcher = new ConceptMatchingRule(typeSystem1, typeSystem2, true);
            var mappings = matcher.ComputeMappings(sourceRootType);
            return mappings.First();
        }
    }
}
