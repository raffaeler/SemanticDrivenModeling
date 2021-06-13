using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SemanticGlossaryGenerator;

using SemanticLibrary;
using SemanticLibrary.Helpers;

namespace SemanticStructuresTests
{
    [TestClass]
    public class InfrastructureTests
    {
        [TestMethod]
        public void Terms()
        {
            // should not throw
            var data1 = RawDataLoaders.LoadTerms(@"Data\ERP-Terms-01.txt");
            Assert.IsTrue(data1.Count > 0);

            var data2 = RawDataLoaders.LoadTerms(@"Data\ERP-Terms-02.txt");
            Assert.IsTrue(data2.Count > 0);

            var data3 = RawDataLoaders.LoadTerms(@"Data\ERP-Terms-03.txt");
            Assert.IsTrue(data3.Count > 0);

            var dataFinal = RawDataLoaders.MergeTerms(data1, data2, data3);
            Assert.IsTrue(dataFinal.Count > data1.Count);
            Assert.IsTrue(dataFinal.Count > data2.Count);
            Assert.IsTrue(dataFinal.Count > data3.Count);
        }


        [TestMethod]
        public void ParseHierarchyTextFile()
        {
            Debug.WriteLine(Directory.GetCurrentDirectory());
            var fileInfo = new FileInfo("Samples\\Relationships.txt");
            var index = 0;
            var results = new (int, int, int)[] { (5, 1, 3), (5, 2, 2) };

            var parser = new SimplifiedRelationshipsParser(
                (List<string> words, string description, List<string> comments, List<List<string>> aliases) =>
            {
                var wordsCount = results[index].Item1;
                var aliasCount = results[index].Item2;
                var aliasSubCount = results[index].Item3;

                Assert.AreEqual(wordsCount, words.Count, "words");
                Assert.AreEqual(aliasSubCount, aliases[aliasCount - 1].Count, "aliases");
                index++;
            });

            using var file = fileInfo.OpenText();
            string line;
            while ((line = file.ReadLine()) != null)
            {
                parser.Feed(line);
            }

            parser.Feed(null);
        }
    }
}
