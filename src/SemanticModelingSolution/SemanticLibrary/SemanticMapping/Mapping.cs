using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SurrogateLibrary;
using SurrogateLibrary.Helpers;

namespace SemanticLibrary
{
    /// <summary>
    /// This class maintain a map between a root source type to a root target type
    /// belonging to different models
    /// </summary>
    /// <param name="SourceTypeSystemIdentifier">The identifier of the type system where the Source type is defined</param>
    /// <param name="TargetTypeSystemIdentifier">The identifier of the type system where the Target type is defined</param>
    /// <param name="MapIdentifier">The identifier of this map. This is useful to distinguish different 
    /// versions of the same model. It is also used to choose a map from a Media Type specified in the
    /// content negotiation headers</param>
    /// <param name="Source">The root type for the source model</param>
    /// <param name="Target">The root type for the target model</param>
    /// <param name="Evaluation">The class describing the evaluation parameters</param>
    public record Mapping(string SourceTypeSystemIdentifier, string TargetTypeSystemIdentifier, string MapIdentifier,
        SurrogateType<Metadata> Source, SurrogateType<Metadata> Target, Evaluation Evaluation)
    {
        /// <summary>
        /// The list of pairs describing the map
        /// </summary>
        public ListEx<NavigationPair> Mappings { get; set; }

        /// <summary>
        /// The dynamically calculated score for all the properies
        /// </summary>
        public int PropertiesScore => Mappings.Select(m => m.Evaluation.Score).Sum();

        /// <summary>
        /// Update the references using the type systems. The updated references are
        /// the ones that are not sertialized. The type system is used to restore them
        /// </summary>
        /// <param name="typeSystems"></param>
        public void UpdateCache(params TypeSystem<Metadata>[] typeSystems)
        {
            UpdateCache(typeSystems);
        }

        /// <summary>
        /// See the description in the other overload
        /// </summary>
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
