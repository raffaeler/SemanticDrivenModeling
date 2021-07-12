using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    //public record TermToConcept(Concept Concept, Concept ContextConcept, params (Term Term, int Weight)[] Impliers)
    public record TermToConcept(Concept Concept, Concept ContextConcept, ConceptSpecifier ConceptSpecifier, Term Term, int Weight)
    {
        public override string ToString()
        {
            return $"{ContextConcept.Name}:{Concept.Name}[{ConceptSpecifier.Name}] ({Term.Name})[{Weight}]";
        }
    }
}
