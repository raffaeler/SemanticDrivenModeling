using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SemanticGlossaryGenerator
{
    public class SimplifiedRelationshipsParser
    {
        private SimplifiedTextParser _parser;
        private Action<List<string>, string, List<string>, List<List<string>>> _commit;
        private char[] _listSep = new char[] { ':' };

        public SimplifiedRelationshipsParser(Action<List<string>, string, List<string>, List<List<string>>> commit,
            Action<string, string> varAssignment)
        {
            _parser = new SimplifiedTextParser(OnParsed, varAssignment);
            _commit = commit;
        }

        public void Feed(string line) => _parser.Feed(line);

        private void OnParsed(string word, string description, List<string> comments, List<string> aliases)
        {
            var words = word.Split(_listSep, StringSplitOptions.None)
                .Select(w => w.Trim())
                .ToList();

            var newAliases = aliases
                .Select(a => a.Split(_listSep, StringSplitOptions.None)
                    .Select(w => w.Trim())
                    .ToList())
                .ToList();

            _commit(words, description, comments, newAliases);
        }

    }
}
