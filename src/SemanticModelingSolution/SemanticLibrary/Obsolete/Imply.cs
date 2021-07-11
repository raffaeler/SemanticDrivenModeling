using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticLibrary
{
    /// <summary>
    /// The SourceConcept, when present, determines the context to map a term to a destination concept
    /// </summary>
    [Obsolete]
    public record Imply(Concept SourceConcept, Term Term, Concept ImpliedConcept, int Weight = 50)
    {
        public Imply(Term Term, Concept ImpliedConcept, int Weight = 50)
            : this(null, Term, ImpliedConcept, Weight)
        {
        }

    }
}
