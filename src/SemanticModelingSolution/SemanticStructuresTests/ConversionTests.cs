using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using ConversionLibrary;
using ConversionLibrary.Converters;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SemanticStructuresTests
{
    [TestClass]
    public class ConversionTests
    {
        [TestMethod]
        public void Conversions1()
        {
            var context = new ConversionContext();
            var conv = new ConversionEngine(context);
            conv.Conversions.TryGetValue(typeof(string), out IConversion c);
            var dc = (ToStringConversion)c;
            Assert.AreEqual("12.4", dc.From(12.4));
            Assert.AreEqual("{313245C9-AE46-426F-AA5E-9781C70FD656}".ToLower(), dc.From(Guid.Parse("{313245C9-AE46-426F-AA5E-9781C70FD656}")));
           

            conv.Conversions.TryGetValue(typeof(Guid), out IConversion g);
            var gg = (ToGuidConversion)g;
            Assert.AreEqual(Guid.Parse("{313245C9-AE46-426F-AA5E-9781C70FD656}"), gg.From("{313245C9-AE46-426F-AA5E-9781C70FD656}"));


        }
    }
}
