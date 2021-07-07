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
        private DomainBase _domain;
        //private List<string> _allTerms;
        private List<string> _allComposedTerms;

        public SemanticAnalysis(DomainBase domain)
        {
            _domain = domain;
            //_allTerms = domain.Links
            //    .Select(ttc => ttc.Term.Name)
            //    .Distinct()
            //    .ToList();

            _allComposedTerms = domain.Links
                .Select(ttc => ttc.Term.Name)
                .Where(t => t.ToCharArray().Where(c => char.IsUpper(c)).Count() > 1).ToList();
        }

        public IList<TermToConcept> AnalyzeType(string className)
        {
            var conceptsLinksClass = LexicalHelper.CamelPascalCaseExtract(_allComposedTerms, className)
                .Select(t => TypeConceptsLinksSelector(t))
                .ToList();
            return conceptsLinksClass;
        }

        public IList<TermToConcept> AnalyzeProperty(IList<TermToConcept> classTermToConcepts, string propertyName, Type propertyType)
        {
            //var pureConcepts = GetPureConcepts(classTermToConcepts);
            var normalizedTermStrings = LexicalHelper.CamelPascalCaseExtract(_allComposedTerms, propertyName);
            IList<TermToConcept> termToConcepts = normalizedTermStrings
                .SelectMany(str => _domain.Links.Where(t => string.Compare(t.Term.Name, str, true) == 0))
                .ToList();

            // For example a class named Address and a property named AddressCity
            // we want to remove the "Address" TermToConcept unless the property name is just Address
            // In this case we just get "City"
            termToConcepts = termToConcepts.Except(classTermToConcepts).ToList();
            //var redundants = termToConcepts.Except(classTermToConcepts).ToList();
            //if (redundants.Count < termToConcepts.Count)
            //{
            //    foreach (var redundant in redundants)
            //    {
            //        termToConcepts.Remove(redundant);
            //    }
            //}

            if (termToConcepts.Count == 1) return termToConcepts;
            termToConcepts = FilterOutDuplicateConcepts(termToConcepts);
            // ConceptsLinksSelector
            return termToConcepts;


            //// the where condition removes the concepts that are equal to the class name
            //// example: class named Address and property named AddressCity
            //// the concept Address is removed from the property
            //IList<TermToConcept> conceptsLinksProperty = LexicalHelper.CamelPascalCaseExtract(_allComposedTerms, propertyName)
            //    .Select(t => ConceptsLinksSelector(t, mainConcepts))
            //    //.Where(ttc => !mainConcepts.Contains(ttc.Concept))
            //    .ToList();

            //// If there are multiple terms pointing to the same concept
            //// it is necessary to reduce the information because otherwise
            //// the scoring system will be higher when we will start the next phase
            //conceptsLinksProperty = FilterOutDuplicateConcepts(conceptsLinksProperty);

            //return conceptsLinksProperty;
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

        //private IList<TermToConcept> GetPureConcepts(IList<TermToConcept> classTermToConcepts)
        //{
        //    List<TermToConcept> result = new();
        //    foreach (var ttc in classTermToConcepts)
        //    {
        //        result.Add(_domain.Links
        //            .Single(l => 
        //                l.Concept == ttc.Concept &&
        //                l.Term.Name == ttc.Concept.Name &&
        //                l.ConceptSpecifier == KnownBaseConceptSpecifiers.None));
        //    }
        //    return result;
        //}

        private TermToConcept TypeConceptsLinksSelector(string term)
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

            return filtered.OrderByDescending(s => s.Weight).First();


            //term = term.Singularize(false);
            //var filtered = _domain.Links
            //    .Where(t => string.Compare(t.Term.Name, term, true) == 0 && t.ConceptSpecifier == KnownBaseConceptSpecifiers.None)
            //    .ToArray();
            //Debug.Assert(filtered.Length == 1);
            //return filtered.Single();
        }

        [Obsolete]
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
