using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SurrogateLibrary;

namespace SemanticLibrary
{
    public record TypePairEvaluation(SurrogateType<Metadata> Source, SurrogateType<Metadata> Target,
        Evaluation Evaluation)
    {
        public IList<NavigationPair> Mappings { get; set; }
        public int PropertiesScore => Mappings.Select(m => m.Evaluation.Score).Sum();
    }

}
