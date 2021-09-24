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
    internal class TypeTests
    {
        public int X1 { get; set; }
        public int X2 { get; init; }
        public int X3 { get; }
        public int X4 { get; private set; }

        public class TA { public TA A { get; set; } public TB B { get; set; } }
        public class TB { public TA A { get; set; } public TB B { get; set; } }

        public void Run()
        {
            //Test1();
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
            ts.UpdateCache();

            var tsJson = JsonSerializer.Serialize(ts, options);
            var tsClone = JsonSerializer.Deserialize<TypeSystem>(tsJson);
            Debug.Assert(ts != tsClone);

            tsClone.UpdateCache();
            Debug.Assert(ts == tsClone);

            if (!ts.TryGetSurrogateTypeByName("SimpleDomain1.Order", out var entryPoint)) Debug.Fail("not found");
            foreach (var p in entryPoint.Properties.Values)
            {
                Console.WriteLine(p.ToString());
            }

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
            var x1 = JsonSerializer.Deserialize<SurrogateLibrary.SurrogateType>(json);

            var tsJson = JsonSerializer.Serialize(ts, options);
            var tsClone = JsonSerializer.Deserialize<TypeSystem>(tsJson);

            var diff1 = ts.Types.Except(tsClone.Types).ToList();
            var diff2 = tsClone.Types.Except(ts.Types).ToList();
            var compProp = tsClone.Types[1].Properties.Equals(ts.Types[1].Properties);
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
