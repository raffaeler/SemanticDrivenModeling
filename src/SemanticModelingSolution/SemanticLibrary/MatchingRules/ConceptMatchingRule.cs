using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary.Helpers;

namespace SemanticLibrary
{
    public class ConceptMatchingRule
    {
        public const int MinimumScoreForTypes = 50;
        private readonly bool _enableVerboseLogOnConsole;

        public ConceptMatchingRule(bool enableVerboseLogOnConsole)
        {
            _enableVerboseLogOnConsole = enableVerboseLogOnConsole;
        }

        public IReadOnlyCollection<ScoredTypeMapping> CandidateTypes { get; private set; }

        public void ComputeMappings(ModelTypeNode source, IEnumerable<ModelTypeNode> targets,
            int minimumScore = MinimumScoreForTypes)
        {
            var candidateTypes = new List<ScoredTypeMapping>(targets
                .Select(modelTypeNode => new ScoredTypeMapping(source, modelTypeNode, GetTypeScore(source, modelTypeNode)))
                .OrderByDescending(t => t.TypeScore)
                .Where(o => o.TypeScore > MinimumScoreForTypes));

            var flattenedSourceProperties = source.FlatHierarchyProperties().ToList();

            foreach (var candidateModelType in candidateTypes)
            {
                var flattenedTargetProperties = candidateModelType.TargetModelTypeNode.FlatHierarchyProperties();
                var matcher = new ScoredReservationLoop<ModelNavigationNode>(0, true);
                var mappings = matcher.GetScoredMappings(flattenedSourceProperties, flattenedTargetProperties,
                    GetPropertyScore, onSelectEquallyScored);

                candidateModelType.PropertyMappings = mappings;

                Console.WriteLine(DumpMappings(candidateModelType));
            }

            this.CandidateTypes = candidateTypes;
        }

        internal string DumpMappings(ScoredTypeMapping candidateModelType)
        {
            var sb = new StringBuilder();
            var source = candidateModelType.SourceModelTypeNode;
            var target = candidateModelType.TargetModelTypeNode;
            sb.Append($"Source type: {candidateModelType.SourceModelTypeNode.Type.Namespace}.{target.TypeName} => ");
            sb.Append($"Candidate type: {target.Type.Namespace}.{target.TypeName}");
            sb.AppendLine($" (Type score: {candidateModelType.TypeScore}) Prop score: {candidateModelType.PropertiesScore}");
            sb.AppendLine($"Mappings:");
            foreach (var map in candidateModelType.PropertyMappings)
            {
                var sourcePath = map.Source.GetMapPath();
                var targetPath = map.Target.GetMapPath();
                sb.Append($"{sourcePath} => {targetPath}");
                //sb.Append($"{map.Source.ModelPropertyNode.OwnerTypeName}.{map.Source.Name} => {map.Target.ModelPropertyNode.OwnerTypeName}.{map.Target.Name}");
                sb.AppendLine($" [{map.Score}]");
            }
            sb.AppendLine();
            return sb.ToString();
        }

        private ScoredPropertyMapping<ModelNavigationNode> onSelectEquallyScored(List<ScoredPropertyMapping<ModelNavigationNode>> mappings)
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

        private int GetPropertyScore(ModelNavigationNode source, ModelNavigationNode target)
        {
            var sourceContexts = source.ModelPropertyNode.Parent.CandidateConcepts;
            var targetContexts = target.ModelPropertyNode.Parent.CandidateConcepts;
            var isValidated = ValidateContexts(source, target);

            if (_enableVerboseLogOnConsole) VerboseLog(source, target, isValidated);
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

            if (isValidated) score = 1234;
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

        private int ComputeTotalWeight(int numSourceTerms, int numTargetTerms, int sourceWeight, int targetWeight, double increment)
        {
            double result = ((double)sourceWeight / numSourceTerms + (double)targetWeight / numTargetTerms) / 2.0;
            result += (result * increment);

            return (int)result;
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

        // all the concepts for the target segments must share something in common with the anything of the source entire path
        private bool ValidateContexts(ModelNavigationNode source, ModelNavigationNode target)
        {
            //if (source.Name == "Email") Debugger.Break();
            var sourceTtcs = source.GetAllConceptsFromPath().ToList();
            bool result = true;
            var tempTarget = target;
            while (tempTarget != null)
            {
                var matchesSource = false;
                foreach (var targetTtc in tempTarget.ModelPropertyNode.TermToConcepts)
                {
                    // verify whether any of the sources matches the current target
                    foreach (var sourceTtc in sourceTtcs)
                    {
                        matchesSource |= IsMatch(sourceTtc, targetTtc);

                        if (matchesSource) break;
                    }

                }

                result &= matchesSource;
                if (!result) break;

                tempTarget = tempTarget.Previous;
            }

            return result;
        }

        private bool IsMatch(TermToConcept sourceTtc, TermToConcept targetTtc)
        {
            // there is a match when
            // - the concept is the same but it is not Any or Undefined
            var conceptsMatch = sourceTtc.Concept == targetTtc.Concept &&
                sourceTtc.Concept != KnownBaseConcepts.Any &&
                sourceTtc.Concept != KnownBaseConcepts.Undefined;

            // there is a match when:
            // - the specifier is the same
            // - the destination specifier is "larger" (not specified ==> none)
            var specifiersMatch =
                (sourceTtc.ConceptSpecifier == targetTtc.ConceptSpecifier) ||
                (targetTtc.ConceptSpecifier == KnownBaseConceptSpecifiers.None);

            return conceptsMatch && specifiersMatch;
        }

        private bool ValidateContexts_old1(ModelNavigationNode source, ModelNavigationNode target)
        {
            bool result = true;
            var tempTarget = target;
            while (tempTarget != null)
            {
                var tempSource = source;
                while (result && tempSource != null)
                {
                    var intersection = source.ModelPropertyNode.Parent.CandidateConcepts
                        .Intersect(tempTarget.ModelPropertyNode.Parent.CandidateConcepts);
                    result &= intersection.Any();

                    tempSource = tempSource.Previous;
                }

                tempTarget = tempTarget.Previous;
            }

            return result;
        }

        private void VerboseLog(ModelNavigationNode source, ModelNavigationNode target, bool isValidated)
        {
            //var sourceContexts = source.Parent.CandidateConcepts;
            //var targetContexts = target.Parent.CandidateConcepts;

            //var srcctx = string.Join(",", sourceContexts.ToArray().Select(s => s.Name));
            //var tgtctx = string.Join(",", targetContexts.ToArray().Select(s => s.Name));

            //var props = $"{source.Parent.TypeName}[{srcctx}].{source.Property.Name} ==> {target.Parent.TypeName}[{tgtctx}].{target.Property.Name}";
            ////var ctx = $"{srcctx} ==> {tgtctx}";
            int padding = 60;
            var props = $"{source.ModelPropertyNode.ToStringNoConceptual().PadRight(padding - 5)} ==> {target.ModelPropertyNode.ToStringNoConceptual().PadRight(padding)}";

            var sourceConceptualPath = source.GetConceptualMapPath("Source: ", padding);
            var targetConceptualPath = target.GetConceptualMapPath("Target: ", padding);

            WriteEx("Map:    ", ConsoleColor.Blue);
            WriteEx(props);
            if (!isValidated)
                WriteLineEx($"Not validated", ConsoleColor.Red);
            else
                WriteLineEx($"Validated", ConsoleColor.Green);

            WriteLineEx(sourceConceptualPath);
            WriteLineEx(targetConceptualPath);
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
