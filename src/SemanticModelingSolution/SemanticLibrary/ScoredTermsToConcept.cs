using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    /// <summary>
    /// This class keeps both the TermToConcept and a score that is derived from the
    /// similarity between the orginal term and the one defined in the set of available terms
    /// 0 means no similarity
    /// 100 means the same exact word
    /// </summary>
    public record ScoredTermToConcept(TermToConcept TermToConcept, int MatchingScore)
    {
    }
}
