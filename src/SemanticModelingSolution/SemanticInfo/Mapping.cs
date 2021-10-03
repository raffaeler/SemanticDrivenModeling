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

    }

}
