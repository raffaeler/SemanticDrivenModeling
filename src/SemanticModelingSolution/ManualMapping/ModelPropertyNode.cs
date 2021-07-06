using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary;
using SemanticLibrary.Helpers;

namespace ManualMapping
{
    public class ModelPropertyNode : IModelNode, IEqualityComparer<ModelPropertyNode>, IName
    {
        private ModelTypeNode _navigationNode;
        public string OwnerTypeName => Parent.TypeName;
        public ModelTypeNode Parent { get; set; }
        public string Name => Property?.Name;
        public PropertyInfo Property { get; set; }
        public PropertyKind PropertyKind { get; set; }

        /// <summary>
        /// </summary>
        public ModelTypeNode NavigationNode
        {
            get => _navigationNode;
            set
            {
                _navigationNode = value;
                OnNavigationModeChanged();
            }
        }

        private void OnNavigationModeChanged()
        {
            // update FlattenedSubProperties
            if (_navigationNode == null) return;
        }

        /// <summary>
        /// This collection contains the list of propertis of the One-To-One 
        /// or One-To-Many relationships of the direct child type (CoreType)
        /// If the Proeprty type is a BasicType this is null
        /// It will be used to map the concepts between graphs. Thanks to the Parent it
        /// is then possible re-create the hierarachy when generating code
        /// </summary>
        public IList<ModelPropertyNode> FlattenedSubProperties => _navigationNode?.PropertyNodes;

        /// <summary>
        /// The type of the property extracted from the collection.
        /// If the property type is string, this is still string
        /// But if the property type is List of string, this is string
        /// </summary>
        public Type CoreType { get; set; }

        public IList<TermToConcept> TermToConcepts { get; set; }
        public IEnumerable<Concept> CandidateConcepts => TermToConcepts.Select(c => c.Concept);
        public IEnumerable<string> CandidateConceptNames => TermToConcepts.Select(c => c.Concept.Name);
        public IEnumerable<string> CandidateConceptSpecifierNames => TermToConcepts.Select(c => c.ConceptSpecifier.Name);

        private string UniqueString => $"{Parent?.Type?.FullName}.{Property?.Name}";

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
            var concepts = string.Join(", ", CandidateConceptNames);
            return $"{OwnerTypeName}.{Name} ({PropertyKind}) [{concepts}]";
        }

        public bool Equals(ModelPropertyNode x, ModelPropertyNode y)
        {
            return x.UniqueString.Equals(y.UniqueString);
        }

        public int GetHashCode([DisallowNull] ModelPropertyNode obj)
        {
            return obj.UniqueString.GetHashCode();
        }
    }
}
