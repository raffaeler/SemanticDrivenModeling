using System;

namespace SemanticLibrary
{
    public record Concept(string Name, string description, params ConceptAttribute[] ConceptAttributes)
    {
        //public ConceptNature Nature { get; set; }
    }
}
