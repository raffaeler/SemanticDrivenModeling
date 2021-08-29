using System;

namespace SemanticLibrary
{
    public record Concept(string Name, string Description, params ConceptSpecifier[] ConceptSpecifiers)
    {
    }
}
