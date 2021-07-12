using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using SemanticGlossaryGenerator;
using SemanticGlossaryGenerator.Helpers;

namespace TestConsole
{
    class Program
    {

        static void Main(string[] args)
        {
            CoreGenerator gen = new CoreGenerator();
            gen.ProcessFile(new System.IO.FileInfo("DomainBase.txt"), (key, parameters) =>
            {
                string format = Diagnostics.Messages[key].MessageFormat.ToString(null);
                var message = string.Format(format, parameters);
                Console.WriteLine($"Error {key}, {message}");
            });
            var sourceTerms = gen.Terms.Generate();
            var sourceConcepts = gen.Concepts.Generate();
            var sourceConceptSpecifiers = gen.ConceptSpecifiers.Generate();
            var sourceDomain = gen.Domain.Generate();

            var txtTerms = sourceTerms.ToString();
            var txtConcepts = sourceConcepts.ToString();
            var txtConceptSpecifiers = sourceConceptSpecifiers.ToString();
            var txtDomain = sourceDomain.ToString();
            Console.WriteLine(txtTerms);
            Console.WriteLine();
            Console.WriteLine(txtConcepts);
            Console.WriteLine();
            Console.WriteLine(txtConceptSpecifiers);
            Console.WriteLine();
            Console.WriteLine(txtDomain);
        }

        static void Main2(string[] args)
        {

            ClassGenerator gen = new("MyNamespace", "MyClass");
            gen.Usings.Add("System");

            //var comment = gen.CreateXmlComment(true, "a", "b", "c");
            // var commentText = comment.ToFullString();


            string word = "Identity";
            string description = "Not necessarily an item identifier. Name, Tag, etc.";
            List<string> comments = new() { "The non unique identity of an entity" };
            List<string> aliases = new() { "Name", "80", "The name of the identity" };

            gen.Members.Add(gen.CreateStaticField(comments.ToArray(), "Term", aliases[0],
                gen.CreateInitializersWithStrings("Term", aliases[0], aliases[2])));

            gen.Members.Add(gen.CreateStaticField(comments.ToArray(), "Concept", word,
                gen.CreateInitializersWithStrings("Concept", word, description)));

            var co = gen.CreateCreateObject("TermToConcept",
                        gen.CreateMemberAccess2("KnownConcepts", "Identity"),
                        gen.CreateTuple(gen.CreateMemberAccess2("KnownTerms", "Name"), gen.CreateNumericLiteralExpression(90)));

            gen.Members.Add(gen.CreatePropertyWithInitializer(
                new[] { "a", "b", "c" },
                gen.MakeListOfT("TermToConcept"), "Links",
                gen.CreateInitializerWithCollection("TermToConcept"
                //co, co, co
                )));

            var statements = new[] { co, co, co }
                .Select(e => gen.CreateAddCollection("Links", e));
            gen.Members.Add(gen.CreateConstructor(Array.Empty<string>(), statements.ToArray()));

            var source = gen.Generate();
            Console.WriteLine(source.ToString());
        }
    }
}
