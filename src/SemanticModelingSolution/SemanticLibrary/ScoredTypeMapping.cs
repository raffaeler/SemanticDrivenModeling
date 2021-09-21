using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SemanticLibrary
{
    public record ScoredTypeMapping//(ModelTypeNode SourceModelTypeNode, ModelTypeNode TargetModelTypeNode, int TypeScore)
    {
        public ScoredTypeMapping()
        {
        }

        public ScoredTypeMapping(ModelTypeNode sourceModelTypeNode, ModelTypeNode targetModelTypeNode, int typeScore)
        {
            this.SourceModelTypeNode = sourceModelTypeNode;
            this.TargetModelTypeNode = targetModelTypeNode;
            this.TypeScore = typeScore;
        }

        public ModelTypeNode SourceModelTypeNode { get; set; }
        public ModelTypeNode TargetModelTypeNode { get; set; }
        public int TypeScore { get; set; }

        public int PropertiesScore => PropertyMappings.Select(m => m.Score).Sum();
        public IList<ScoredPropertyMapping<ModelNavigationNode>> PropertyMappings { get; set; }
    }
}
