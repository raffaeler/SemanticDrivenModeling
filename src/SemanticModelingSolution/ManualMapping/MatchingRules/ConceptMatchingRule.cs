using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary;
using SemanticLibrary.Helpers;

namespace ManualMapping.MatchingRules
{
    public class ConceptMatchingRule : IConceptMatchingRule
    {
        private const int _minimumScoreForTypes = 50;
        private readonly bool _enableVerboseLogOnConsole;

        public ConceptMatchingRule(bool enableVerboseLogOnConsole)
        {
            _enableVerboseLogOnConsole = enableVerboseLogOnConsole;
        }

        public ModelTypeNode FindMatch(ModelTypeNode source, IEnumerable<ModelTypeNode> targets, IEnumerable<Concept> contexts = null)
        {
            var top = FindOrderedMatches(source, targets, contexts).First();
            return top.modelTypeNode;
        }

        public IEnumerable<(ModelTypeNode modelTypeNode, int score)> FindOrderedMatches(
            ModelTypeNode source, IEnumerable<ModelTypeNode> targets, IEnumerable<Concept> contexts = null)
        {
            var ordered = targets
                .Select(modelTypeNode => (modelTypeNode, score: GetTypeScore(source, modelTypeNode)))
                .OrderByDescending(t => t.score);

            // evaluate properties foreach mapping that is worth

            Console.WriteLine($"The source type is {source.TypeName} whose properties are:");
            var flattenedSourceProperties = source.FlatHierarchyProperties().ToList();
            foreach (var p in flattenedSourceProperties)
                Console.WriteLine($"\t{p.ToString()}");
            Console.WriteLine();

            List<(ModelTypeNode, int)> propertiesScore = new();
            // all the types which I got before, but only over a given score
            foreach (var candidateModelType in ordered.Where(o => o.score > _minimumScoreForTypes))
            {
                var flattenedTargetProperties = candidateModelType.modelTypeNode.FlatHierarchyProperties();
                var matcher = new ScoredReservationLoop<ModelPropertyNode>();
                var mappings = matcher.GetScoredMappings(flattenedSourceProperties, flattenedTargetProperties,
                    GetPropertyScore, onSelectEquallyScored);

                var totalScore = mappings
                    .Select(m => m.Score)
                    .Sum();
                propertiesScore.Add((candidateModelType.modelTypeNode, totalScore));

                Console.WriteLine($"Candidate type: {candidateModelType.modelTypeNode.TypeName} (Type score: {candidateModelType.score}) Prop score: {totalScore}");
                Console.WriteLine($"Mappings:");
                foreach (var map in mappings)
                {
                    Console.WriteLine($"{map.Source.OwnerTypeName}.{map.Source.Name} => {map.Target.OwnerTypeName}.{map.Target.Name} [{map.Score}]");
                }
                Console.WriteLine();
            }

            var orderedPropertiesScope = propertiesScore.OrderByDescending(p => p.Item2).ToList();

            return ordered;
        }

        private ScoredMapping<ModelPropertyNode> onSelectEquallyScored(List<ScoredMapping<ModelPropertyNode>> mappings)
        {
            foreach (var mapping in mappings)
            {
                var sourceTerms = mapping.Source.TermToConcepts.Select(ttc => ttc.Term);
                var targetTerms = mapping.Target.TermToConcepts.Select(ttc => ttc.Term);

                var intersection = sourceTerms.Intersect(targetTerms);
                var firstMatch = intersection.FirstOrDefault();
                if (firstMatch != null)
                {
                    return mappings.First(m => m.Source.TermToConcepts.Select(ttc => ttc.Term).Contains(firstMatch));
                }
            }

            return mappings.First();
        }

        private int GetTypeScore(ModelTypeNode source, ModelTypeNode target)
        {
            int score = 0;
            foreach (var sourceTtc in source.TermToConcepts)
            {
                foreach (var targetTtc in target.TermToConcepts)
                {
                    score += GetTypeScore(sourceTtc, targetTtc);
                }
            }

            return score;
        }

        private int GetTypeScore(TermToConcept source, TermToConcept target)
        {
            // when matching a type, concepts must be the same
            if (source.Concept != target.Concept)
            {
                return 0;
            }

            int increment = 0;

            // even if the concept are the same, they still may need disambiguation
            // therefore, if the ConceptSpecifier of the target exists, it must match the one specified in the source
            // todo: consider avoiding the comparison if the source.ConceptSpecifier is == None
            // todo: consider changing increment instead of returning 0
            if (target.ConceptSpecifier != KnownBaseConceptSpecifiers.None && source.ConceptSpecifier != target.ConceptSpecifier)
            {
                // the typical case when enters here is:
                // source=Vendor, target=Customer
                // they are both companies but with totally different specifiers
                return 0;
            }

            var score = ComputeTotalWeight(source.Weight, target.Weight, increment);

            return score;
        }


        private int GetPropertyScore(ModelPropertyNode source, ModelPropertyNode target)
        {
            if (_enableVerboseLogOnConsole) VerboseLog(source, target);
            var sourceContexts = source.Parent.CandidateConcepts;
            var targetContexts = target.Parent.CandidateConcepts;
            int score = 0;
            foreach (var targetTtc in target.TermToConcepts)
            {
                foreach (var sourceTtc in source.TermToConcepts)
                {
                    //score += GetScore(false, sourceTtc, targetTtc, sourceContexts, targetContexts);
                    score += GetPropertyScore(sourceTtc, targetTtc, sourceContexts, targetContexts);
                }
            }

            return score;
        }

