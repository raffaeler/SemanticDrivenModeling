using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SemanticGlossaryGenerator;

using SemanticLibrary;
using SemanticLibrary.Helpers;

namespace SemanticStructuresTests
{
    [TestClass]
    public class InfrastructureTests
    {
        //[TestMethod]
        //public void Terms()
        //{
        //    // should not throw
        //    var data1 = RawDataLoaders.LoadTerms(@"Data\ERP-Terms-01.txt");
        //    Assert.IsTrue(data1.Count > 0);

        //    var data2 = RawDataLoaders.LoadTerms(@"Data\ERP-Terms-02.txt");
        //    Assert.IsTrue(data2.Count > 0);

        //    var data3 = RawDataLoaders.LoadTerms(@"Data\ERP-Terms-03.txt");
        //    Assert.IsTrue(data3.Count > 0);

        //    var dataFinal = RawDataLoaders.MergeTerms(data1, data2, data3);
        //    Assert.IsTrue(dataFinal.Count > data1.Count);
        //    Assert.IsTrue(dataFinal.Count > data2.Count);
        //    Assert.IsTrue(dataFinal.Count > data3.Count);
        //}


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

        [TestMethod]
        public void InvalidComposedWord()
        {
            var testDomain = new MyDomain();

            Assert.ThrowsException<Exception>(() => LexicalHelper.CamelPascalCaseExtract(testDomain.AllTerms, "AName"),
                "Composed words should not be able to overlap");
            Assert.ThrowsException<Exception>(() => LexicalHelper.CamelPascalCaseExtract(testDomain.AllTerms, "MyAName"),
                "Composed words should not be able to overlap");
        }

        [TestMethod]
        public void TestPascalCaseExtract()
        {
            var testDomain = new MyDomain();

            CollectionAssert.AreEqual(new string[] { "Id" }, LexicalHelper.CamelPascalCaseExtract(testDomain.AllTerms, "Id"));
            CollectionAssert.AreEqual(new string[] { "Id" }, LexicalHelper.CamelPascalCaseExtract(testDomain.AllTerms, "id"));
            CollectionAssert.AreEqual(new string[] { "First", "Name" }, LexicalHelper.CamelPascalCaseExtract(testDomain.AllTerms, "FirstName"));
            CollectionAssert.AreEqual(new string[] { "First", "Name" }, LexicalHelper.CamelPascalCaseExtract(testDomain.AllTerms, "firstName"));
        }

        [TestMethod]
        public void TestPascalCaseExtract2()
        {
            var testDomain = new MyDomain();

            CollectionAssert.AreEqual(new string[] { "ZipCode" }, LexicalHelper.CamelPascalCaseExtract(testDomain.ComposedTerms, "ZipCode"));
            CollectionAssert.AreEqual(new string[] { "Mega", "ZipCode" }, LexicalHelper.CamelPascalCaseExtract(testDomain.ComposedTerms, "MegaZipCode"));
            CollectionAssert.AreEqual(new string[] { "Mega", "ZipCode", "Id" }, LexicalHelper.CamelPascalCaseExtract(testDomain.ComposedTerms, "MegaZipCodeId"));
        }

        [TestMethod]
        public void TestPascalCaseExtract3()
        {
            var testDomain = new GeneratedCode.Domain();
            var composedTerms = testDomain.Links
                .Select(ttc => ttc.Term.Name)
                .Where(t => t.ToCharArray().Where(c => char.IsUpper(c)).Count() > 1).ToList();

            CollectionAssert.AreEqual(new string[] { "ZipCode" }, LexicalHelper.CamelPascalCaseExtract(composedTerms, "ZipCode"));
            CollectionAssert.AreEqual(new string[] { "Mega", "ZipCode" }, LexicalHelper.CamelPascalCaseExtract(composedTerms, "MegaZipCode"));
            CollectionAssert.AreEqual(new string[] { "Mega", "ZipCode", "Id" }, LexicalHelper.CamelPascalCaseExtract(composedTerms, "MegaZipCodeId"));
        }

        private class MyDomain : DomainBase
        {
            //public override List<TermToConcept> Links => AllTerms
            //    .Select(t => new TermToConcept(null, null, null, new Term(t), 0))
            //    .ToList();

            public MyDomain()
            {
                Links.AddRange(AllTerms
                    .Select(t => new TermToConcept(null, null, null, new Term(t), 0)));
            }

            public override List<TermToConcept> Links => base.Links;

            public List<string> AllTerms => new()
            {
                "Id", "First", "Name", "AName","My", "ZipCode"
            };

            public List<string> ComposedTerms => Links
                .Select(ttc => ttc.Term.Name)
                .Where(t => t.ToCharArray().Where(c => char.IsUpper(c)).Count() > 1).ToList();
        }


    }
}
