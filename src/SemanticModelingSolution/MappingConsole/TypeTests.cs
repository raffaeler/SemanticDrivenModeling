using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.Json;

using SurrogateLibrary;
using SurrogateLibrary.Helpers;
using System.Diagnostics;

namespace MappingConsole
{
    public class TA { public TA A { get; set; } public TB B { get; set; } public int X { get; set; } }
    public class TB { public TA A { get; set; } public TB B { get; set; } public int Y { get; set; } }

    public record XX1(int id, YY1 y);
    public record YY1(int id, ZZ1 z);
    public record ZZ1(int id, string Name);

    public record Info(int Id, string Name);


    internal class TypeTests
    {
        public int X1 { get; set; }
        public int X2 { get; init; }
        public int X3 { get; }
        public int X4 { get; private set; }


        public void Run()
        {
            // record "with" => shallow copy
            //var z1 = new XX1(1, new YY1(1, new ZZ1(1, "x1")));
            //var z2 = z1 with { id=2 };
            //Debug.Assert(z1 != z2);
            //Debug.Assert(object.ReferenceEquals(z1.y, z2.y));
            //Debug.Assert(object.ReferenceEquals(z1.y.z, z2.y.z));

            Test1();
            Test1WithInfo();
            Test2();
        }

        private void Test2()
        {
            var options = new JsonSerializerOptions()
            {
                IgnoreReadOnlyProperties = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            };

            TypeSystem ts = new TypeSystem();
            ts.GetOrCreate(typeof(SimpleDomain1.Order));
            ts.GetOrCreate(typeof(TA));
            ts.UpdateCache();

            var tsJson = JsonSerializer.Serialize(ts, options);
            var tsClone = JsonSerializer.Deserialize<TypeSystem>(tsJson);
            Debug.Assert(ts != tsClone);

            tsClone.UpdateCache();
            Debug.Assert(ts == tsClone);

            if (!ts.TryGetSurrogateTypeByName("SimpleDomain1.Order", out var entryPointOrder)) Debug.Fail("not found");
            if (!ts.TryGetSurrogateTypeByName("MappingConsole.TA", out var entryPointTA)) Debug.Fail("not found");
            foreach (var p in entryPointOrder.Properties.Values)
            {
                Console.WriteLine(p.ToString());
            }

            //var allPropertiesTA = entryPointTA.FlattenHierarchy().ToList();
            var allPropertiesOrder = entryPointOrder.FlattenHierarchy().ToList();
            GC.Collect();
            GC.Collect();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            foreach (var p in allPropertiesOrder)
            {
                Console.WriteLine(p.GetLeaf().Path);
            }

            var path5 = allPropertiesOrder[5];
            path5.UpdateCache(ts);
            var json5 = JsonSerializer.Serialize(path5);
            var path5clone = JsonSerializer.Deserialize<NavigationPath<SurrogateLibrary.VoidType>>(json5);
            path5clone.UpdateCache(ts);
            Debug.Assert(path5 == path5clone);
            Debug.Assert(path5.Equals(path5clone));
            var hc5 = path5.GetHashCode();
            var hc5clone = path5clone.GetHashCode();


        }

        private void Test1()
        {
            var t1 = typeof(List<string>);
            var t2 = typeof(IDictionary<string, Action<int>>);
            var t3 = typeof(List<>);
            var t4 = typeof(IDictionary<,>);
            var t5 = typeof(string);
            var t6 = typeof(Program);

            var q1 = t1.ToStringEx(true);
            var q2 = t2.ToStringEx(true);
            var q3 = t3.ToStringEx(true);
            var q4 = t4.ToStringEx(true);
            var q5 = t5.ToStringEx(true);
            var q6 = t6.ToStringEx(true);

            var ts = new TypeSystem();

            var s1 = ts.GetOrCreate(t1);
            var s2 = ts.GetOrCreate(t2);
            var s3 = ts.GetOrCreate(t3);
            var s4 = ts.GetOrCreate(t4);
            var s5 = ts.GetOrCreate(t5);
            var s6 = ts.GetOrCreate(t6);
            var s7 = ts.GetOrCreate(typeof(TA));

            var options = new JsonSerializerOptions() { IgnoreReadOnlyProperties = true };
            var json = JsonSerializer.Serialize(s1, options);
            var x1 = JsonSerializer.Deserialize<SurrogateLibrary.SurrogateType<SurrogateLibrary.VoidType>>(json);

            var tsJson = JsonSerializer.Serialize(ts, options);
            var tsClone = JsonSerializer.Deserialize<TypeSystem>(tsJson);

            var diff1 = ts.Types.Except(tsClone.Types).ToList();
            var diff2 = tsClone.Types.Except(ts.Types).ToList();
            var compProp = tsClone.Types[1].Properties.Equals(ts.Types[1].Properties);
            var comp = ts == tsClone;
            Debug.Assert(comp);

        }

        private void Test1WithInfo()
        {
            var t1 = typeof(List<string>);
            var t2 = typeof(IDictionary<string, Action<int>>);
            var t3 = typeof(List<>);
            var t4 = typeof(IDictionary<,>);
            var t5 = typeof(string);
            var t6 = typeof(Program);

            var q1 = t1.ToStringEx(true);
            var q2 = t2.ToStringEx(true);
            var q3 = t3.ToStringEx(true);
            var q4 = t4.ToStringEx(true);
            var q5 = t5.ToStringEx(true);
            var q6 = t6.ToStringEx(true);

            var ts = new TypeSystem<Info>();

            var s1 = ts.GetOrCreate(t1);
            var s2 = ts.GetOrCreate(t2);
            var s3 = ts.GetOrCreate(t3);
            var s4 = ts.GetOrCreate(t4);
            var s5 = ts.GetOrCreate(t5);
            var s6 = ts.GetOrCreate(t6);
            var s7 = ts.GetOrCreate(typeof(TA));
            s6.SetInfo(new Info(10, "boh"));
            s7.SetInfo(new Info(1, "Raf"));

            var options = new JsonSerializerOptions() { /*IgnoreReadOnlyProperties = true*/ };
            var json = JsonSerializer.Serialize(s7, options);
            var x1 = JsonSerializer.Deserialize<SurrogateLibrary.SurrogateType<Info>>(json);

            var tsJson = JsonSerializer.Serialize(ts, options);
            var tsClone = JsonSerializer.Deserialize<TypeSystem<Info>>(tsJson);

            var diff1 = ts.Types.Except(tsClone.Types).ToList();
            var diff2 = tsClone.Types.Except(ts.Types).ToList();
            var compProp = tsClone.Types[1].Properties.Equals(ts.Types[1].Properties);
            var compTypes = tsClone.Types.Equals(ts.Types);
            var compTypeNames = tsClone.TypesByFullName.Equals(ts.TypesByFullName);
            var comp = ts == tsClone;
            Debug.Assert(comp);

        }
        static void EqualityBrokenInCSharp()
        {
            // equality is broken in C#
            var l1 = new ListEx<int>(new int[] { 1, 2 });
            var l2 = new ListEx<int>(new int[] { 1, 2 });
            var r1 = (IReadOnlyList<int>)l1;
            var r2 = (IReadOnlyList<int>)l2;
            var c1 = l1 == l2;      // true
            var c2 = r1 == r2;      // false :-(
            var c3 = r1.Equals(r2); // true
        }


    }
}
