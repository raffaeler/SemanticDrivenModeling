using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    public record TermToConcept
    {
        public TermToConcept()
        {
        }

        public TermToConcept(Concept concept, Concept contextConcept, ConceptSpecifier conceptSpecifier, Term term, int weight)
        {
            Concept = concept;
            ContextConcept = contextConcept;
            ConceptSpecifier = conceptSpecifier;
            Term = term;
            Weight = weight;
        }

        public Concept Concept { get; set; }
        public Concept ContextConcept { get; set; }
        public ConceptSpecifier ConceptSpecifier { get; set; }
        public Term Term { get; set; }
        public int Weight { get; set; }

        public override string ToString()
        {
            return $"{ContextConcept.Name}:{Concept.Name}[{ConceptSpecifier.Name}] ({Term.Name})[{Weight}]";
        }
    }
}
