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
                .OrderBy(ws => ws.start)
                .ToArray();

            var lastEnd = 0;
            foreach (var c in composed)
            {
                if (c.start < lastEnd) throw new Exception("Composed words are overlapping");
                lastEnd += c.word.Length;
            }

            if (composed.Length == 0)
            {
                return CamelPascalCaseExtract(word);
            }

            var result = new List<string>();
            int startFence = 0;
            for (int i = 0; i < composed.Length; i++)
            {
                var item = composed[i];
                // add the non-composed words before the current composed word
                if (item.start != startFence)
                {
                    var segments = CamelPascalCaseExtract(word.Substring(startFence, item.start - startFence));
                    result.AddRange(segments);
                }

                // add the composed word
                result.Add(item.word);

                startFence += item.word.Length;//item.start + word.Length;

                // if it is the last composed word, add the non-composed words after the current composed word
                if (i == composed.Length - 1)
                {
                    if (word.Length > item.start + item.word.Length)
                    {
                        var segments = CamelPascalCaseExtract(word.Substring(item.start + item.word.Length));
                        result.AddRange(segments);
                    }
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
