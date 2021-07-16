using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary;

namespace DeserializationConsole
{
    public class Analyzer
    {
        private Dictionary<string, Type[]> _models = new Dictionary<string, Type[]>();

        public Analyzer()
        {
            this.Domain = new GeneratedCode.Domain();
        }

        public GeneratedCode.Domain Domain { get; set;  }
        public IDictionary<string, IList<ModelTypeNode>> ModelTypeNodes { get; set; } = new Dictionary<string, IList<ModelTypeNode>>();

        public IList<ModelTypeNode> Prepare(string friendlyName, Type[] modelTypes)
        {
            var visitor = new DomainTypesGraphVisitor(Domain, modelTypes);
            var models = visitor.Visit(null, null, null);

            _models[friendlyName] = modelTypes;
            ModelTypeNodes[friendlyName] = models;
            return models;
        }

        public ScoredTypeMapping CreateMappingsFor(ModelTypeNode source, IList<ModelTypeNode> candidateTargets)
        {
            var matcher = new ConceptMatchingRule(true);
            matcher.ComputeMappings(source, candidateTargets);
            return matcher.CandidateTypes.First();
        }
    }
}
