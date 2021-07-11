using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SemanticLibrary.Helpers
{
    /// <summary>
    /// This helper class loops two sets of objects
    /// For each pair an external func is invoked to receive back a score
    /// At the end of the loop the highest pairs are selected so that every element of the first set 
    /// is assigned to the second element just once
    /// </summary>
    public class ScoredReservationLoop<T> where T : IEqualityComparer<T>, IName
    {
        private int _minimumScore;
        private bool _allowOneSourceToMultipleTargets;
        public ScoredReservationLoop(int minimumScore = 0, bool allowOneSourceToMultipleTargets = true)
        {
            _minimumScore = minimumScore;
            _allowOneSourceToMultipleTargets = allowOneSourceToMultipleTargets;
        }

        /// <summary>
        /// This method maps a source list to a target list
        /// The logic of the scoring is external (getScore lambda).
        /// 1. loop both lists and assign a score for each pair source-target
        ///   - if the score is below or equal to _minimumScore, the pair is discarded (no results are added to the collection)
        /// 2. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="getScore"></param>
        /// <param name="onSelectEquallyScored"></param>
        public IList<ScoredPropertyMapping<T>> GetScoredMappings(IEnumerable<T> source, IEnumerable<T> target,
            Func<T, T, int> getScore,
            Func<List<ScoredPropertyMapping<T>>, ScoredPropertyMapping<T>> onSelectEquallyScored)
        {
            var targetList = target.ToList();
            List<List<ScoredPropertyMapping<T>>> masterScoredList = new();
            List<ScoredPropertyMapping<T>> scoredList = new();

            // assign the score for all the permutations of the two lists
            // masterScoredList.Count will contain the number of the source elements
            // that received a score (potentially assigned to a target)
            foreach (var targetItem in targetList)
            {
                if (targetItem.Name == "ShipCity") Debugger.Break();
                foreach (var sourceItem in source)
                {
                    var score = getScore(sourceItem, targetItem);
                    if (score > _minimumScore)
                    {
                        scoredList.Add(new(sourceItem, targetItem, score));
                    }
                }

                if (scoredList.Count > 0)
                {
                    masterScoredList.Add(scoredList);
                    scoredList = new();
                }
            }

            // now we have all the classification (all over all)
            // We loop over the target items to get the source having the highests score
            // This way, we can never obtain the same target mapped multiple times
            // Instead it may happen that a single source property may be mapped to multiple targets
            // In general this is legit, but we may want to avoid it setting allowOneSourceToMultipleTargets to false

            List<ScoredPropertyMapping<T>> finalMap = new();
            HashSet<T> _excludedSources = new();
            foreach (var targetItem in targetList)
            {
                var mappings = masterScoredList
                    .SelectMany(m => m)
                    .Where(pm => pm.Target.Equals(targetItem) && !_excludedSources.Contains(pm.Source))
                    .OrderByDescending(pm => pm.Score);

                if (mappings.Any())
                {
                    var top = mappings.First();
                    if (top.Score == 0) { Debug.Fail("This should never happen as we don't have zero scored items"); continue; }

                    var topList = mappings.Where(pm => pm.Score == top.Score).ToList();

                    var selected = topList.Count > 1 ? (onSelectEquallyScored.Invoke(topList)) : top;

                    finalMap.Add(selected);
                    if (!_allowOneSourceToMultipleTargets)
                    {
                        _excludedSources.Add(selected.Source);
                    }
                }
            }

            return finalMap;
        }


    }

}
