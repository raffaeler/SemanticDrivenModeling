﻿using System;

namespace SemanticLibrary
{
    public record Concept(string Name, string description, params ConceptSpecifier[] ConceptSpecifiers)
    {
    }
}
