using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary;

namespace ManualMapping
{
    public class ModelPropertyNode : IModelNode
    {
        public string OwnerTypeName => Parent.TypeName;
        public ModelTypeNode Parent { get; set; }
        public string Name => Property.Name;
        public PropertyInfo Property { get; set; }
        public PropertyKind PropertyKind { get; set; }

        /// <summary>
        /// The type of the property extracted from the collection.
        /// If the property type is string, this is still string
        /// But if the property type is List of string, this is string
        /// </summary>
        public Type CoreType { get; set; }

        public IList<TermsToConcept> TermsToConcepts { get; set; }
        public IEnumerable<Concept> CandidateConcepts => TermsToConcepts.Select(c => c.Concept);
        public IEnumerable<string> CandidateConceptNames => TermsToConcepts.Select(c => c.Concept.Name);
    }
}
