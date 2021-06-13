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
        public static readonly string GeneratedDomainClassName = "Domain";
        public static readonly string TermClassName = "Term";
        public static readonly string ConceptClassName = "Concept";
        public static readonly string TermsToConceptClassName = "TermsToConcept";
        public static readonly string TermsToConceptPropertyName = "Links";
        public static readonly string TermsToConceptPropertyComment = "The relationships between terms and a concept";


        public CoreGenerator()
        {
            Terms = new ClassGenerator(Namespace, GeneratdTermsClassName);
            Concepts = new ClassGenerator(Namespace, GeneratedConceptsClassName);
            Domain = new ClassGenerator(Namespace, GeneratedDomainClassName);

            Terms.Usings.Add("System.Collections.Generic");
            Terms.Usings.Add("SemanticLibrary");

            Concepts.Usings.Add("System.Collections.Generic");
            Concepts.Usings.Add("SemanticLibrary");

            Domain.Usings.Add("System.Collections.Generic");
            Domain.Usings.Add("SemanticLibrary");


            Domain.Members.Add(Domain.CreatePropertyWithInitializer(
                new[] { TermsToConceptPropertyComment, },
                Domain.MakeListOfT(TermsToConceptClassName), TermsToConceptPropertyName,
                Domain.CreateInitializerWithCollection(TermsToConceptClassName)));
        }

        public ClassGenerator Terms { get; }
        public ClassGenerator Concepts { get; }
        public ClassGenerator Domain { get; }

        public void ProcessFile(FileInfo fileInfo, Action<string, string[]> onError)
        {
            HashSet<string> _uniqueness = new HashSet<string>();

            List<ExpressionSyntax> links = new List<ExpressionSyntax>();
            var parser = new SimplifiedRelationshipsParser(
                (List<string> words, string description, List<string> comments, List<List<string>> aliases) =>
                {
                    var mainConcept = words.First();
                    if (mainConcept.Contains(' '))
                    {
                        onError("SM01", new string[] { mainConcept });
                        return;
                    }

                    if (_uniqueness.Contains(mainConcept))
                    {
                        onError("SM03", new string[] { mainConcept });
                        return;
                    }

                    _uniqueness.Add(mainConcept);

                    Concepts.Members.Add(Concepts.CreateStaticField(comments?.ToArray(), ConceptClassName, mainConcept,
                        Concepts.CreateInitializersWithStrings(ConceptClassName, mainConcept, description)));


                    foreach (var alias in aliases)
                    {
                        var term = alias[0];
                        int weight;
                        if (alias.Count <= 0 || !int.TryParse(alias[1], out weight))
                        {
                            weight = 100;
                        }
                        
                        var termDescription = alias.Count > 2 ? alias[2] : string.Empty;

                        Terms.Members.Add(Terms.CreateStaticField(new[] { termDescription }, TermClassName, term,
                            Terms.CreateInitializersWithStrings(TermClassName, term, termDescription)));

                        links.Add(Domain.CreateCreateObject(TermsToConceptClassName,
                            Domain.CreateMemberAccess2(GeneratedConceptsClassName, mainConcept),
                            Domain.CreateTuple(Domain.CreateMemberAccess2(GeneratdTermsClassName, term), Domain.CreateNumericLiteralExpression(weight))));

                    }

                });

            using var file = fileInfo.OpenText();
            string line;
            while ((line = file.ReadLine()) != null)
            {
                parser.Feed(line);
            }

            parser.Feed(null);

            var statements = links
                .Select(e => Domain.CreateAddCollection(TermsToConceptPropertyName, e));
            Domain.Members.Add(Domain.CreateConstructor(Array.Empty<string>(), statements.ToArray()));
        }


    }
}
