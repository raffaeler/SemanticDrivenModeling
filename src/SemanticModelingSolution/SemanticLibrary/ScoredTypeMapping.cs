using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SemanticLibrary
{
    public record ScoredTypeMapping(ModelTypeNode SourceModelTypeNode, ModelTypeNode TargetModelTypeNode, int TypeScore)
    {
        public int PropertiesScore => PropertyMappings.Select(m => m.Score).Sum();
        public IList<ScoredPropertyMapping<ModelNavigationNode>> PropertyMappings { get; internal set; }
    }
}
