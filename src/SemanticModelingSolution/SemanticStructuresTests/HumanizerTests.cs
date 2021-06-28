using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Humanizer;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SemanticGlossaryGenerator;

using SemanticLibrary;
using SemanticLibrary.Helpers;

namespace SemanticStructuresTests
{
    [TestClass]
    public class HumanizerTests
    {
        [TestMethod]
        public void HumanizerBasics()
        {
            Assert.AreEqual("shoes", "shoe".Pluralize(false));
            Assert.AreEqual("shoes", "shoes".Pluralize(false));
            Assert.AreEqual("toes", "toes".Pluralize(false));
            Assert.AreEqual("toes", "toe".Pluralize(false));
            Assert.AreEqual("doors", "door".Pluralize(false));
            Assert.AreEqual("doors", "doors".Pluralize(false));

            Assert.AreEqual("shoe", "shoe".Singularize(false));
            Assert.AreEqual("shoe", "shoes".Singularize(false));
            //Assert.AreEqual("toe", "toes".Singularize(false));        // library bug
            Assert.AreEqual("toe", "toe".Singularize(false));
            Assert.AreEqual("door", "door".Singularize(false));
            Assert.AreEqual("door", "doors".Singularize(false));


        }
    }
}
