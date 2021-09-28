﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary.Helpers
{
    /// <summary>
    /// A dictionary that implements a shallow comparison
    /// It is true just if the number of elements are the same
    /// This is done for the SurrogateType.Properties, 
    /// otherwise there is a cycle (StackOverflow) on == operator
    /// generated by the record
    /// </summary>
    internal class DictionaryExShallow<TKey, TValue> : Dictionary<TKey, TValue>, IEquatable<DictionaryExShallow<TKey, TValue>>
    {
        public DictionaryExShallow() : base() { }

        public DictionaryExShallow(int capacity) : base(capacity) { }

        public DictionaryExShallow(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) { }

        public DictionaryExShallow(IEqualityComparer<TKey> comparer) : base(comparer) { }

        public DictionaryExShallow(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

        public DictionaryExShallow(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        public DictionaryExShallow(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public DictionaryExShallow(IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> comparer) : base(collection, comparer) { }

        public DictionaryExShallow(IDictionary<TKey, TValue> dictionary,
            IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }

        public static bool operator ==(DictionaryExShallow<TKey, TValue> obj1,
            DictionaryExShallow<TKey, TValue> obj2)
        {
            if (object.ReferenceEquals(obj1, null)) return false;
            return obj1.Equals(obj2);
        }

        public static bool operator !=(DictionaryExShallow<TKey, TValue> obj1,
            DictionaryExShallow<TKey, TValue> obj2)
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
                int hash = 73;
                foreach (var item in this)
                {
                    hash = hash * 31 + item.GetHashCode();
                }
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DictionaryExShallow<TKey, TValue>);
        }

        public bool Equals(DictionaryExShallow<TKey, TValue> other)
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