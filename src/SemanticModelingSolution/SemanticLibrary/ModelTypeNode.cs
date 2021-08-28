using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary.Helpers;

namespace SemanticLibrary
{
    public class ModelTypeNode : IModelNode
    {
        // default constructor is intended only for the serializer
        public ModelTypeNode()
        {
        }

        internal ModelTypeNode(Type type, IList<TermToConcept> termToConcepts)
        {
            Type = new(type);
            TermToConcepts = termToConcepts;
        }

        public SurrogateType Type { get; set; }

        public IList<TermToConcept> TermToConcepts { get; set; }
        public IEnumerable<Concept> CandidateConcepts => TermToConcepts.Select(c => c.Concept);
        public IEnumerable<string> CandidateConceptNames => TermToConcepts.Select(c => c.Concept.Name);

        public IList<ModelPropertyNode> PropertyNodes { get; set; } = new List<ModelPropertyNode>();

        public override string ToString()
        {
            var concepts = string.Join(", ", CandidateConceptNames);
            return $"{Type.Name} [{concepts}]";
        }
    }
}
