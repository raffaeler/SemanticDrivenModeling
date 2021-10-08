using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using SemanticGlossaryGenerator.Helpers;

namespace SemanticGlossaryGenerator
{
    public class CoreGenerator
    {
        public static readonly string Namespace = "GeneratedCode";

        public static readonly string GeneratdTermsClassName = "KnownTerms";
        public static readonly string GeneratedConceptsClassName = "KnownConcepts";
        public static readonly string GeneratedConceptSpecifiersClassName = "KnownConceptSpecifiers";
        public static readonly string GeneratedDomainClassName = "Domain";

        public static readonly string BaseTermsClassName = "KnownBaseTerms";
        public static readonly string BasedConceptsClassName = "KnownBaseConcepts";
        public static readonly string BasedConceptSpecifiersClassName = "KnownBaseConceptSpecifiers";
        public static readonly string BasedDomainClassName = "DomainBase";

        public static readonly string TermClassName = "Term";
        public static readonly string ConceptClassName = "Concept";
        public static readonly string ConceptSpecifierClassName = "ConceptSpecifier";
        public static readonly string TermToConceptClassName = "TermToConcept";
        public static readonly string TermToConceptPropertyName = "Links";
        public static readonly string TermToConceptPropertyComment = "The relationships between terms and a concept";

        public static readonly string NoneConceptSpecifier = "None";


        public CoreGenerator()
        {
            Terms = new ClassGenerator(Namespace, GeneratdTermsClassName);
            Concepts = new ClassGenerator(Namespace, GeneratedConceptsClassName);
            ConceptSpecifiers = new ClassGenerator(Namespace, GeneratedConceptSpecifiersClassName);
            Domain = new ClassGenerator(Namespace, GeneratedDomainClassName);

            Terms.BaseClass = BaseTermsClassName;
            Concepts.BaseClass = BasedConceptsClassName;
            ConceptSpecifiers.BaseClass = BasedConceptSpecifiersClassName;
            Domain.BaseClass = BasedDomainClassName;

            Terms.Usings.Add("System");
            Terms.Usings.Add("System.Collections.Generic");
            Terms.Usings.Add("SemanticLibrary");

            Concepts.Usings.Add("System");
            Concepts.Usings.Add("System.Collections.Generic");
            Concepts.Usings.Add("SemanticLibrary");

            ConceptSpecifiers.Usings.Add("System");
            ConceptSpecifiers.Usings.Add("System.Collections.Generic");
            ConceptSpecifiers.Usings.Add("SemanticLibrary");

            // moved into the base class
            //ConceptSpecifiers.Members.Add(ConceptSpecifiers.CreateStaticField(
            //    new string[] { "Default specifier when the term does not add any further information" },
            //    ConceptSpecifierClassName,
            //    NoneConceptSpecifier,
            //    ConceptSpecifiers.CreateInitializersWithStrings(ConceptSpecifierClassName, NoneConceptSpecifier)));


            Domain.Usings.Add("System");
            Domain.Usings.Add("System.Collections.Generic");
            Domain.Usings.Add("SemanticLibrary");

            //Domain.Members.Add(Domain.CreatePropertyWithInitializer(
            //    new[] { TermToConceptPropertyComment, },
            //    Domain.MakeListOfT(TermToConceptClassName), TermToConceptPropertyName,
            //    Domain.CreateInitializerWithCollection(TermToConceptClassName), true));
            Domain.Members.Add(Domain.CreatePropertyWithArrowCallingBase(
                new[] { TermToConceptPropertyComment, },
                Domain.MakeListOfT(TermToConceptClassName), TermToConceptPropertyName, true));
        }

        public ClassGenerator Terms { get; }
        public ClassGenerator Concepts { get; }
        public ClassGenerator ConceptSpecifiers { get; }
        public ClassGenerator Domain { get; }

