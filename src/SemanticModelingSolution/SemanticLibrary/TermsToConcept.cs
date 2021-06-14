using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    //public record TermsToConcept(Concept Concept, Concept ContextConcept, params (Term Term, int Weight)[] Impliers)
    public record TermsToConcept(Concept Concept, Concept ContextConcept, ConceptSpecifier ConceptSpecifier, Term Term, int Weight)
    {
    }
}
