using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticGlossaryGenerator
{
    /// <summary>
    /// Parser of a simple textual file format. Rules:
    /// - Every content is a separate line
    /// - Lines starting by "#" are totally ignored (internal comments)
    /// - Empty lines are ignored
    /// - Lines starting by "//" are inserted as comments in the generated code
    /// - Lines starting by "$" are inserted as description
    /// - Lines starting by one or more " " are trimmed and added as Alias of the main string (the one not starting by space)
    /// Note: not all the parsed file may have a use for "aliases"
    /// 
    /// This parser is externallyu fed with one line at a time.
    /// </summary>
    public class SimplifiedTextParser
    {
        public static readonly string InternalCommentTag = "#";
        public static readonly string GeneratedCommentTag = "//";
        public static readonly string DescriptionTag = "$";
        public static readonly string Whitespace = " ";

        private List<string> _comments;
        private List<string> _aliases;
        private string _word;
        private string _description;
        private Action<string, string, List<string>, List<string>> _commit;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commit">A lambda taking word, description, comments, aliases</param>
        public SimplifiedTextParser(Action<string, string, List<string>, List<string>> commit)
        {
            _commit = commit;
        }

        public void Feed(string line)
        {
            if (line == null)
            {
                // last line with no final crlf
                Emit(_word, _description);
                return;
            }

            if (line.StartsWith(InternalCommentTag))
            {
                return;
            }

            if (line.StartsWith(GeneratedCommentTag))
            {
                if (_word != null) Emit(_word, _description);

                AddComment(line);
                return;
            }

            if (line.StartsWith(DescriptionTag))
            {
                if (_word != null) Emit(_word, _description);

                _description = line.Substring(1).Trim();
                return;
            }

            if (line.StartsWith(Whitespace))
            {
                AddAlias(line);
                return;
            }

            if (line.Length == 0)
            {
                Emit(_word, _description);
                return;
            }

            Emit(_word, _description);
            _word = line.Trim();
        }


        private void Reset()
        {
            _comments = null;
            _aliases = null;
            _word = null;
            _description = null;
        }

        private void AddComment(string comment)
        {
            if (_comments == null) _comments = new List<string>();
            _comments.Add(comment.TrimStart('/', ' ').TrimEnd(' '));
        }

        private void AddAlias(string alias)
        {
            if (_aliases == null) _aliases = new List<string>();
            _aliases.Add(alias.Trim());
        }

        private void Emit(string word, string description)
        {
            if (word == null) return;

            _commit(word,
                description == null ? string.Empty : description,
                _comments == null? new List<string>() : _comments,
                _aliases == null ? new List<string>() : _aliases);
            Reset();
        }
    }
}
