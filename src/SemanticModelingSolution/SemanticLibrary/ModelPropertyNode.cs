using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SemanticLibrary.Helpers;

namespace SemanticLibrary
{
    public class ModelPropertyNode : IModelNode, IEqualityComparer<ModelPropertyNode>, IName
    {
        // default constructor is intended only for the serializer
        public ModelPropertyNode()
        {
        }

        internal ModelPropertyNode(ModelTypeNode parent, PropertyInfo propertyInfo,
            PropertyKind kind, Type coreType, IList<TermToConcept> termToConcepts)
        {
            Parent = parent;
            PropertyInfo = new(propertyInfo, parent.Type);
            PropertyKind = kind;
            CoreType = new(coreType);
            TermToConcepts = termToConcepts;
        }

        /// <summary>
        /// The type owning this property
        /// </summary>
        [JsonIgnore]
        public ModelTypeNode Parent { get; set; }

        /// <summary>
        /// The relevant metadata retrieved with reflection
        /// and copied here so that we can serialize them
        /// </summary>
        public SurrogatePropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// The type of the property extracted from the collection.
        /// If the property type is string, this is still string
        /// But if the property type is List of string, this is string
        /// </summary>
        public SurrogateType CoreType { get; set; }

        /// <summary>
        /// The navigation info for this property.
        /// The type is different from CoreType only for collections
        /// </summary>
        public ModelTypeNode NavigationNode { get; set; }

        /// <summary>
        /// Our property classification 
        /// </summary>
        public PropertyKind PropertyKind { get; set; }

        [JsonIgnore]
        public string Name => PropertyInfo?.Name;

        public IList<TermToConcept> TermToConcepts { get; set; }

        [JsonIgnore]
        public IEnumerable<Concept> CandidateConcepts => TermToConcepts.Select(c => c.Concept);
        
        [JsonIgnore]
        public IEnumerable<string> CandidateConceptNames => TermToConcepts.Select(c => c.Concept.Name);
        
        [JsonIgnore]
        public IEnumerable<string> CandidateConceptSpecifierNames => TermToConcepts.Select(c => c.ConceptSpecifier.Name);

        [JsonIgnore]
        private string UniqueString => $"{Parent?.Type.FullName}.{PropertyInfo?.Name}";

        public override bool Equals(object obj)
        {
            ModelPropertyNode item = obj as ModelPropertyNode;
            if (item == null) return false;

            return UniqueString.Equals(item.UniqueString);
        }

        public override int GetHashCode()
        {
            return UniqueString.GetHashCode();
        }

        public override string ToString()
        {
            var ttcs = string.Join(", ", TermToConcepts);
            return $"{ToStringNoConceptual()} <{ttcs}>";
        }

        public string ToStringNoConceptual()
        {
            return $"{Parent.Type.Name}.{Name} ({PropertyKind})";
        }

        public bool Equals(ModelPropertyNode x, ModelPropertyNode y)
        {
            return x.UniqueString.Equals(y.UniqueString);
        }

        public int GetHashCode(ModelPropertyNode obj)
        {
            return obj.UniqueString.GetHashCode();
        }
    }
}
