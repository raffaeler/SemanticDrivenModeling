using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SurrogateLibrary;

namespace SemanticLibrary
{
    public static class NavigationSegmentExtensions
    {
        private const string _dotPathSeparator = ".";

        public static string GetObjectMapPath(this NavigationSegment<Metadata> segment)
            => segment.GetLeaf().Path;// PathAlt;


        public static string GetMapPath(this NavigationSegment<Metadata> segment, bool skipCurrentProperty = false)
        {
            var temp = segment.GetLeaf();
            if (skipCurrentProperty)
                temp = temp.Previous;
            if (temp == null) return string.Empty;
            return temp.Path;// PathAlt;
        }

        public static string GetConceptualMapPath(this NavigationSegment<Metadata> segment,
            string labelFirstLine, int firstPadding, string separator = "\r\n")
        {
            string spaces = "        ";
            var label = labelFirstLine.PadRight(spaces.Length);
            var temp = segment.GetLeaf();
            List<string> segments = new();
            while (temp != null)
            {
                var isProperty = temp.Property != null;
                var metadata = isProperty ? temp.Property.Info : temp.Type.Info;

                if(isProperty)
                {
                    var ownerType = temp.Property.OwnerType;
                    var ttcParent = $"Type:{string.Join(",", ownerType.Info.TermToConcepts)}".PadRight(firstPadding);
                    var ttcProperty = $"Property:{string.Join(",", temp.Property.Info.TermToConcepts)}";

                    var prefix = temp.Previous == null ? label : spaces;
                    segments.Insert(0, $"{prefix}{ttcParent}{ttcProperty}");
                }

                temp = temp.Previous;
            }

            return string.Join(separator, segments);
        }

        public static IEnumerable<TermToConcept> GetAllConceptsFromPath(this NavigationSegment<Metadata> segment)
        {
            var temp = segment;
            HashSet<string> uniqueness = new();
            while (temp != null)
            {
                if (temp.Property != null)
                {
                    foreach (var ttc in temp.Property.Info.TermToConcepts)
                    {
                        var unique = $"{ttc.Concept.Name}{ttc.ConceptSpecifier.Name}";
                        if (uniqueness.Contains(unique)) continue;
                        uniqueness.Add(unique);

                        yield return ttc;
                    }

                    foreach (var ttc in temp.Property.OwnerType.Info.TermToConcepts)
                    {
                        var unique = $"";
                        if (uniqueness.Contains(unique)) continue;
                        uniqueness.Add(unique);

                        yield return ttc;
                    }
                }

                temp = temp.Previous;
            }
        }

    }
}
