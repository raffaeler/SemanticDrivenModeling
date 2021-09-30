using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

using SurrogateLibrary.Helpers;

namespace SurrogateLibrary
{
    public record SurrogateType<T> : SurrogateType
    {
        private DictionaryExShallow<UInt64, SurrogateProperty<T>> _properties = new();
        private DictionaryExShallow<UInt64, SurrogateProperty<T>> _incoming = new();

        /// <summary>
        /// Only Basic Types
        /// </summary>
        internal SurrogateType(UInt64 index, Type type) : base(index, type)
        {
        }

        [JsonConstructor]
        public SurrogateType(UInt64 index, string assemblyName,
            string @namespace, string name, string fullName, TypeFields typeFields1,
            UInt64 innerTypeIndex1, UInt64 innerTypeIndex2, IReadOnlyList<UInt64> propertyIndexes, T info)
            : base(index, assemblyName,
                  @namespace, name, fullName, typeFields1,
                  innerTypeIndex1, innerTypeIndex2, propertyIndexes) =>
            Info = info;


        [JsonIgnore]
        public IReadOnlyDictionary<UInt64, SurrogateProperty<T>> Properties => _properties;

        [JsonIgnore]
        public IReadOnlyDictionary<UInt64, SurrogateProperty<T>> Incoming => _incoming;

        [JsonIgnore]
        public SurrogateType<T> InnerType1 { get; private set; }

        [JsonIgnore]
        public SurrogateType<T> InnerType2 { get; private set; }

        public T Info { get; internal set; }


        public void UpdateCache(TypeSystemBase<T> typeSystem)
        {
            InnerType1 = typeSystem.GetSurrogateType(InnerTypeIndex1);
            InnerType2 = typeSystem.GetSurrogateType(InnerTypeIndex2);

            _properties.Clear();
            foreach (var property in PropertyIndexes)
            {
                var prop = typeSystem.GetSurrogateProperty(property);
                _properties[prop.Index] = prop;
            }

            foreach (var property in typeSystem.Properties)
            {
                var prop = property.Value;
                if (prop.PropertyTypeIndex == Index)
                    _incoming[prop.Index] = prop;
            }
        }

        public override string ToString()
        {
            return $"[{Index}] {FullName}";
        }

    }
}
