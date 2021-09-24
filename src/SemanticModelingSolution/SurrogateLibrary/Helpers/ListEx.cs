using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary.Helpers
{
    public class ListEx<T> : List<T>, IEquatable<ListEx<T>>
    {
        public ListEx() : base()
        {
        }

        public ListEx(int capacity) : base(capacity)
        {
        }

        public ListEx(IEnumerable<T> collection) : base(collection)
        {
        }

        public static bool operator ==(ListEx<T> obj1, ListEx<T> obj2)
        {
            if (object.ReferenceEquals(obj1, null)) return false;
            return obj1.Equals(obj2);
        }

        public static bool operator !=(ListEx<T> obj1, ListEx<T> obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        /// https://stackoverflow.com/a/8094931/492913
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 19;
                foreach (var item in this)
                {
                    hash = hash * 31 + item.GetHashCode();
                }
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ListEx<T>);
        }

        public bool Equals(ListEx<T> other)
        {
            if (object.ReferenceEquals(other, null)) return false;
            if (object.ReferenceEquals(this, other)) return true;

            return this.SequenceEqual(other);
        }
    }
}
