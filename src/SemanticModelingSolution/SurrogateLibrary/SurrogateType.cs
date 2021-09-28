using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

using SurrogateLibrary.Helpers;

namespace SurrogateLibrary
{
    public record SurrogateType
    {
        private Type _type;
        private DictionaryExShallow<UInt64, SurrogateProperty> _properties = new();
        private DictionaryExShallow<UInt64, SurrogateProperty> _incoming = new();

        internal ListEx<UInt64> _propertyIndexes;

        /// <summary>
        /// Only Basic Types
        /// </summary>
        internal SurrogateType(UInt64 index, Type type)
        {
            var (flags, _, _) = type.Classify();
            Index = index;
            AssemblyName = ITypeSystem.PlaceholderForSystemAssemblyName; //type.Assembly.GetName().Name;
            Namespace = type.Namespace;
            Name = type.Name;
            FullName = GetFullName(type);
            TypeFields1 = flags;
            _propertyIndexes = new ListEx<UInt64>();
        }

        [JsonConstructor]
        public SurrogateType(UInt64 index, string assemblyName,
            string @namespace, string name, string fullName, TypeFields typeFields1, UInt64 innerTypeIndex1, UInt64 innerTypeIndex2,
            IReadOnlyList<UInt64> propertyIndexes) =>
            (Index, AssemblyName, Namespace, Name, FullName, TypeFields1, InnerTypeIndex1, InnerTypeIndex2, PropertyIndexes) =
            (index, assemblyName, @namespace, name, fullName, typeFields1, innerTypeIndex1, innerTypeIndex2, propertyIndexes);

        public UInt64 Index { get; init; }
        public string AssemblyName { get; init; }
        public string Namespace { get; init; }
        public string Name { get; init; }

        /// <summary>
        /// In this type system, FullName must be unique as it used in dictionaries
        /// </summary>
        public string FullName { get; init; }
        public TypeFields TypeFields1 { get; init; }
        public UInt64 InnerTypeIndex1 { get; init; }
        public UInt64 InnerTypeIndex2 { get; init; }

        public IReadOnlyList<UInt64> PropertyIndexes
        {
            get => _propertyIndexes;
            init => _propertyIndexes = value == null ? null : new ListEx<UInt64>(value);
        }

        [JsonIgnore]
        public IReadOnlyDictionary<UInt64, SurrogateProperty> Properties => _properties;

        [JsonIgnore]
        public IReadOnlyDictionary<UInt64, SurrogateProperty> Incoming => _incoming;

        [JsonIgnore]
        public SurrogateType InnerType1 { get; private set; }

        [JsonIgnore]
        public SurrogateType InnerType2 { get; private set; }

        public bool IsBasicType => this.Index < KnownTypes.MaxIndexForBasicTypes;

        public static string GetFullName(Type type) => type.ToStringEx(true);

        /// <summary>
        /// This only works when the Type is already loaded in memory
        /// otherwise an Exception will be thrown;
        /// </summary>
        public Type GetOriginalType()
        {
            var assemblyName = AssemblyName == ITypeSystem.PlaceholderForSystemAssemblyName
                ? null : AssemblyName;
            return _type ??= TypeHelper.GetEntityType(FullName, assemblyName);
        }

        public bool Is(Type type)
        {
            if (FullName == GetFullName(type)) return true;
            return false;
        }

        public override string ToString()
        {
            return $"[{Index}] {FullName}";
        }

        internal void UpdateCache(ITypeSystem typeSystem)
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
    }
}
