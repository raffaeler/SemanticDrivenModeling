using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

using Humanizer;

using SemanticLibrary.Helpers;
using SurrogateLibrary;

namespace SemanticLibrary
{
    public class SemanticAnalysis
    {
        private DomainBase _domain;
        private List<string> _allComposedTerms;

        public SemanticAnalysis(DomainBase domain)
        {
            _domain = domain;

            _allComposedTerms = domain.Links
                .Select(ttc => ttc.Term.Name)
                .Where(t => t.ToCharArray().Where(c => char.IsUpper(c)).Count() > 1).ToList();
        }

        public void AssignSemantics(SurrogateType<Metadata> surrogateType)
        {
            SurrogateVisitor<Metadata> visitor = new();
            visitor.Visit(surrogateType,
                (type, _) => OnType(type), null,
                (property, _) => OnProperty(property), null, null);
        }

        private void OnType(SurrogateType<Metadata> type)
        {
            var classTermToConcepts = this.AnalyzeType(type.Name);
            Metadata metadata = new(classTermToConcepts);
            type.SetInfo(metadata);
        }

        private void OnProperty(SurrogateProperty<Metadata> property)
        {
            var type = property.OwnerType;
            var coreType = property.PropertyType.GetCoreType();
            var propertyTermToConcepts = this.AnalyzeProperty(type.Info.TermToConcepts, property.Name, coreType);
            Metadata metadata = new(propertyTermToConcepts);
            property.SetInfo(metadata);
        }


        /// <summary>
        /// Given a type name, it returns the list of its TermToConcept filtered appropriately
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public IList<TermToConcept> AnalyzeType(string className)
        {
            var conceptsLinksClass = LexicalHelper.CamelPascalCaseExtract(_allComposedTerms, className)
                .Select(t => TypeConceptsLinksSelector(t))
                .ToList();
            return conceptsLinksClass;
        }

        /// <summary>
        /// Given the list of classtoterm elements of a class and a property name/type
        /// it returns the list of its TermToConcept filtered appropriately
        /// </summary>
        /// <param name="classTermToConcepts"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        public IList<TermToConcept> AnalyzeProperty(IList<TermToConcept> classTermToConcepts,
            string propertyName, SurrogateType propertyType)
        {
            //var pureConcepts = GetPureConcepts(classTermToConcepts);
            var normalizedTermStrings = LexicalHelper.CamelPascalCaseExtract(_allComposedTerms, propertyName);

            List<TermToConcept> termToConcepts = new();
            foreach (var str in normalizedTermStrings)
            {
                // look for all the TermToConcept having the given (normalized) term
                var ttcs = _domain.Links.Where(t => string.Compare(t.Term.Name, str, true) == 0).ToList();
                if (ttcs.Count == 0) continue;
                if (ttcs.Count == 1) { termToConcepts.Add(ttcs[0]); continue; }

                // if there are more than 1, it means the same term is associated to more than one concept
                bool found = false;
                foreach (var ttc in ttcs)
                {
                    // look for the TermToConcept whose context matches with the TermToConcepts obtained from the class name
                    // for example the term Mid in a class named Employee (which is a Person concept) will match the Identity concept
                    var conceptMatchingContext = classTermToConcepts.FirstOrDefault(c => c.Concept == ttc.ContextConcept);
                    if (conceptMatchingContext != null)
                    {
                        found = true;
                        termToConcepts.Add(ttc);
                        break;
                    }
                }

                if (!found)
                {
                    // when there is no context concept associated with a term, we get the first "generic"
                    // generally this means the domain has an ambiguity
                    var selectedTtcs = ttcs.Where(t => t.ContextConcept == KnownBaseConcepts.Any).ToArray();
                    if (selectedTtcs.Length > 1)
                    {
                        var ambiguousTerms = string.Join(", ", ttcs);
                        Console.WriteLine($"Ambiguous Term {str} was discarged because it is specified multiple times in the domain: {ambiguousTerms}");
                        continue;
                    }

                    if (selectedTtcs.Length == 0)
                        termToConcepts.Add(ttcs.First());
                }
            }


            // Now we remove the redundant TermToConcept elements
            // For example a class named Address and a property named AddressCity
            // we want to remove the "Address" TermToConcept unless the property name is just Address
            // In this case we just get "City"
            termToConcepts = termToConcepts.Except(classTermToConcepts).ToList();
            if (termToConcepts.Count == 1) return termToConcepts;

            // Remove duplicate concepts (different Terms pointing to the same Concept)
            var finalTermToConcepts = FilterOutDuplicateConcepts(termToConcepts);
            //if (finalTermToConcepts.Count != termToConcepts.Count) Debugger.Break();
            return finalTermToConcepts;


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

        private List<TermToConcept> FilterOutDuplicateConcepts(IEnumerable<TermToConcept> conceptsLinksProperty)
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

                    if (item.ConceptSpecifier != KnownBaseConceptSpecifiers.None && selected.ConceptSpecifier == KnownBaseConceptSpecifiers.None)
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
            return new TermToConcept(KnownBaseConcepts.Undefined, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None,
                new Term(term, string.Empty, true), 100);
        }
    }
}
