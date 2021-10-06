using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaterializerLibrary
{
    public interface IContainer
    {
        Type Type { get; }
    }

    internal interface IContainerDebug
    {
        object ObjectItem { get; }
    }

    internal class Container<K> : IContainer, IContainerDebug
    {
        public Container(K item) { Item = item; }
        public K Item { get; set; }
        public Type Type => typeof(K);
        public object ObjectItem => Item;
    }
}
