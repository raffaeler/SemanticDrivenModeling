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
                var matcher = new ScoredReservationLoop<ModelNavigationNode>();
                var mappings = matcher.GetScoredMappings(flattenedSourceProperties, flattenedTargetProperties,
                    GetPropertyScore, onSelectEquallyScored);

                var totalScore = mappings
                    .Select(m => m.Score)
                    .Sum();
                propertiesScore.Add((candidateModelType.modelTypeNode, totalScore));

                Console.Write($"Source type: {source.Type.Namespace}.{candidateModelType.modelTypeNode.TypeName} => ");
                Console.Write($"Candidate type: {candidateModelType.modelTypeNode.Type.Namespace}.{candidateModelType.modelTypeNode.TypeName}");
                Console.WriteLine($" (Type score: {candidateModelType.score}) Prop score: {totalScore}");
                Console.WriteLine($"Mappings:");
                foreach (var map in mappings)
                {
                    var sourcePath = GetPropertyMap(map.Source);
                    var targetPath = GetPropertyMap(map.Target);
                    Console.Write($"{sourcePath} => {targetPath}");
                    //Console.Write($"{map.Source.ModelPropertyNode.OwnerTypeName}.{map.Source.Name} => {map.Target.ModelPropertyNode.OwnerTypeName}.{map.Target.Name}");
                    Console.WriteLine($" [{map.Score}]");
                }
                Console.WriteLine();
            }

            var orderedPropertiesScope = propertiesScore.OrderByDescending(p => p.Item2).ToList();

            return ordered;
        }

        private string GetPropertyMap(ModelNavigationNode modelNavigationNode)
        {
            var temp = modelNavigationNode;
            List<string> segments = new();
            while (temp != null)
            {
                var propertyNode = temp.ModelPropertyNode;
                segments.Insert(0, propertyNode.Name);
                temp = temp.Previous;
            }

            return string.Join(".", segments);
        }

        private ScoredMapping<ModelNavigationNode> onSelectEquallyScored(List<ScoredMapping<ModelNavigationNode>> mappings)
        {
            foreach (var mapping in mappings)
            {
                var sourceTerms = mapping.Source.ModelPropertyNode.TermToConcepts.Select(ttc => ttc.Term);
                var targetTerms = mapping.Target.ModelPropertyNode.TermToConcepts.Select(ttc => ttc.Term);

                var intersection = sourceTerms.Intersect(targetTerms);
                var firstMatch = intersection.FirstOrDefault();
                if (firstMatch != null)
                {
                    return mappings.First(m => m.Source.ModelPropertyNode.TermToConcepts.Select(ttc => ttc.Term).Contains(firstMatch));
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
                    score += GetTypeScore(
                        source.TermToConcepts.Count,
                        target.TermToConcepts.Count,
                        sourceTtc, targetTtc);
                }
            }

            return score;
        }

        private int GetTypeScore(int numSourceTerms, int numTargetTerms, TermToConcept source, TermToConcept target)
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

            var score = ComputeTotalWeight(numSourceTerms, numTargetTerms, source.Weight, target.Weight, increment);

            return score;
        }

        private bool ValidateContexts(ModelNavigationNode source, ModelNavigationNode target)
        {
            bool result = true;
            var tempTarget = target;
            while (tempTarget != null)
            {
                var tempSource = source;
                while (tempSource != null)
                {
                    result &= source.ModelPropertyNode.Parent.CandidateConcepts
                        .Intersect(tempTarget.ModelPropertyNode.Parent.CandidateConcepts)
                        .Any();

                    tempSource = tempSource.Previous;
                }

                tempTarget = tempTarget.Previous;
            }

            return result;
        }

        private int GetPropertyScore(ModelNavigationNode source, ModelNavigationNode target)
        {
            var sourceContexts = source.ModelPropertyNode.Parent.CandidateConcepts;
            var targetContexts = target.ModelPropertyNode.Parent.CandidateConcepts;
            var isValidated = ValidateContexts(source, target);

            if (_enableVerboseLogOnConsole) VerboseLog(source.ModelPropertyNode, target.ModelPropertyNode, isValidated);
            int score = 0;
            foreach (var targetTtc in target.ModelPropertyNode.TermToConcepts)
            {
                foreach (var sourceTtc in source.ModelPropertyNode.TermToConcepts)
                {
                    //score += GetScore(false, sourceTtc, targetTtc, sourceContexts, targetContexts);
                    score += GetPropertyScore(
                        source.ModelPropertyNode.TermToConcepts.Count,
                        target.ModelPropertyNode.TermToConcepts.Count,
                        sourceTtc, targetTtc, sourceContexts, targetContexts);
                }
            }

            return score;
        }

        private int GetPropertyScore(int numSourceTerms, int numTargetTerms, TermToConcept source, TermToConcept target,
            IEnumerable<Concept> sourceContexts = null, IEnumerable<Concept> targetContexts = null)
        {
            double increment = 0.0;
            bool? matchingTopConcepts;
            if (source.Concept == KnownBaseConcepts.Any || source.Concept == KnownBaseConcepts.Undefined)
                matchingTopConcepts = null;
            else if (target.Concept == source.Concept)
                matchingTopConcepts = true;
            else
                matchingTopConcepts = false;

            bool? matchingConceptContext;
            if (target.ContextConcept == KnownBaseConcepts.Any || target.ContextConcept == KnownBaseConcepts.Undefined)
                matchingConceptContext = null;
            else if (target.ContextConcept == source.Concept)
                matchingConceptContext = true;
            else
                matchingConceptContext = false;

            if(matchingConceptContext.HasValue) increment += matchingConceptContext.Value ? 0.3 : -0.3;

            bool? matchingSpecifiers;
            if (source.ConceptSpecifier == KnownBaseConceptSpecifiers.None)
                matchingSpecifiers = null;
            else if (target.ConceptSpecifier == source.ConceptSpecifier)
                matchingSpecifiers = true;
            else
                matchingSpecifiers = false;

            if(matchingSpecifiers.HasValue) increment += matchingSpecifiers.Value ? 0.6 : -0.6;

            bool matchingTargetContext =
                (targetContexts != null && targetContexts.Contains(source.Concept)) ||
                (sourceContexts != null && sourceContexts.Contains(target.Concept));

            bool isFailed = false;
            if (matchingTopConcepts == false && matchingTargetContext == false) isFailed = true;
            if (source.Concept == KnownBaseConcepts.UniqueIdentity && target.Concept != source.Concept) isFailed = true;
            if (target.Concept == KnownBaseConcepts.UniqueIdentity && target.Concept != source.Concept) isFailed = true;

            var total = isFailed ? 0 : ComputeTotalWeight(numSourceTerms, numTargetTerms, source.Weight, target.Weight, increment);

            if (_enableVerboseLogOnConsole) VerboseLog(source, target, sourceContexts, targetContexts, increment, total,
                matchingTopConcepts, matchingConceptContext, matchingSpecifiers, matchingTargetContext);
            return total;
        }

        //private int GetScore(bool matchRoot, TermToConcept source, TermToConcept target,
        //    IEnumerable<Concept> sourceContexts = null, IEnumerable<Concept> targetContexts = null)
        //{
        //    if (matchRoot && source.Concept != target.Concept)
        //    {
        //        return 0;
        //    }

        //    double increment = 0.0;

        //    if (targetContexts != null)
        //    {
        //        //if (targetContexts.Contains(source.ContextConcept)
        //        //    || targetContexts.Contains(source.Concept)
        //        //    || sourceContexts.Intersect(targetContexts).Any()
        //        //    //|| contexts.Contains(source.Term)
        //        //    )
        //        if (source.Concept == target.Concept)
        //        {
        //            // context match, increase the match score
        //            increment = 0.2;
        //        }
        //        else
        //        {
        //            // context does not match, decrease the match score
        //            //increment = -0.20;
        //            return 0;
        //        }
        //    }

        //    // concept specifiers
        //    if (source.ConceptSpecifier.Name == target.Term.Name)
        //    {
        //        increment += 0.2;
        //    }

        //    return ComputeTotalWeight(source.Weight, target.Weight, increment);
        //}

        private int ComputeTotalWeight(int numSourceTerms, int numTargetTerms, int sourceWeight, int targetWeight, double increment)
        {
            double result = ((double)sourceWeight / numSourceTerms + (double)targetWeight / numTargetTerms) / 2.0;
            result += (result * increment);

            return (int)result;
        }

        private void VerboseLog(ModelPropertyNode source, ModelPropertyNode target, bool isValidated)
        {
            var sourceContexts = source.Parent.CandidateConcepts;
            var targetContexts = target.Parent.CandidateConcepts;

            var srcctx = string.Join(",", sourceContexts.ToArray().Select(s => s.Name));
            var tgtctx = string.Join(",", targetContexts.ToArray().Select(s => s.Name));

            var props = $"{source.Parent.TypeName}[{srcctx}].{source.Property.Name} ==> {target.Parent.TypeName}[{tgtctx}].{target.Property.Name}";
            //var ctx = $"{srcctx} ==> {tgtctx}";

            WriteEx($"** Property:      {props}");
            if (!isValidated)
                WriteLineEx($" No", ConsoleColor.Red);
            else
                WriteLineEx($" Y", ConsoleColor.Green);
        }

        private void VerboseLog(TermToConcept source, TermToConcept target,
            IEnumerable<Concept> sourceContexts, IEnumerable<Concept> targetContexts,
            double increment, int total, bool? matchingTopConcepts, bool? matchingConceptContext,
            bool? matchingSpecifiers, bool? matchingTargetContext)
        {
            var srcctx = string.Join(",", sourceContexts.ToArray().Select(s => s.Name));
            var tgtctx = string.Join(",", targetContexts.ToArray().Select(s => s.Name));

            WriteLineEx($" tot:{total} (inc:{increment})", total != 100 ? ConsoleColor.Green : ConsoleColor.White);
            WriteLineEx($"   Terms:         {source.Term.Name.PadRight(20)} ==> {target.Term.Name}");

            var contextsContains =
                ((targetContexts != null && targetContexts.Contains(source.Concept)) ? $"{tgtctx}.Contains({source.Concept.Name}) " : "") +
                ((sourceContexts != null && sourceContexts.Contains(target.Concept)) ? $"{srcctx}.Contains({target.Concept.Name}) " : "");
            WriteLineEx($"   Contexts:    {B2S(matchingTargetContext)} {contextsContains}");
            WriteLineEx($"   Concepts:    {B2S(matchingTopConcepts)} {source.Concept.Name.PadRight(20)} ==> {target.Concept.Name}");
            WriteLineEx($"   CContexts:   {B2S(matchingConceptContext)} {source.ContextConcept.Name.PadRight(20)} ==> {target.ContextConcept.Name}");
            WriteLineEx($"   CSpecifiers: {B2S(matchingSpecifiers)} {source.ConceptSpecifier.Name.PadRight(20)} ==> {target.ConceptSpecifier.Name}");
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

        private string B2S(bool? value)
        {
            if (!value.HasValue) return " ";
            if (value.Value) return "Y";
            return "N";
        }

        private void WriteLineEx(string line, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(line);
        }
        private void WriteEx(string line, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write(line);
        }

    }
}