        private int GetPropertyScore(TermToConcept source, TermToConcept target,
            IEnumerable<Concept> sourceContexts = null, IEnumerable<Concept> targetContexts = null)
        {
            double increment = 0.0;
            bool matchingTopConcepts = target.Concept == source.Concept &&
                (source.Concept != KnownBaseConcepts.Any || source.Concept != KnownBaseConcepts.Undefined);

            bool matchingConceptContext = target.ContextConcept == source.Concept &&
                (source.Concept != KnownBaseConcepts.Any || source.Concept != KnownBaseConcepts.Undefined); //change weight

            bool matchingSpecifiers = target.ConceptSpecifier == source.ConceptSpecifier &&
                source.ConceptSpecifier != KnownBaseConceptSpecifiers.None;

            bool matchingTargetContext =
                (targetContexts != null && targetContexts.Contains(source.Concept)) ||
                (sourceContexts != null && sourceContexts.Contains(target.Concept));

            if (!(matchingTopConcepts || matchingTargetContext)) return 0;

            increment += matchingConceptContext ? 0.15 : 0;
            increment += matchingSpecifiers ? 0.4 : 0;

            var total = ComputeTotalWeight(source.Weight, target.Weight, increment);

            if (_enableVerboseLogOnConsole) VerboseLog(source, target, sourceContexts, targetContexts, increment, total,
                matchingTopConcepts, matchingConceptContext, matchingSpecifiers, matchingTargetContext);
            return total;
        }

        private int GetScore(bool matchRoot, TermToConcept source, TermToConcept target,
            IEnumerable<Concept> sourceContexts = null, IEnumerable<Concept> targetContexts = null)
        {
            if (matchRoot && source.Concept != target.Concept)
            {
                return 0;
            }

            double increment = 0.0;

            if (targetContexts != null)
            {
                //if (targetContexts.Contains(source.ContextConcept)
                //    || targetContexts.Contains(source.Concept)
                //    || sourceContexts.Intersect(targetContexts).Any()
                //    //|| contexts.Contains(source.Term)
                //    )
                if (source.Concept == target.Concept)
                {
                    // context match, increase the match score
                    increment = 0.2;
                }
                else
                {
                    // context does not match, decrease the match score
                    //increment = -0.20;
                    return 0;
                }
            }

            // concept specifiers
            if (source.ConceptSpecifier.Name == target.Term.Name)
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

        private void VerboseLog(ModelPropertyNode source, ModelPropertyNode target)
        {
            var sourceContexts = source.Parent.CandidateConcepts;
            var targetContexts = target.Parent.CandidateConcepts;

            var srcctx = string.Join(",", sourceContexts.ToArray().Select(s => s.Name));
            var tgtctx = string.Join(",", targetContexts.ToArray().Select(s => s.Name));

            var props = $"{source.Parent.TypeName}[{srcctx}].{source.Property.Name} <==> {target.Parent.TypeName}[{tgtctx}].{target.Property.Name}";
            //var ctx = $"{srcctx} <==> {tgtctx}";

            Console.WriteLine($"** Property:      {props}");
        }

        private void VerboseLog(TermToConcept source, TermToConcept target,
            IEnumerable<Concept> sourceContexts, IEnumerable<Concept> targetContexts,
            double increment, int total, bool matchingTopConcepts, bool matchingConceptContext,
            bool matchingSpecifiers, bool matchingTargetContext)
        {
            var srcctx = string.Join(",", sourceContexts.ToArray().Select(s => s.Name));
            var tgtctx = string.Join(",", targetContexts.ToArray().Select(s => s.Name));

            Console.WriteLine($" tot:{total} (inc:{increment})");
            Console.WriteLine($"   Terms:         {source.Term.Name.PadRight(20)} <==> {target.Term.Name}");

            var contextsContains =
                ((targetContexts != null && targetContexts.Contains(source.Concept)) ? $"{tgtctx}.Contains({source.Concept.Name}) " : "") +
                ((sourceContexts != null && sourceContexts.Contains(target.Concept)) ? $"{srcctx}.Contains({target.Concept.Name}) " : "");
            Console.WriteLine($"   Contexts:    {B2S(matchingTargetContext)} {contextsContains}");
            Console.WriteLine($"   Concepts:    {B2S(matchingTopConcepts)} {source.Concept.Name.PadRight(20)} <==> {target.Concept.Name}");
            Console.WriteLine($"   CContexts:   {B2S(matchingConceptContext)} {source.ContextConcept.Name.PadRight(20)} <==> {target.ContextConcept.Name}");
            Console.WriteLine($"   CSpecifiers: {B2S(matchingSpecifiers)} {source.ConceptSpecifier.Name.PadRight(20)} <==> {target.ConceptSpecifier.Name}");
            Console.WriteLine();
            // source.Term.Name
            // target.Term.Name
            // source.Concept.Name
            // target.Concept.Name
            // source.ContextConcept.Name
            // target.ContextConcept.Name
            // source.ConceptSpecifier.Name
            // target.ConceptSpecifier.Name
            // string.Join(",",sourceContexts.ToArray().Select(s => s.Name))
            // string.Join(",",targetContexts.ToArray().Select(s => s.Name))
            // increment
            // total

        }

        private string B2S(bool value) => value ? "Y" : " ";

    }
}
