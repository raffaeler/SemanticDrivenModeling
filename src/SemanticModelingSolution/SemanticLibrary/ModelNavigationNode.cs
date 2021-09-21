using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary.Helpers;

namespace SemanticLibrary
{
    /// <summary>
    /// This class is used to preserve a specific "navigation" between a property and the type it points to
    /// We want to be able to navigate the graph back, not considering all the possible navigation but
    /// just the one we were drilling down into
    /// 
    /// A potential flaw is where a type (or its children) can navigate to a type passing by two different path
    /// In public models this is usually a remote possibility but we may want to address this hypthesis in future releases
    /// </summary>
    public class ModelNavigationNode : IEqualityComparer<ModelNavigationNode>, IName
    {
        private const string _dotPathSeparator = ".";

        public ModelNavigationNode()
        {
        }

        public ModelNavigationNode(ModelPropertyNode modelPropertyNode, ModelNavigationNode previous)
        {
            ModelPropertyNode = modelPropertyNode;
            Previous = previous;
        }

        public ModelPropertyNode ModelPropertyNode { get; set; }
        public ModelNavigationNode Previous { get; set; }
        public int Score { get; set; }
        public string Name => ModelPropertyNode.Name;

        public string GetObjectMapPath()
        {
            string separator = _dotPathSeparator;
            string arrayElementPlaceholder = "$";
            bool insertStartType = true;
            bool insertStartNamespace = false;

            var temp = this;
            List<string> segments = new();
            ModelTypeNode rootType = this.ModelPropertyNode.Parent;
            var skipCurrentProperty = ModelPropertyNode.PropertyKind != PropertyKind.OneToManyToDomain &&
                ModelPropertyNode.PropertyKind != PropertyKind.OneToManyToUnknown;

            if (skipCurrentProperty && temp != null)
            {
                temp = temp.Previous;
            }

            while (temp != null)
            {
                var propertyNode = temp.ModelPropertyNode;
                if (skipCurrentProperty &&
                    (temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToDomain ||
                    temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToUnknown))
                {
                    segments.Insert(0, arrayElementPlaceholder);
                }

                segments.Insert(0, propertyNode.Name);
                rootType = temp.ModelPropertyNode.Parent;
                temp = temp.Previous;
            }

            if (insertStartType)
            {
                if (insertStartNamespace)
                    segments.Insert(0, $"{rootType.Type.FullName}");
                else
                    segments.Insert(0, rootType.Type.Name);
            }

            return string.Join(separator, segments);
        }


        public string GetMapPath(string separator = _dotPathSeparator, bool skipCurrentProperty = false, 
            string arrayElementPlaceholder = "$", bool insertStartType = true, bool insertStartNamespace = false)
        {
            var temp = this;
            List<string> segments = new();
            ModelTypeNode rootType = this.ModelPropertyNode.Parent;
            if (skipCurrentProperty && temp != null)
            {
                temp = temp.Previous;
            }

            while (temp != null)
            {
                var propertyNode = temp.ModelPropertyNode;
                if(temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToDomain ||
                    temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToUnknown)
                {
                    segments.Insert(0, arrayElementPlaceholder);
                }

                segments.Insert(0, propertyNode.Name);
                rootType = temp.ModelPropertyNode.Parent;
                temp = temp.Previous;
            }

            if (insertStartType)
            {
                if (insertStartNamespace)
                    segments.Insert(0, $"{rootType.Type.FullName}");
                else
                    segments.Insert(0, rootType.Type.Name);
            }

            return string.Join(separator, segments);
        }

        //public List<(string, )> GetTypeMapPath()
        //{
        //    var temp = this;
        //    List<string> segments = new();
        //    while (temp != null)
        //    {
        //        var typeName = temp.ModelPropertyNode.Parent.Type.Name;
        //        segments.Insert(0, typeName);
        //        temp = temp.Previous;
        //    }

        //    return string.Join(".", segments);
        //}

        public string GetConceptualMapPath(string labelFirstLine, int firstPadding, string separator = "\r\n")
        {
            string spaces = "        ";
            var label = labelFirstLine.PadRight(spaces.Length);
            var temp = this;
            List<string> segments = new();
            while (temp != null)
            {
                var propertyNode = temp.ModelPropertyNode;

                var ttcParent = $"Type:{string.Join(",", propertyNode.Parent.TermToConcepts)}".PadRight(firstPadding);
                var ttcProperty = $"Property:{string.Join(",", propertyNode.TermToConcepts)}";

                var prefix = temp.Previous == null ? label : spaces;
                segments.Insert(0, $"{prefix}{ttcParent}{ttcProperty}");
                temp = temp.Previous;
            }

            return string.Join(separator, segments);
        }

        public IEnumerable<TermToConcept> GetAllConceptsFromPath()
        {
            var temp = this;
            HashSet<string> uniqueness = new();
            while (temp != null)
            {
                foreach (var ttc in temp.ModelPropertyNode.TermToConcepts)
                {
                    var unique = $"{ttc.Concept.Name}{ttc.ConceptSpecifier.Name}";
                    if (uniqueness.Contains(unique)) continue;
                    uniqueness.Add(unique);

                    yield return ttc;
                }

                foreach (var ttc in temp.ModelPropertyNode.Parent.TermToConcepts)
                {
                    var unique = $"";
                    if (uniqueness.Contains(unique)) continue;
                    uniqueness.Add(unique);

                    yield return ttc;
                }

                temp = temp.Previous;
            }
        }

        //public IList<TermToConcept> GetAllConceptsFromPath()
        //{
        //    var temp = this;
        //    List<TermToConcept> ttcs = new();
        //    while (temp != null)
        //    {
        //        ttcs.AddRange(temp.ModelPropertyNode.TermToConcepts);
        //        ttcs.AddRange(temp.ModelPropertyNode.Parent.TermToConcepts);
        //        temp = temp.Previous;
        //    }

        //    return ttcs;
        //}

        public bool Equals(ModelNavigationNode x, ModelNavigationNode y)
        {
            return x.ModelPropertyNode.Equals(y.ModelPropertyNode);
        }

        public int GetHashCode(ModelNavigationNode obj)
        {
            return obj.ModelPropertyNode.GetHashCode();
        }

        public override int GetHashCode()
        {
            return ModelPropertyNode.GetHashCode();
        }

        public override string ToString()
        {
            if (Previous == null)
                return $"{ModelPropertyNode.Parent.Type.FullName}";

            return $"{ModelPropertyNode.Parent.Type.FullName}.{ModelPropertyNode.Name} (from: {Previous?.ToString()})";
        }
    }
}
