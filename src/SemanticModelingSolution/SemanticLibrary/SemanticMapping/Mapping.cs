using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SurrogateLibrary;
using SurrogateLibrary.Helpers;

namespace SemanticLibrary
{
    public record Mapping(string SourceTypeSystemIdentifier, string TargetTypeSystemIdentifier,
        SurrogateType<Metadata> Source, SurrogateType<Metadata> Target, Evaluation Evaluation)
    {
        public ListEx<NavigationPair> Mappings { get; set; }
        public int PropertiesScore => Mappings.Select(m => m.Evaluation.Score).Sum();

        public void UpdateCache(params TypeSystem<Metadata>[] typeSystems)
        {
            UpdateCache(typeSystems);
        }

        public void UpdateCache(IReadOnlyCollection<TypeSystem<Metadata>> typeSystems)
        {
            var source = typeSystems.FirstOrDefault(t => t.Identifier == SourceTypeSystemIdentifier);
            var target = typeSystems.FirstOrDefault(t => t.Identifier == TargetTypeSystemIdentifier);
            if(source == null || target == null)
            {
                throw new ArgumentException("Cannot find the type system matching the identifier");
            }

            UpdateCacheInternal(source, target);
        }

        private void UpdateCacheInternal(TypeSystem<Metadata> sourceTypeSystem, TypeSystem<Metadata> targetTypeSystem)
        {
            Source.UpdateCache(sourceTypeSystem);
            Target.UpdateCache(targetTypeSystem);
            foreach (var mapping in Mappings)
            {
                mapping.Source.UpdateCache(sourceTypeSystem);
                mapping.Target.UpdateCache(targetTypeSystem);
            }
        }

    }

}
