using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MappingConsole
{
    internal class BoxingTests
    {
        public record R1(string Name);
        public record R2(string Name);

        public interface IContainer { }

        public struct Container<T1> : IContainer
        {
            public Container(T1 item) { Item = item; }

            public T1 Item { get; set; }
        }

        public Dictionary<string, IContainer> Instances;


        public void Test()
        {
            Instances = new();
            Instances["R1"] = new Container<R1>(new R1("Raf"));
            Instances["R2"] = new Container<R2>(new R2("Dan"));

            Instances.TryGetValue("R1", out var c1);
            Instances.TryGetValue("R2", out var c2);

            var n1 = ((Container<R1>)c1).Item.Name;
            var n2 = ((Container<object>)c2).Item;
        }
    }
}
