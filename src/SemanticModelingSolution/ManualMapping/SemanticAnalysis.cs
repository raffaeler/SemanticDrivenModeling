using System;
using System.Collections.Generic;
using System.Text;

using SemanticLibrary;
using SemanticLibrary.Helpers;
using GeneratedCode;
using System.Linq;
using System.Diagnostics;

namespace ManualMapping
{
    public class SemanticAnalysis
    {
        private Domain _domain;
        public SemanticAnalysis()
        {
            _domain = new Domain();
        }

        public IList<TermsToConcept> Analyze(string className)
        {
            var conceptsLinksClass = LexicalHelper.CamelPascalCaseExtract(className)
                .Select(t => ConceptsLinksSelector(t, KnownConcepts.Any))
                .ToList();
            return conceptsLinksClass;
        }

        public IList<TermsToConcept> Analyze(IList<TermsToConcept> classTermsToConcepts, string propertyName, Type propertyType)
        {

            var mainConcepts = classTermsToConcepts.Select(c => c.Concept);

            var conceptsLinksProperty = LexicalHelper.CamelPascalCaseExtract(propertyName)
                .Select(t=> ConceptsLinksSelector(t, mainConcepts))
                .ToList();

            return conceptsLinksProperty;
        }

        private TermsToConcept ConceptsLinksSelector(string term, IEnumerable<Concept> contexts)
        {
            var ttcs = contexts.Select(c => ConceptsLinksSelector(term, c)).ToArray();
            Debug.Assert(ttcs.Length != 0);
            if(ttcs.Length == 1)
            {
                return ttcs.Single();
            }

            return ttcs.OrderByDescending(t => t.Weight).First();
        }

        private TermsToConcept ConceptsLinksSelector(string term, Concept context)
        {
            var filtered = _domain.Links.Where(t => string.Compare(t.Term.Name, term, true) == 0).ToArray();
            if (filtered.Length == 0)
            {
                return MakeUndefined(term);
            }

            if (filtered.Length == 1)
            {
                return filtered.Single();
            }

            // at this point filtered.Length > 1

            var subfiltered = filtered.Where(t => t.ContextConcept == context).ToArray();
            if (subfiltered.Length == 0)
            {
                return filtered.OrderByDescending(s => s.Weight).First();
            }

            if (subfiltered.Length == 1)
            {
                return subfiltered.Single();
            }

            // at this pointt subfiltered.Length > 1
            return subfiltered.OrderByDescending(s => s.Weight).First();
        }

        private TermsToConcept MakeUndefined(string term)
        {
            return new TermsToConcept(KnownConcepts.Undefined, KnownConcepts.Any, KnownConceptSpecifiers.None,
                new Term(term, string.Empty, true), 100);
        }
    }
}
