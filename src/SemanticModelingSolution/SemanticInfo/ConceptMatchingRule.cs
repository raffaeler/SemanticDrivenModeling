using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary.Helpers;

using SurrogateLibrary;

namespace SemanticLibrary
{
    public class ConceptMatchingRule
    {
        public const int MinimumScoreForTypes = 50;
        private readonly bool _enableVerboseLogOnConsole;
        private readonly bool _logOnlyValidated;
        private TypeSystem<Metadata> _sourceTypeSystem;
        private TypeSystem<Metadata> _targetTypeSystem;

        public ConceptMatchingRule(
            TypeSystem<Metadata> sourceTypeSystem,
            TypeSystem<Metadata> targetTypeSystem,
            bool enableVerboseLogOnConsole)
        {
            _sourceTypeSystem = sourceTypeSystem;
            _targetTypeSystem = targetTypeSystem;

            _enableVerboseLogOnConsole = enableVerboseLogOnConsole;
            _logOnlyValidated = false;
            //_logOnlyValidated = true;
        }

        public IList<Mapping> ComputeMappings(SurrogateType<Metadata> source,
            int minimumScore = MinimumScoreForTypes)
        {
            var targets = _targetTypeSystem.Types.Values.Where(t => !t.IsBasicType);

            var candidateTypes = targets
                .Where(t => t.Info != null)
                .Select(target => new Mapping(
                    source, target, new Evaluation(GetTypeScore(source, target))))
                .OrderByDescending(t => t.Evaluation.Score)
                .Where(o => o.Evaluation.Score > MinimumScoreForTypes)
                .ToList();

            var flattenedSource = GraphFlattener.FlattenHierarchy(source, _sourceTypeSystem);

            foreach (var candidate in candidateTypes)
            {
                var flattenedTarget = GraphFlattener.FlattenHierarchy(candidate.Target, _targetTypeSystem);

                var matcher = new AutomaticMapper(0, true);
                IList<NavigationPair> mappings = matcher.GetScoredMappings(
                    flattenedSource, flattenedTarget, GetPropertyScore, onSelectEquallyScored);

                candidate.Mappings = mappings;

                Console.WriteLine(DumpMappings(candidate));
            }

            return candidateTypes;
        }

        internal string DumpMappings(Mapping candidateModelType)
        {
            var sb = new StringBuilder();
            var source = candidateModelType.Source;
            var target = candidateModelType.Target;
            sb.Append($"Source type: {source.FullName} => ");
            sb.Append($"Candidate type: {target.FullName}");

            sb.AppendLine($" (Type score: {candidateModelType.Evaluation.Score}) Prop score: {candidateModelType.PropertiesScore}");
            sb.AppendLine($"Mappings:");
            foreach (var map in candidateModelType.Mappings)
            {
                var sourcePath = map.Source.GetMapPath();
                var targetPath = map.Target.GetMapPath();
                sb.Append($"{sourcePath} => {targetPath}");
                //sb.Append($"{map.Source.ModelPropertyNode.OwnerTypeName}.{map.Source.Name} => {map.Target.ModelPropertyNode.OwnerTypeName}.{map.Target.Name}");
                sb.AppendLine($" [{map.Evaluation.Score}]");
            }
            sb.AppendLine();
            return sb.ToString();
        }

        private NavigationPair onSelectEquallyScored(List<NavigationPair> mappings)
        {
            var firstTarget = mappings.First().Target;
            if (firstTarget.GetAllConceptsFromPath().Select(ttc => ttc.Concept).Contains(KnownBaseConcepts.UniqueIdentity))
            {
                // if this mappings refer to a uniqueIdentity, let's get the one with the same depth
                var targetDepth = GetDepth(firstTarget);
                var sources = mappings
                    .Select(m => (depth: GetDepth(m.Source), mapping: m))
                    .Where(t => t.depth <= targetDepth)
                    .OrderBy(t => t.depth);
                if (sources.Any()) return sources.First().mapping;
            }

            foreach (var mapping in mappings)
            {
                var sourceTerms = mapping.Source.Info.TermToConcepts.Select(ttc => ttc.Term);
                var targetTerms = mapping.Target.Info.TermToConcepts.Select(ttc => ttc.Term);

                var intersection = sourceTerms.Intersect(targetTerms);
                var firstMatch = intersection.FirstOrDefault();
                if (firstMatch != null)
                {
                    return mappings.First(m => m.Source.Info.TermToConcepts.Select(ttc => ttc.Term).Contains(firstMatch));
                }
            }

            return mappings.First();
        }

        private int GetDepth(NavigationSegment<Metadata> node)
        {
            int i = 0;
            while (node != null)
            {
                node = node.Previous;
                i++;
            }

            return i;
        }

