using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using SurrogateLibrary;
using SurrogateLibrary.Helpers;

namespace SemanticLibrary
{
    /// <summary>
    /// This helper class loops two sets of objects
    /// For each pair an external func is invoked to receive back a score
    /// At the end of the loop the highest pairs are selected so that every element of the first set 
    /// is assigned to the second element just once
    /// </summary>
    public class AutomaticMapper
    {
        private int _minimumScore;
        private bool _allowOneSourceToMultipleTargets;

        public AutomaticMapper(int minimumScore = 0, bool allowOneSourceToMultipleTargets = true)
        {
            _minimumScore = minimumScore;
            _allowOneSourceToMultipleTargets = allowOneSourceToMultipleTargets;
        }

        /// <summary>
        /// This method returns the top scored property mappings between source and target
        /// The logic of the scoring is external (getScore lambda).
        /// A minimum threshold score prevents fragile mappings.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="getScore">The scoring function</param>
        /// <param name="onSelectEquallyScored">The function that resolves the ambiguities when multiple properties has the same score</param>
        public ListEx<NavigationPair> GetScoredMappings(
            IEnumerable<NavigationSegment<Metadata>> source,
            IEnumerable<NavigationSegment<Metadata>> target,
            Func<NavigationSegment<Metadata>, NavigationSegment<Metadata>, int> getScore,
            Func<List<NavigationPair>, NavigationPair> onSelectEquallyScored)
        {
            var targetList = target.ToList();
            List<List<NavigationPair>> masterScoredList = new();
            List<NavigationPair> scoredList = new();

            // assign the score for all the permutations of the two lists
            // masterScoredList.Count will contain the number of the source elements
            // that received a score (potentially assigned to a target)
            foreach (var targetItem in targetList)
            {
                foreach (var sourceItem in source)
                {
                    var score = getScore(sourceItem, targetItem);
                    if (score > _minimumScore)
                    {
                        scoredList.Add(new(sourceItem, targetItem, new Evaluation(score)));
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

            ListEx<NavigationPair> finalMap = new();
            HashSet<NavigationSegment<Metadata>> _excludedSources = new();
            foreach (var targetItem in targetList)
            {
                var mappings = masterScoredList
                    .SelectMany(m => m)
                    .Where(pm => pm.Target.Equals(targetItem) && !_excludedSources.Contains(pm.Source))
                    .OrderByDescending(pm => pm.Evaluation.Score);

                if (mappings.Any())
                {
                    var top = mappings.First();
                    if (top.Evaluation.Score == 0)
                    {
                        Debug.Fail("This should never happen as we don't have zero scored items");
                        continue;
                    }

                    var topList = mappings.Where(pm => pm.Evaluation.Score == top.Evaluation.Score).ToList();

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
