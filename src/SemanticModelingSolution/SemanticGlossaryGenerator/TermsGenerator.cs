using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SemanticGlossaryGenerator
{
    //[Generator]
    public class TermsGenerator : ISourceGenerator
    {
        public static readonly string Prefix = "Terms";
        private static readonly string _spaces = new string(' ', 8);

        public void Execute(GeneratorExecutionContext context)
        {
            var files = GetFilesByPrefix(context.AdditionalFiles, Prefix);

            var nspace = "GeneratedCode";
            var className = "KnownTerms";
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using SemanticLibrary;");
            sb.AppendLine($"namespace {nspace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public static class {className}");
            sb.AppendLine("    {");

            foreach (var file in files)
            {
                GenerateField(context, sb, file);
            }

            sb.AppendLine("    }"); // class
            sb.AppendLine("}"); // namespace

            var sourceCode = sb.ToString();

            context.AddSource(nameof(TermsGenerator), SourceText.From(sourceCode, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //context.RegisterForPostInitialization(pi => pi.AddSource(...,...))
            //context.RegisterForSyntaxNotifications()
        }

        private List<FileInfo> GetFilesByPrefix(IEnumerable<AdditionalText> additionalFiles, string prefix)
        {
            var files = new List<FileInfo>();
            foreach (var additional in additionalFiles)
            {
                var fi = new FileInfo(additional.Path);
                var name = fi.Name;
                if (!name.StartsWith(Prefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                files.Add(fi);
            }

            return files;
        }

        public void GenerateField(GeneratorExecutionContext context, StringBuilder sb, FileInfo fileInfo)
        {
            HashSet<string> _uniqueness = new HashSet<string>();

            var parser = new SimplifiedTextParser((string word, string description, List<string> comments, List<string> aliases) =>
                {
                    if (_uniqueness.Contains(word))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                            id: "SM04",
                            title: "Duplicate terms are not allowed",
                            messageFormat: "Duplicate terms are not allowed: skipping '{0}'",
                            category: "TermsGenerator",
                            DiagnosticSeverity.Warning,
                            isEnabledByDefault: true), Location.None, word));
                        return;
                    }

                    _uniqueness.Add(word);
                    GenerateField(context, sb, word, description, comments, aliases);
                });

            using var file = fileInfo.OpenText();
            string line;
            while ((line = file.ReadLine()) != null)
            {
                parser.Feed(line);
            }

            parser.Feed(null);
        }

        /// <summary>
        /// 
        /// </summary>
        private void GenerateField(GeneratorExecutionContext context, StringBuilder sb,
            string word, string description, List<string> comments, List<string> aliases)
        {
            if (word.Contains(" "))
            {
                // TODO: future => merge words with pascal case notation
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                    id: "SM02",
                    title: "Term strings cannot contain spaces",
                    messageFormat: "Term strings cannot contain spaces: '{0}'",
                    category: "TermsGenerator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true), Location.None, word));
            }

            sb.AppendLine($"{_spaces}/// <summary>");
            if (comments != null)
            {
                foreach (var comment in comments)
                {
                    sb.AppendLine($"{_spaces}/// {comment}");
                }
            }

            var desc = description == null ? string.Empty : description;

            sb.AppendLine($"{_spaces}/// </summary>");

            if (aliases != null)
            {
                var terms = string.Join(", ", aliases
                    .Select(a => $"\"{a}\""));

                sb.AppendLine($"{_spaces}public static Term {word} = new Term(\"{word}\", \"{desc}\", {terms});");
            }
            else
            {
                sb.AppendLine($"{_spaces}public static Term {word} = new Term(\"{word}\", \"{desc}\");");
            }

            sb.AppendLine();
        }



    }
}
