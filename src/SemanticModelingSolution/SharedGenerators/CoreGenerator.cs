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

        public static readonly string TermClassName = "Term";
        public static readonly string ConceptClassName = "Concept";
        public static readonly string ConceptSpecifierClassName = "ConceptSpecifier";
        public static readonly string TermsToConceptClassName = "TermsToConcept";
        public static readonly string TermsToConceptPropertyName = "Links";
        public static readonly string TermsToConceptPropertyComment = "The relationships between terms and a concept";

        public static readonly string NoneConceptSpecifier = "None";


        public CoreGenerator()
        {
            Terms = new ClassGenerator(Namespace, GeneratdTermsClassName);
            Concepts = new ClassGenerator(Namespace, GeneratedConceptsClassName);
            ConceptSpecifiers = new ClassGenerator(Namespace, GeneratedConceptSpecifiersClassName);
            Domain = new ClassGenerator(Namespace, GeneratedDomainClassName);

            Terms.Usings.Add("System");
            Terms.Usings.Add("System.Collections.Generic");
            Terms.Usings.Add("SemanticLibrary");

            Concepts.Usings.Add("System");
            Concepts.Usings.Add("System.Collections.Generic");
            Concepts.Usings.Add("SemanticLibrary");

            ConceptSpecifiers.Usings.Add("System");
            ConceptSpecifiers.Usings.Add("System.Collections.Generic");
            ConceptSpecifiers.Usings.Add("SemanticLibrary");

            ConceptSpecifiers.Members.Add(ConceptSpecifiers.CreateStaticField(
                new string[]{ "Default specifier when the term does not add any further information" },
                ConceptSpecifierClassName,
                NoneConceptSpecifier,
                ConceptSpecifiers.CreateInitializersWithStrings(ConceptSpecifierClassName, NoneConceptSpecifier)));


            Domain.Usings.Add("System");
            Domain.Usings.Add("System.Collections.Generic");
            Domain.Usings.Add("SemanticLibrary");

            Domain.Members.Add(Domain.CreatePropertyWithInitializer(
                new[] { TermsToConceptPropertyComment, },
                Domain.MakeListOfT(TermsToConceptClassName), TermsToConceptPropertyName,
                Domain.CreateInitializerWithCollection(TermsToConceptClassName)));
        }

        public ClassGenerator Terms { get; }
        public ClassGenerator Concepts { get; }
        public ClassGenerator ConceptSpecifiers { get; }
        public ClassGenerator Domain { get; }

        public void ProcessFile(FileInfo fileInfo, Action<string, string[]> onError)
        {
            HashSet<string> conceptsUniqueness = new HashSet<string>();
            HashSet<string> termsUniqueness = new HashSet<string>();
            HashSet<string> contexts = new HashSet<string>();

            List<ExpressionSyntax> links = new List<ExpressionSyntax>();
            var parser = new SimplifiedRelationshipsParser(
                (List<string> words, string description, List<string> comments, List<List<string>> aliasesList) =>
                {
                    int termsIndex = 1;
                    if (!AreValidWords("Concept", conceptsUniqueness, words, onError))
                    {
                        return;                    }

                    if (!AreValidWords("Term", termsUniqueness, aliasesList
                                                .Where(a => a.Count > termsIndex)
                                                .Select(a => a[termsIndex]),
                                                onError))
                    {
                        return;
                    }

                    var mainConcept = words.First();
                    List<string> specifiers = new();

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

                        Terms.Members.Add(Terms.CreateStaticField(new[] { termDescription }, TermClassName, term,
                            Terms.CreateInitializersWithStrings(TermClassName, term, termDescription)));

                        if (!string.IsNullOrEmpty(specifier))
                        {
                            specifiers.Add(specifier);
                        }
                        else
                        {
                            specifier = NoneConceptSpecifier;
                        }

                        links.Add(Domain.CreateCreateObject(TermsToConceptClassName,
                            Domain.CreateMemberAccess2(GeneratedConceptsClassName, mainConcept),
                            Domain.CreateMemberAccess2(GeneratedConceptsClassName, contextConcept),
                            Domain.CreateMemberAccess2(GeneratedConceptSpecifiersClassName, specifier),
                            Domain.CreateMemberAccess2(GeneratdTermsClassName, term),
                            Domain.CreateNumericLiteralExpression(weight)));

                        //links.Add(Domain.CreateCreateObject(TermsToConceptClassName,
                        //    Domain.CreateMemberAccess2(GeneratedConceptsClassName, mainConcept),
                        //    Domain.CreateMemberAccess2(GeneratedConceptsClassName, contextConcept),
                        //    Domain.CreateTuple(Domain.CreateMemberAccess2(GeneratdTermsClassName, term), Domain.CreateNumericLiteralExpression(weight))));
                    }


                    List<ExpressionSyntax> expressions = new();
                    expressions.Add(Concepts.CreateStringLiteralExpression(mainConcept));
                    expressions.Add(Concepts.CreateStringLiteralExpression(description));
                    foreach (var specifier in specifiers)
                    {
                        ConceptSpecifiers.Members.Add(ConceptSpecifiers.CreateStaticField(
                            Array.Empty<string>(),
                            ConceptSpecifierClassName,
                            specifier,
                            ConceptSpecifiers.CreateInitializersWithStrings(ConceptSpecifierClassName, specifier)));

                        expressions.Add(Concepts.CreateMemberAccess2(GeneratedConceptSpecifiersClassName, specifier));
                    }

                    Concepts.Members.Add(Concepts.CreateStaticField(comments?.ToArray(), ConceptClassName, mainConcept,
                        Concepts.CreateInitializersWithExpressions(ConceptClassName, expressions.ToArray())));
                });

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

            var statements = links
                .Select(e => Domain.CreateAddCollection(TermsToConceptPropertyName, e));

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

                if (uniqueness.Contains(item))
                {
                    onError("SM02", new string[] { itemName, item });
                    return false;
                }

                uniqueness.Add(item);
            }

            return true;
        }

    }
}
