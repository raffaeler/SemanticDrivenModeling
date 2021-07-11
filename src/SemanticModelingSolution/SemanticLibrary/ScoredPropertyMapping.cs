using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    public record ScoredPropertyMapping<T>(T Source, T Target, int Score) where T : IEqualityComparer<T>;
}