        public void ProcessFile(FileInfo fileInfo, Action<string, string[]> onError)
        {
            HashSet<string> conceptsUniqueness = new HashSet<string>();
            HashSet<string> specifiersUniqueness = new HashSet<string>();
            HashSet<string> termsUniqueness = new HashSet<string>();
            HashSet<string> contexts = new HashSet<string>();
            Dictionary<string, string> assignments = new();

            List<ExpressionSyntax> links = new List<ExpressionSyntax>();
            var parser = new SimplifiedRelationshipsParser(
                (List<string> words, string description, List<string> comments, List<List<string>> aliasesList) =>
                {
                    int termsIndex = 1;
                    if (!AreValidWords("Concept", conceptsUniqueness, words, onError))
                    {
                        return;
                    }

                    if (!AreValidWords("Term", termsUniqueness, aliasesList
                                                .Where(a => a.Count > termsIndex)
                                                .Select(a => a[termsIndex]),
                                                onError))
                    {
                        return;
                    }

                    var mainConcept = words.First();
                    if (!termsUniqueness.Contains(mainConcept))
                    {
                        Terms.Members.Add(Terms.CreateStaticField(null, TermClassName, mainConcept,
                            Terms.CreateInitializersWithStrings(TermClassName, mainConcept)));

                        termsUniqueness.Add(mainConcept);
                    }

                    List<string> specifiers = new();
                    specifiers.Add(NoneConceptSpecifier);

                    foreach (var aliases in aliasesList)
                    {
                        var contextConcept = GetFromAlias(aliases, 0, "Any");
                        var termAndSpecifier = GetFromAlias(aliases, termsIndex, string.Empty);
                        var (term, specifier, isValid) = SplitTermAndSpecifier(termAndSpecifier);
                        var weight = GetFromAlias(aliases, 2, 100);
                        var termDescription = GetFromAlias(aliases, 3, string.Empty);
                        if (!isValid)
                        {
                            onError("SM04", new string[] { termAndSpecifier });
                            continue;
                        }

                        if (contextConcept != string.Empty)
                        {
                            contexts.Add(contextConcept);
                        }

                        if (!termsUniqueness.Contains(term))
                        {
                            Terms.Members.Add(Terms.CreateStaticField(new[] { termDescription }, TermClassName, term,
                                Terms.CreateInitializersWithStrings(TermClassName, term, termDescription)));
                            termsUniqueness.Add(term);
                        }

                        if (!string.IsNullOrEmpty(specifier))
                        {
                            specifiers.Add(specifier);
                        }
                        else
                        {
                            specifier = NoneConceptSpecifier;
                        }

                        links.Add(Domain.CreateCreateObject(TermToConceptClassName,
                            Domain.CreateMemberAccess2(GeneratedConceptsClassName, mainConcept),
                            Domain.CreateMemberAccess2(GeneratedConceptsClassName, contextConcept),
                            Domain.CreateMemberAccess2(GeneratedConceptSpecifiersClassName, specifier),
                            Domain.CreateMemberAccess2(GeneratdTermsClassName, term),
                            Domain.CreateNumericLiteralExpression(weight)));

                        //links.Add(Domain.CreateCreateObject(TermToConceptClassName,
                        //    Domain.CreateMemberAccess2(GeneratedConceptsClassName, mainConcept),
                        //    Domain.CreateMemberAccess2(GeneratedConceptsClassName, contextConcept),
                        //    Domain.CreateTuple(Domain.CreateMemberAccess2(GeneratdTermsClassName, term), Domain.CreateNumericLiteralExpression(weight))));
                    }

                    // the term of the concept is linked to its same concept
                    links.Add(Domain.CreateCreateObject(TermToConceptClassName,
                        Domain.CreateMemberAccess2(GeneratedConceptsClassName, mainConcept),
                        Domain.CreateMemberAccess2(GeneratedConceptsClassName, "Any"),   // no context here
                        Domain.CreateMemberAccess2(GeneratedConceptSpecifiersClassName, NoneConceptSpecifier),
                        Domain.CreateMemberAccess2(GeneratdTermsClassName, mainConcept),
                        Domain.CreateNumericLiteralExpression(100)));


                    List<ExpressionSyntax> expressions = new();
                    expressions.Add(Concepts.CreateStringLiteralExpression(mainConcept));
                    expressions.Add(Concepts.CreateStringLiteralExpression(description));
                    foreach (var specifier in specifiers.Distinct())
                    {
                        if (!specifiersUniqueness.Contains(specifier))
                        {
                            ConceptSpecifiers.Members.Add(ConceptSpecifiers.CreateStaticField(
                                Array.Empty<string>(),
                                ConceptSpecifierClassName,
                                specifier,
                                ConceptSpecifiers.CreateInitializersWithStrings(ConceptSpecifierClassName, specifier)));
                            specifiersUniqueness.Add(specifier);
                        }

                        expressions.Add(Concepts.CreateMemberAccess2(GeneratedConceptSpecifiersClassName, specifier));
                    }

                    Concepts.Members.Add(Concepts.CreateStaticField(comments?.ToArray(), ConceptClassName, mainConcept,
                        Concepts.CreateInitializersWithExpressions(ConceptClassName, expressions.ToArray())));
                },
                (name, value) => assignments[name] = value);

            using var file = fileInfo.OpenText();
            string line;
            while ((line = file.ReadLine()) != null)
            {
                parser.Feed(line);
            }

            parser.Feed(null);

            foreach (var ctx in contexts)
            {
                if (!conceptsUniqueness.Contains(ctx))
                {
                    onError("SM03", new string[] { ctx });
                }
            }

            List<StatementSyntax> statements = new();
            foreach(var assignment in assignments)
            {
                // Warning: we are assuming the variable specified in assignment.Key
                // is already declared (like Name in DomainBase)
                // Anything else should be declared in the derived class being created
                statements.Add(Domain.CreateSimpleAssignment(assignment.Key, assignment.Value));
            }

            statements.AddRange(links
                .Select(e => Domain.CreateAddCollection(TermToConceptPropertyName, e)));

            Domain.Members.Add(Domain.CreateConstructor(Array.Empty<string>(), statements.ToArray()));
        }

        private (string term, string specifier, bool isValid) SplitTermAndSpecifier(string input)
        {
            var start = input.IndexOf('[');
            var end = input.IndexOf(']');
            if (start == -1 && end == -1) return (input, string.Empty, true);
            if (start == -1 || end == -1) return (input, string.Empty, false);

            return (input.Substring(0, start), input.Substring(start + 1, end - start - 1), true);
        }

        private T GetFromAlias<T>(List<String> aliases, int index, T defaultValue)
        {
            if (aliases.Count <= index) return defaultValue;
            var str = aliases[index];
            if (string.IsNullOrEmpty(str)) return defaultValue;

            if (typeof(T) == typeof(string))
            {
                return (T)(object)str;
            }

            if (typeof(T) == typeof(int) && int.TryParse(str, out int value))
            {

                return (T)(object)value;
            }

            throw new ArgumentException($"Can't convert '{str}' to '{typeof(T).Name}'");
        }

        private bool AreValidWords(string itemName, HashSet<string> uniqueness, IEnumerable<string> items, Action<string, string[]> onError)
        {
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item) || item.Contains(" ") || !Char.IsLetter(item[0]))
                {
                    onError("SM01", new string[] { itemName, item });
                    return false;
                }

                // duplicate terms are allowed
                if (itemName != "Term")
                {
                    if (uniqueness.Contains(item))
                    {
                        onError("SM02", new string[] { itemName, item });
                        return false;
                    }

                    uniqueness.Add(item);
                }
            }

            return true;
        }

    }
}
