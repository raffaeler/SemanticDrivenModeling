using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SurrogateLibrary.Tests
{
    [TestClass]
    public class TypeSystemTests
    {
        public class TA { public TA A { get; set; } public TB B { get; set; } public int X { get; set; } }
        public class TB { public TA A { get; set; } public TB B { get; set; } public int Y { get; set; } }
        public record Info(int Id, string Name);

        public record EvaluationMock(int Score);
        public record NavigationPairMock(NavigationSegment<VoidType> Source, NavigationSegment<VoidType> Target,
            EvaluationMock Evaluation);



        [TestMethod]
        public void TestTyped()
        {
            var ts = new TypeSystem<Info>("id1");

            var t1 = typeof(List<string>);
            var t2 = typeof(IDictionary<string, Action<int>>);
            var t3 = typeof(List<>);
            var t4 = typeof(IDictionary<,>);
            var t5 = typeof(string);
            var t6 = typeof(TypeSystemTests);

            var s1 = ts.GetOrCreate(t1);
            var s2 = ts.GetOrCreate(t2);
            var s3 = ts.GetOrCreate(t3);
            var s4 = ts.GetOrCreate(t4);
            var s5 = ts.GetOrCreate(t5);
            var s6 = ts.GetOrCreate(t6);
            var s7 = ts.GetOrCreate(typeof(TA));
            s6.SetInfo(new Info(10, "boh"));
            s7.SetInfo(new Info(1, "Raf"));

            var tsJson = JsonSerializer.Serialize(ts);
            var tsClone = JsonSerializer.Deserialize<TypeSystem<Info>>(tsJson);

            var diff1 = ts.Types.Except(tsClone.Types).ToList();
            var diff2 = tsClone.Types.Except(ts.Types).ToList();
            Assert.AreEqual(0, diff1.Count);
            Assert.AreEqual(0, diff2.Count);

            Assert.IsTrue(tsClone.Types[1].Properties.Equals(ts.Types[1].Properties));
            Assert.IsTrue(tsClone.Types.Equals(ts.Types));
            Assert.IsTrue(tsClone.TypesByFullName.Equals(ts.TypesByFullName));
            Assert.IsTrue(ts == tsClone);

        }

        [TestMethod]
        public void TestUntyped()
        {
            var ts = new TypeSystem("id1");

            var t1 = typeof(List<string>);
            var t2 = typeof(IDictionary<string, Action<int>>);
            var t3 = typeof(List<>);
            var t4 = typeof(IDictionary<,>);
            var t5 = typeof(string);
            var t6 = typeof(TypeSystemTests);

            var s1 = ts.GetOrCreate(t1);
            var s2 = ts.GetOrCreate(t2);
            var s3 = ts.GetOrCreate(t3);
            var s4 = ts.GetOrCreate(t4);
            var s5 = ts.GetOrCreate(t5);
            var s6 = ts.GetOrCreate(t6);
            var s7 = ts.GetOrCreate(typeof(TA));

            var tsJson = JsonSerializer.Serialize(ts);
            var tsClone = JsonSerializer.Deserialize<TypeSystem>(tsJson);

            var diff1 = ts.Types.Except(tsClone.Types).ToList();
            var diff2 = tsClone.Types.Except(ts.Types).ToList();
            Assert.AreEqual(0, diff1.Count);
            Assert.AreEqual(0, diff2.Count);

            Assert.IsTrue(tsClone.Types[1].Properties.Equals(ts.Types[1].Properties));
            Assert.IsTrue(tsClone.Types.Equals(ts.Types));
            Assert.IsTrue(tsClone.TypesByFullName.Equals(ts.TypesByFullName));
            Assert.IsTrue(ts == tsClone);

        }

        [TestMethod]
        public void TestNavigation()
        {
            TypeSystem ts = new TypeSystem("id1");
            ts.GetOrCreate(typeof(SimpleDomain1.Order));
            ts.UpdateCache();
            if (!ts.TryGetSurrogateTypeByName("SimpleDomain1.Order", out var entryPointOrder)) Assert.Fail("not found");

            var allPropertiesOrder = entryPointOrder.FlattenHierarchy().ToList();

            var paths = allPropertiesOrder.Select(p => p.GetLeaf().Path).ToList();
            Assert.IsTrue(paths.All(p => p.StartsWith("Order.")));
            Assert.IsTrue(paths.All(p => p.Length > 6));

            var path5 = allPropertiesOrder[5];
            path5.UpdateCache(ts);
            var json5 = JsonSerializer.Serialize(path5);
            var path5clone = JsonSerializer.Deserialize<NavigationSegment<VoidType>>(json5);
            path5clone.UpdateCache(ts);
            Assert.AreEqual(path5, path5clone);
            Assert.IsTrue(path5.Equals(path5clone));
            var hc5 = path5.GetHashCode();
            var hc5clone = path5clone.GetHashCode();
            Assert.AreEqual(hc5, hc5clone);
        }
    }
}
