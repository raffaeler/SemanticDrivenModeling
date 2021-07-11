using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace SemanticLibrary
{
    [Obsolete]
    public record Scope
    {
        public Scope(Concept concept, params Imply[] implies)
        {
            this.Concept = concept;
            var temp = new List<Imply>();
            temp.AddRange(implies);
            this.Implies = temp;
        }

        public Scope(params Imply[] implies)
        {
            this.Concept = null;
            var temp = new List<Imply>();
            temp.AddRange(implies);
            this.Implies = temp;
        }

        public Concept Concept { get; init; }

        public IList<Imply> Implies { get; init; }

        public bool IsGlobal => Concept == null;
    }
}