        private int GetPropertyScore(NavigationSegment<Metadata> sourceRoot, NavigationSegment<Metadata> targetRoot)
        {
            var source = sourceRoot.GetLeaf();
            var target = targetRoot.GetLeaf();

            var sourceContexts = source.Property.GetOwnerType(_sourceTypeSystem).Info.CandidateConcepts;
            var targetContexts = target.Property.GetOwnerType(_sourceTypeSystem).Info.CandidateConcepts;
            var isValidated = ValidateContexts(source, target);

            if (_enableVerboseLogOnConsole) VerboseLog(source, target, isValidated);

            int score = 0;
            foreach (var targetTtc in target.Info.TermToConcepts)
            {
                foreach (var sourceTtc in source.Info.TermToConcepts)
                {
                    //score += GetScore(false, sourceTtc, targetTtc, sourceContexts, targetContexts);
                    score += GetPropertyScore(
                        source.Info.TermToConcepts.Count,
                        target.Info.TermToConcepts.Count,
                        sourceTtc, targetTtc, sourceContexts, targetContexts);
                }
            }

            if (score != 0)
            {
                List<Concept> src = new();
                List<Concept> tgt = new();
                src.AddRange(sourceContexts);
                src.AddRange(source.Info.TermToConcepts.Select(ttc => ttc.Concept));

                tgt.AddRange(targetContexts);
                tgt.AddRange(target.Info.TermToConcepts.Select(ttc => ttc.Concept));
                var intersectionCount = src.Intersect(tgt).Count();
                score += 30 * (intersectionCount / tgt.Count);
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

            if (matchingConceptContext.HasValue) increment += matchingConceptContext.Value ? 0.3 : -0.3;

            bool? matchingSpecifiers;
            if (matchingTopConcepts != true || source.ConceptSpecifier == KnownBaseConceptSpecifiers.None)
                matchingSpecifiers = null;
            else if (target.ConceptSpecifier == source.ConceptSpecifier)
                matchingSpecifiers = true;
            else
                matchingSpecifiers = false;

            if (matchingSpecifiers.HasValue) increment += matchingSpecifiers.Value ? 0.6 : -0.6;

            bool matchingTargetContext =
                (targetContexts != null && targetContexts.Contains(source.Concept)) ||
                (sourceContexts != null && sourceContexts.Contains(target.Concept));

            //if (matchingTargetContext) increment += 0.5;

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

        private int GetTypeScore(SurrogateType<Metadata> source, SurrogateType<Metadata> target)
        {
            int score = 0;
            foreach (var sourceTtc in source.Info.TermToConcepts)
            {
                foreach (var targetTtc in target.Info.TermToConcepts)
                {
                    score += GetTypeScore(
                        source.Info.TermToConcepts.Count,
                        target.Info.TermToConcepts.Count,
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
        private bool ValidateContexts(NavigationSegment<Metadata> sourceRoot, NavigationSegment<Metadata> targetRoot)
        {
            var source = sourceRoot.GetLeaf();
            var target = targetRoot.GetLeaf();

            // unique identity must be on both or nothing
            var ttcSourceUniqueIdentity = source.Info.TermToConcepts.Any(t => t.Concept == KnownBaseConcepts.UniqueIdentity);
            var ttcTargetUniqueIdentity = target.Info.TermToConcepts.Any(t => t.Concept == KnownBaseConcepts.UniqueIdentity);
            if (ttcSourceUniqueIdentity != ttcTargetUniqueIdentity)
            {
                return false;
            }

            //if (source.Name == "Email") Debugger.Break();
            // get the navigation path **including** the 1-1 or 1-many
            var sourceTtcs = source.GetAllConceptsFromPath().ToList();
            bool result = true;
            var tempTarget = target;

            // navigate the target path **without** the properties that are 1-1 or 1-many
            while (tempTarget != null)
            {
                var matchesSource = false;
                foreach (var targetTtc in tempTarget.Info.TermToConcepts)
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

        //private bool ValidateContexts_old1(ModelNavigationNode source, ModelNavigationNode target)
        //{
        //    bool result = true;
        //    var tempTarget = target;
        //    while (tempTarget != null)
        //    {
        //        var tempSource = source;
        //        while (result && tempSource != null)
        //        {
        //            var intersection = source.ModelPropertyNode.Parent.CandidateConcepts
        //                .Intersect(tempTarget.ModelPropertyNode.Parent.CandidateConcepts);
        //            result &= intersection.Any();

        //            tempSource = tempSource.Previous;
        //        }

        //        tempTarget = tempTarget.Previous;
        //    }

        //    return result;
        //}

        private void VerboseLog(NavigationSegment<Metadata> sourceRoot,
            NavigationSegment<Metadata> targetRoot, bool isValidated)
        {
            if (_logOnlyValidated && !isValidated) return;
            var source = sourceRoot.GetLeaf();
            var target = targetRoot.GetLeaf();

            //var sourceContexts = source.Parent.CandidateConcepts;
            //var targetContexts = target.Parent.CandidateConcepts;

            //var srcctx = string.Join(",", sourceContexts.ToArray().Select(s => s.Name));
            //var tgtctx = string.Join(",", targetContexts.ToArray().Select(s => s.Name));

            //var props = $"{source.Parent.TypeName}[{srcctx}].{source.Property.Name} ==> {target.Parent.TypeName}[{tgtctx}].{target.Property.Name}";
            ////var ctx = $"{srcctx} ==> {tgtctx}";
            int padding = 60;
            var props = $"{source.Property.ToString().PadRight(padding - 5)} ==> {target.Property.ToString().PadRight(padding)}";

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
