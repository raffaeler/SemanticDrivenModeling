using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SurrogateLibrary;

namespace SemanticLibrary
{
    /// <summary>
    /// Extension methods used in serialization and deserialization
    /// </summary>
    public static class MappingExtensions
    {
        public static (Dictionary<string, List<NavigationPair>> sourceLookup,
            Dictionary<string, NavigationPair> targetLookup) CreateLookups(this Mapping map)
        {
            Dictionary<string, List<NavigationPair>> sourceLookup = new();
            Dictionary<string, NavigationPair> targetLookup = new();

            foreach (var propertyMapping in map.Mappings)
            {
                // _sourceLookup is needed in the Read to deserialize
                var sourcePath = propertyMapping.Source.GetLeafPathAlt();
                if (!sourceLookup.TryGetValue(sourcePath, out var listSource))
                {
                    listSource = new();
                    sourceLookup[sourcePath] = listSource;
                }

                listSource.Add(propertyMapping);

                // _targetLookup is needed in the Write to serialize
                // there is no list here because every target only has a single source
                var targetPath = propertyMapping.Target.GetLeafPath();
                targetLookup[targetPath] = propertyMapping;
            }

            return (sourceLookup, targetLookup);
        }

        /// <summary>
        /// Creates a map used to remove the object from memory during deserialization
        /// This is used when there is a collection and the collected item must be
        /// removed from the cache so that a new collected item can be created
        /// The entire graph below the collected item must be removed.
        /// The returning dictionary contains:
        /// - key: the source path to the collected item
        /// - value: an hashset with the target paths that are used as keys in the
        ///          Instances dictionary to remove the corresponding IContainer object
        /// </summary>
        public static IDictionary<string, HashSet<string>> CreateDeletableLookup(this Mapping map)
        {
            Dictionary<string, HashSet<string>> result = new();

            foreach (var m in map.Mappings
                .Select(m => (mapping: m, source: m.Source.GetLeafPathAlt(), target: m.Target.GetLeafPath()))
                .OrderBy(m => m.source))
            {
                // retrieve all the "collected item" nodes from left to right
                var source = m.mapping.Source.GetRoot();
                List<NavigationSegment<Metadata>> sourceCollectedItems = new();
                source.OnAllAfter(item =>
                {
                    if (item.IsCollectedItem) sourceCollectedItems.Add(item);
                    return true;
                });

                // retrieve all the "collected item" nodes from left to right
                var target = m.mapping.Target.GetRoot();
                List<NavigationSegment<Metadata>> targetCollectedItems = new();
                target.OnAllAfter(item =>
                {
                    if (item.IsCollectedItem) targetCollectedItems.Add(item);
                    return true;
                });


                var j = 0;
                for (int i = 0; i < sourceCollectedItems.Count; i++)
                {
                    var sourceCollectedItem = sourceCollectedItems[i];
                    if (targetCollectedItems.Count <= j) break;

                    var key = sourceCollectedItem.PathAlt;
                    if (!result.TryGetValue(key, out HashSet<string> deletables))
                    {
                        deletables = new HashSet<string>();
                        result[key] = deletables;
                    }

                    var targetTemp = targetCollectedItems[j];
                    while (targetTemp != null && !targetTemp.IsLeaf)
                    {
                        deletables.Add(targetTemp.PathAlt);
                        targetTemp = targetTemp.Next;
                    }

                    j++;
                }
            }

            return result;
        }
    }
}
