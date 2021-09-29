using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticLibrary
{
    public record Evaluation
    {
        public Evaluation(int score) => Score = score;

        public int Score { get; set; }
    }
}
