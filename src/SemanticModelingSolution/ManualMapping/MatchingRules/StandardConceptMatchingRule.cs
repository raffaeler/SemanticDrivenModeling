using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary;

namespace ManualMapping.MatchingRules
{
    public class StandardConceptMatchingRule : IConceptMatchingRule
    {
        public ModelTypeNode FindMatch(ModelTypeNode source, IEnumerable<ModelTypeNode> targets, IEnumerable<Concept> contexts = null)
        {
            var top = targets
                .Select(mtn => (mtn, GetScore(source, mtn, contexts)))
                .OrderByDescending(t => t.Item2)
                .First();

            return top.mtn;
        }

        public IList<(ModelTypeNode modelTypeNode, int score)> FindOrderedMatches(
            ModelTypeNode source, IEnumerable<ModelTypeNode> targets, IEnumerable<Concept> contexts = null)
        {
            var ordered = targets
                .Select(mtn => (mtn, GetScore(source, mtn, contexts)))
                .OrderByDescending(t => t.Item2)
                .ToList();

            return ordered;
        }

        private int GetScore(ModelTypeNode source, ModelTypeNode target, IEnumerable<Concept> contexts = null)
        {
            int score = 0;
            foreach (var sourceTtc in source.TermsToConcepts)
            {
                foreach (var targetTtc in target.TermsToConcepts)
                {
                    score += GetScore(sourceTtc, targetTtc);
                }
            }

            return score;
        }

        private int GetScore(TermsToConcept source, TermsToConcept target, IEnumerable<Concept> contexts = null)
        {
            if (source.Concept != target.Concept)
            {
                return 0;
            }

            double increment = 0.0;

            if(contexts != null)
            {
                if(contexts.Contains(source.ContextConcept))
                {
                    // context match, increase the match score
                    increment = 0.2;
                }
                else
                {
                    // context does not match, decrease the match score
                    increment = -0.10;
                }
            }

            // concept specifiers
            if(source.ConceptSpecifier.Name == target.Term.Name)
            {
                increment += 0.2;
            }

            return ComputeTotalWeight(source.Weight, target.Weight, increment);
        }

        private int ComputeTotalWeight(int sourceWeight, int targetWeight, double increment)
        {
            double result = (sourceWeight + targetWeight) / 2.0;
            result += (result * increment);

            return (int)result;
        }

        //for (int i = 0; i < source.TermsToConcepts.Count; i++)
        //{
        //    TermsToConcept ttc = source.TermsToConcepts[i];
        //    var scored = GetSC;
        //}

    }
}
