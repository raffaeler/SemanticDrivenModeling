using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace SemanticGlossaryGenerator
{
    public static class Diagnostics
    {
        public static Dictionary<string, DiagnosticDescriptor> Messages = new Dictionary<string, DiagnosticDescriptor>()
        {
            { "SM01", new DiagnosticDescriptor(
                    id: "SM01",
                    title: "Concept strings cannot contain spaces",
                    messageFormat: "Concept strings cannot contain spaces: '{0}'",
                    category: "Parser",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true) },

            { "SM03", new DiagnosticDescriptor(
                        id: "SM03",
                        title: "Duplicate concepts are not allowed",
                        messageFormat: "Duplicate concepts are not allowed: skipping '{0}'",
                        category: "Parser",
                        DiagnosticSeverity.Warning,
                        isEnabledByDefault: true) },
        };
    }
}
