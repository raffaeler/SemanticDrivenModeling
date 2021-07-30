﻿using System;
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
            conv.ConversionTypes.TryGetValue(typeof(string), out IConversion c);
            var dc = (ToStringConversion)c;
            Assert.AreEqual("12.4", dc.From(12.4));
            Assert.AreEqual("{313245C9-AE46-426F-AA5E-9781C70FD656}".ToLower(), dc.From(Guid.Parse("{313245C9-AE46-426F-AA5E-9781C70FD656}")));


            conv.ConversionTypes.TryGetValue(typeof(Guid), out IConversion g);
            var gg = (ToGuidConversion)g;
            Assert.AreEqual(Guid.Parse("{313245C9-AE46-426F-AA5E-9781C70FD656}"), gg.From("{313245C9-AE46-426F-AA5E-9781C70FD656}"));

            f((Int32?)null);
        }

        void f(Int16 a) { }
        void f(Int16? a) { }
        void f(UInt16 a) { }
        void f(UInt16? a) { }
        void f(Int32 a) { }
        void f(Int32? a) { }
        void f(UInt32 a) { }
        void f(UInt32? a) { }
        void f(Int64 a) { }
        void f(Int64? a) { }
        void f(UInt64 a) { }
        void f(UInt64? a) { }


    }
}
