using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary;

namespace ManualMapping.MatchingRules
{
    public record ConceptMatchScore(Concept Concept, int Score)
    {
    }
}
