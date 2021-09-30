using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SurrogateLibrary;
using SurrogateLibrary.Helpers;

namespace SemanticLibrary
{
    public record Mapping(SurrogateType<Metadata> Source, SurrogateType<Metadata> Target,
        Evaluation Evaluation)
    {
        public ListEx<NavigationPair> Mappings { get; set; }
        public int PropertiesScore => Mappings.Select(m => m.Evaluation.Score).Sum();

        public void UpdateCache(TypeSystem<Metadata> sourceTypeSystem, TypeSystem<Metadata> targetTypeSystem)
        {
            Source.UpdateCache(sourceTypeSystem);
            Target.UpdateCache(targetTypeSystem);
            foreach (var mapping in Mappings)
            {
                mapping.Source.UpdateCache(sourceTypeSystem);
                mapping.Target.UpdateCache(targetTypeSystem);
            }
        }

        public (Dictionary<string, List<NavigationPair>> sourceLookup,
            Dictionary<string, NavigationPair> targetLookup) CreateLookups()
        {
            Dictionary<string, List<NavigationPair>> sourceLookup = new();
            Dictionary<string, NavigationPair> targetLookup = new();

            foreach (var propertyMapping in Mappings)
            {
                // _sourceLookup is needed in the Read to deserialize
                var sourcePath = propertyMapping.Source.GetMapPath();
                if (!sourceLookup.TryGetValue(sourcePath, out var listSource))
                {
                    listSource = new();
                    sourceLookup[sourcePath] = listSource;
                }

                listSource.Add(propertyMapping);

                // _targetLookup is needed in the Write to serialize
                // there is no list here because every target only has a single source
                var targetPath = propertyMapping.Target.GetMapPath();
                targetLookup[targetPath] = propertyMapping;
            }

            return (sourceLookup, targetLookup);
        }
    }

}
