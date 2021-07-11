using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticLibrary
{
    public class ModelTypeNode : IModelNode
    {
        public Type Type { get; set; }
        public string TypeName => Type?.Name;

        public IList<TermToConcept> TermToConcepts { get; set; }
        public IEnumerable<Concept> CandidateConcepts => TermToConcepts.Select(c => c.Concept);
        public IEnumerable<string> CandidateConceptNames => TermToConcepts.Select(c => c.Concept.Name);

        public IList<ModelPropertyNode> PropertyNodes { get; set; } = new List<ModelPropertyNode>();

        public override string ToString()
        {
            var concepts = string.Join(", ", CandidateConceptNames);
            return $"{TypeName} [{concepts}]";
        }
    }
}
