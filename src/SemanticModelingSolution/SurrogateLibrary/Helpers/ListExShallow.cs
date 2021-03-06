using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary.Helpers
{
    /// <summary>
    /// A list that implements a shallow comparison
    /// It is true just if the number of elements are the same
    /// This is done for the SurrogateType.Properties, 
    /// otherwise there is a cycle (StackOverflow) on == operator
    /// generated by the record
    /// </summary>
    internal class ListExShallow<T> : List<T>, IEquatable<ListExShallow<T>>
    {
        public ListExShallow() : base() { }

        public ListExShallow(int capacity) : base(capacity) { }

        public ListExShallow(IEnumerable<T> collection) : base(collection) { }

        public static bool operator ==(ListExShallow<T> obj1, ListExShallow<T> obj2)
        {
            if (object.ReferenceEquals(obj1, null)) return false;
            return obj1.Equals(obj2);
        }

        public static bool operator !=(ListExShallow<T> obj1, ListExShallow<T> obj2) => !(obj1 == obj2);

        /// <summary>
        /// https://stackoverflow.com/a/8094931/492913
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 23;
                foreach (var item in this)
                {
                    hash = hash * 31 + item.GetHashCode();
                }
                return hash;
            }
        }

        public override bool Equals(object obj) => Equals(obj as ListExShallow<T>);

        public bool Equals(ListExShallow<T> other)
        {
            if (object.ReferenceEquals(other, null)) return false;
            if (object.ReferenceEquals(this, other)) return true;

            // =================================================
            // ==== Attention: this is a shallow comparison ====
            // =================================================
            return this.Count == other.Count;
        }
    }
}
