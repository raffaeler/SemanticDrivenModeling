using System;
using System.Collections.Generic;
using System.Text;

using SemanticLibrary;
using SemanticLibrary.Helpers;
using GeneratedCode;
using System.Linq;
using System.Diagnostics;
using Humanizer;

namespace ManualMapping
{
    public class SemanticAnalysis
    {
        private Domain _domain;
        public SemanticAnalysis()
        {
            _domain = new Domain();
        }

        public IList<TermToConcept> Analyze(string className)
        {
            var conceptsLinksClass = LexicalHelper.CamelPascalCaseExtract(className)
                .Select(t => ConceptsLinksSelector(t, KnownConcepts.Any))
                .ToList();
            return conceptsLinksClass;
        }

        public IList<TermToConcept> Analyze(IList<TermToConcept> classTermToConcepts, string propertyName, Type propertyType)
        {

            var mainConcepts = classTermToConcepts.Select(c => c.Concept);

            // the where condition removes the concepts that are equal to the class name
            // example: class named Address and property named AddressCity
            // the concept Address is removed from the property
            IList<TermToConcept> conceptsLinksProperty = LexicalHelper.CamelPascalCaseExtract(propertyName)
                .Select(t => ConceptsLinksSelector(t, mainConcepts))
                //.Where(ttc => !mainConcepts.Contains(ttc.Concept))
                .ToList();

            // If there are multiple terms pointing to the same concept
            // it is necessary to reduce the information because otherwise
            // the scoring system will be higher when we will start the next phase
            conceptsLinksProperty = FilterOutDuplicateConcepts(conceptsLinksProperty);

            return conceptsLinksProperty;
        }

        private IList<TermToConcept> FilterOutDuplicateConcepts(IEnumerable<TermToConcept> conceptsLinksProperty)
        {
            List<TermToConcept> result = new();
            var grouped = conceptsLinksProperty
                .GroupBy(g => g.Concept)
                .Select(g => g);

            foreach (var g in grouped)
            {
                var groupingConcept = g.Key;
                //var termToConcepts = g.ToList();
                TermToConcept selected = null;
                foreach (var item in g)
                {
                    if (selected == null || item.Weight > selected.Weight)
                    {
                        selected = item;
                        continue;
                    }

                    if(item.ConceptSpecifier != KnownConceptSpecifiers.None && selected.ConceptSpecifier == KnownConceptSpecifiers.None)
                    {
                        selected = item;
                        continue;
                    }
                }

                result.Add(selected);
            }

            return result;
        }

        private TermToConcept ConceptsLinksSelector(string term, IEnumerable<Concept> contexts)
        {
            var ttcs = contexts.Select(c => ConceptsLinksSelector(term, c)).ToArray();
            Debug.Assert(ttcs.Length != 0);
            if (ttcs.Length == 1)
            {
                return ttcs.Single();
            }

            return ttcs.OrderByDescending(t => t.Weight).First();
        }

        private TermToConcept ConceptsLinksSelector(string term, Concept context)
        {
            term = term.Singularize(false);
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

        private TermToConcept MakeUndefined(string term)
        {
            return new TermToConcept(KnownConcepts.Undefined, KnownConcepts.Any, KnownConceptSpecifiers.None,
                new Term(term, string.Empty, true), 100);
        }
    }
}
