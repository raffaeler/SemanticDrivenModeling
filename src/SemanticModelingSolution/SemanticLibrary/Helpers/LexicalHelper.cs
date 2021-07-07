using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SemanticLibrary.Helpers
{
    public class LexicalHelper
    {
        public static string[] CamelPascalCaseExtract(IList<string> composedTerms, string word)
        {
            if (word.Contains(" ")) throw new ArgumentException($"{word} must not contain spaces");
            if (word.Length == 0) return Array.Empty<string>();
            var composed = composedTerms
                .Select(t => (word: t, start: word.IndexOf(t)))
                .Where(ws => ws.start != -1)
                .ToArray();

            if (composed.Length == 0)
            {
                return CamelPascalCaseExtract(word);
            }

            var result = new List<string>();
            for (int i = 0; i < composed.Length; i++)
            {
                var item = composed[i];
                if(item.start != 0)
                {
                    var segments = CamelPascalCaseExtract(word.Substring(0, item.start));
                    result.AddRange(segments);
                }

                result.Add(item.word);

                if (word.Length > item.start + item.word.Length)
                {
                    var segments = CamelPascalCaseExtract(word.Substring(item.start + item.word.Length));
                    result.AddRange(segments);
                }
            }

            return result.ToArray();
        }


        private static string[] CamelPascalCaseExtract(string word)
        {
            List<string> parts = new();
            StringBuilder sb = new(word.Length);
            var first = word[0];
            // if CamelCase, automatically switch to Pascal case
            sb.Append(char.ToUpper(first));
            bool isLastUpper = char.IsUpper(first);

            for (int i = 1; i < word.Length; i++)
            {
                var ch = word[i];
                var currentIsUpper = char.IsUpper(ch);
                if (currentIsUpper)
                {
                    if (!isLastUpper)
                    {
                        Commit();
                    }
                }

                sb.Append(ch);
                isLastUpper = currentIsUpper;
            }

            Commit();
            return parts.ToArray();

            void Commit()
            {
                if (sb.Length > 0)
                {
                    parts.Add(sb.ToString());
                    sb.Clear();
                }
            }
        }

    }
}
