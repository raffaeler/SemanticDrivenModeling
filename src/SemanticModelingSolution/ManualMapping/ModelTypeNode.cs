using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary;

namespace ManualMapping
{
    public class ModelTypeNode : IModelNode
    {
        public Type Type { get; set; }
        public string TypeName => Type.Name;

        public IList<TermsToConcept> TermsToConcepts { get; set; }
        public IEnumerable<Concept> CandidateConcepts => TermsToConcepts.Select(c => c.Concept);
        public IEnumerable<string> CandidateConceptNames => TermsToConcepts.Select(c => c.Concept.Name);

        public IList<ModelPropertyNode> PropertyNodes { get; set; } = new List<ModelPropertyNode>();
    }
}
