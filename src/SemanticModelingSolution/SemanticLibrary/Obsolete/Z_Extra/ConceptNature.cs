﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticLibrary
{
    public record ConceptNature(string Name, params ConceptSpecifier[] ConceptSpecifiers)
    {
    }
}