using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary.Helpers
{
    public class LexicalHelper
    {
        public static string[] CamelPascalCaseExtract(string word)
        {
            if (word.Contains(" ")) throw new ArgumentException($"{word} must not contain spaces");
            if (word.Length == 0) return Array.Empty<string>();

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
