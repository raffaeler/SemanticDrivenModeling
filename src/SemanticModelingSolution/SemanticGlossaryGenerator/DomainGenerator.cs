using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using SemanticGlossaryGenerator.Helpers;

namespace SemanticGlossaryGenerator
{
    [Generator]
    public class DomainGenerator : ISourceGenerator
    {
        private CoreGenerator _core = new CoreGenerator();
        private static readonly string Prefix = "Domain";

        public void Execute(GeneratorExecutionContext context)
        {
            var files = GetFilesByPrefix(context.AdditionalFiles, Prefix);

            foreach (var file in files)
            {
                _core.ProcessFile(file, (string key, string[] parameters) =>
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.Messages[key], Location.None, parameters));
                });
            }

            context.AddSource(nameof(DomainGenerator) + "_" + "Concepts", _core.Concepts.Generate());
            context.AddSource(nameof(DomainGenerator) + "_" + "Terms", _core.Terms.Generate());
            context.AddSource(nameof(DomainGenerator) + "_" + "Domain", _core.Domain.Generate());
        }

        public void Initialize(GeneratorInitializationContext context)
        {

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

    }
}
