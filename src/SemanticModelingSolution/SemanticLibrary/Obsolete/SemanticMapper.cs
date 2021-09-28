using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    public class SemanticMapper
    {
        private const int _minimumScoreForTypes = 50;
        private readonly bool _enableVerboseLogOnConsole;

        public SemanticMapper(bool enableVerboseLogOnConsole)
        {
            _enableVerboseLogOnConsole = enableVerboseLogOnConsole;
        }

        public IEnumerable<(ModelTypeNode modelTypeNode, int score)> FindMatches(
            ModelTypeNode source, IEnumerable<ModelTypeNode> targets)
        {
            return null;
        }

    }
}
