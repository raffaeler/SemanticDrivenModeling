using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    public record TermsToConcept(Concept Concept, params (Term Term, int Weight)[] Impliers)
    {
    }
}
