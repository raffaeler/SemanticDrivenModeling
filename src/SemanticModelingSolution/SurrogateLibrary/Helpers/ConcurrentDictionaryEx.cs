using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary.Helpers
{
    internal class ConcurrentDictionaryEx<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, IEquatable<ConcurrentDictionaryEx<TKey, TValue>>
    {
        public ConcurrentDictionaryEx() : base()
        {
        }

        public ConcurrentDictionaryEx(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection)
        {
        }

        public ConcurrentDictionaryEx(IEqualityComparer<TKey> comparer) : base(comparer)
        {
        }

        public ConcurrentDictionaryEx(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
        {
        }

        public ConcurrentDictionaryEx(IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> comparer) : base(collection, comparer)
        {
        }

        public ConcurrentDictionaryEx(int concurrencyLevel, int capacity,
            IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer)
        {
        }

        public ConcurrentDictionaryEx(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> comparer) : base(concurrencyLevel, collection, comparer)
        {
        }

        public static bool operator ==(ConcurrentDictionaryEx<TKey, TValue> obj1,
            ConcurrentDictionaryEx<TKey, TValue> obj2)
        {
            if (object.ReferenceEquals(obj1, null)) return false;
            return obj1.Equals(obj2);
        }

        public static bool operator !=(ConcurrentDictionaryEx<TKey, TValue> obj1,
            ConcurrentDictionaryEx<TKey, TValue> obj2)
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
            return Equals(obj as ConcurrentDictionaryEx<TKey, TValue>);
        }

        public bool Equals(ConcurrentDictionaryEx<TKey, TValue> other)
        {
            if (object.ReferenceEquals(other, null)) return false;
            if (object.ReferenceEquals(this, other)) return true;

            return this.Intersect(other).Count() == this.Count;
        }


    }
}
