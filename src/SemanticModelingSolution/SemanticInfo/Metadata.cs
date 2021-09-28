﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SemanticLibrary
{
    public record Metadata
    {
        public Metadata()
        {
            TermToConcepts = new List<TermToConcept>();
        }

        [JsonConstructor]
        public Metadata(IList<TermToConcept> termToConcepts) =>
            TermToConcepts = termToConcepts;

        public IList<TermToConcept> TermToConcepts { get; init; }

        [JsonIgnore]
        public IEnumerable<Concept> CandidateConcepts => TermToConcepts.Select(c => c.Concept);

        [JsonIgnore]
        public IEnumerable<string> CandidateConceptNames => TermToConcepts.Select(c => c.Concept.Name);

        [JsonIgnore]
        public IEnumerable<string> CandidateConceptSpecifierNames => TermToConcepts.Select(c => c.ConceptSpecifier.Name);
    }
}
