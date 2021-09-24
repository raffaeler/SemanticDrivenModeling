using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary.Helpers
{
    internal class DictionaryEx<TKey, TValue> : Dictionary<TKey, TValue>, IEquatable<DictionaryEx<TKey, TValue>>
    {
        public DictionaryEx() : base()
        {
        }

        public DictionaryEx(int capacity) : base(capacity)
        {
        }

        public DictionaryEx(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection)
        {
        }

        public DictionaryEx(IEqualityComparer<TKey> comparer) : base(comparer)
        {
        }

        public DictionaryEx(IDictionary<TKey, TValue> dictionary) : base(dictionary)
        {
        }

        public DictionaryEx(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
        {
        }

        public DictionaryEx(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public DictionaryEx(IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> comparer) : base(collection, comparer)
        {
        }

        public DictionaryEx(IDictionary<TKey, TValue> dictionary,
            IEqualityComparer<TKey> comparer) : base(dictionary, comparer)
        {
        }

        public static bool operator ==(DictionaryEx<TKey, TValue> obj1,
            DictionaryEx<TKey, TValue> obj2)
        {
            if (object.ReferenceEquals(obj1, null)) return false;
            return obj1.Equals(obj2);
        }

        public static bool operator !=(DictionaryEx<TKey, TValue> obj1,
            DictionaryEx<TKey, TValue> obj2)
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
            return Equals(obj as DictionaryEx<TKey, TValue>);
        }

        public bool Equals(DictionaryEx<TKey, TValue> other)
        {
            if (object.ReferenceEquals(other, null)) return false;
            if (object.ReferenceEquals(this, other)) return true;

            return this.Intersect(other).Count() == this.Count;
        }


    }
}
