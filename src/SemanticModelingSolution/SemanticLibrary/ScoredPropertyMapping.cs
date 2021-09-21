using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    public record ScoredPropertyMapping<T>//(T Source, T Target, int Score)
        where T : IEqualityComparer<T>
    {
        public ScoredPropertyMapping()
        {
        }

        public ScoredPropertyMapping(T source, T target, int score)
        {
            Source = source;
            Target = target;
            Score = score;
        }

        public T Source { get; set; }
        public T Target { get; set; }
        public int Score { get; set; }
    }
}
