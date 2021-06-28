using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using FuzzySharp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SemanticGlossaryGenerator;

using SemanticLibrary;
using SemanticLibrary.Helpers;

namespace SemanticStructuresTests
{
    [TestClass]
    public class FuzzyTests
    {
        [TestMethod]
        public void Similarity()
        {
            int r0 = Fuzz.Ratio("door", "door");
            int r1 = Fuzz.Ratio("door", "rood");
            int r2 = Fuzz.Ratio("door", "doors");
            int r3 = Fuzz.Ratio("door", "indoor");
            int r4 = Fuzz.Ratio("door", "Door");
            int r5 = Fuzz.Ratio("InDoor", "Indoor");
            int r6 = Fuzz.Ratio("door", "poor");

            int wr0 = Fuzz.WeightedRatio("door", "door");
            int wr1 = Fuzz.WeightedRatio("door", "rood");
            int wr2 = Fuzz.WeightedRatio("door", "doors");
            int wr3 = Fuzz.WeightedRatio("door", "indoor");
            int wr4 = Fuzz.WeightedRatio("door", "Door");
            int wr5 = Fuzz.WeightedRatio("InDoor", "Indoor");
            int wr6 = Fuzz.WeightedRatio("door", "poor");

        }

    }
}
