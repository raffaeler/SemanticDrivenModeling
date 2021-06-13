using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SemanticLibrary.Helpers
{
    public static class RawDataLoaders
    {
        public static readonly string CommentToken = "#";

        /// <summary>
        /// Load all the terms from a csv file.
        /// Expected format: string1[tab]string2
        /// string1 may start with a '#' (comment). In this case the entire line is skipped
        /// string1 is the term
        /// string2 is the definition
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static IList<Term> LoadTerms(string filename)
        {
            var fullname = Path.GetFullPath(filename);
            if (!File.Exists(fullname)) throw new ArgumentException($"The file {fullname} does not exist");

            var content = File.ReadAllText(fullname);
            using var sr = new StringReader(content);

            List<Term> terms = new();
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                var parts = line.Split('\t');
                if (parts.Length == 0) continue;
                if (string.IsNullOrEmpty(parts[0]) || parts[0].StartsWith(CommentToken)) continue;

                if (parts.Length < 2) throw new InvalidOperationException($"The file format is incorrect: at least two fields separated by a [tab] are expected\r\n{line}");

                var (main, others) = GetMultiples(parts[0]);
                var term = new Term(main, parts[1]);
                if (others != null && others.Count > 0)
                {
                    foreach (var item in others)
                        term.AltNames.Add(item);
                }

                terms.Add(term);
            }

            return terms;
        }

        private static (string main, IList<string> others) GetMultiples(string key)
        {
            List<string> others = null;
            int index = 0;
            string main = key;


            while ((index = key.IndexOf('(', index)) != -1)
            {
                var endIndex = key.IndexOf(')', index);
                if (endIndex == -1) break;
                if (others == null) others = new();

                if (others.Count == 0)
                {
                    main = key.Substring(0, index).Trim();
                }

                var syn = key.Substring(index + 1, endIndex - index - 1);
                others.Add(syn);
                index++;
            }

            return (main, others);
        }

        public static IList<Term> MergeTerms(params IList<Term>[] termLists)
        {
            return termLists
                .SelectMany(a => a)
                .GroupBy(t => t.Name.ToLower(), t => t)
                .Select(g => MergeTerm(g))
                .OrderBy(t => t.Name)
                .ToList();
        }

        private static Term MergeTerm(IEnumerable<Term> terms)
        {
            Term result = null;
            foreach (var t in terms)
            {
                if (result == null)
                {
                    result = t;
                    continue;
                }

                foreach (var alt in t.AltNames)
                {
                    if (result.AltNames.Contains(alt, StringComparer.CurrentCultureIgnoreCase))
                        continue;

                    result.AltNames.Add(alt);
                }
            }

            return result;
        }

    }
}
