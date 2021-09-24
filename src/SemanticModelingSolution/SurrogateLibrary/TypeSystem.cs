using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SurrogateLibrary.Helpers;

namespace SurrogateLibrary
{
    public record TypeSystem
    {
        private ConcurrentDictionaryEx<UInt64, SurrogateType> _types;
        private ConcurrentDictionaryEx<string, SurrogateType> _typesByUniqueName;
        private ConcurrentDictionaryEx<UInt64, SurrogatePropertyInfo> _properties;
        private UInt64 _typeIndex = 0;
        private UInt64 _propertyIndex = 0;

        /// <summary>
        /// This is used in the basic types belonging to CoreLib to replace
        /// the assembly name. This may change in different .NET implementations
        /// but it is meaningless to classify our type system
        /// On other types the AssemblyName is used to create the type with Type.GetType.
        /// </summary>
        public static readonly string PlaceholderForSystemAssemblyName = null;

        public TypeSystem()
        {
            _types = new();
            _typesByUniqueName = new();
            _properties = new();
            foreach (var type in KnownTypes.BasicTypes)
            {
                if (type.Index > KnownTypes.MaxIndexForBasicTypes)
                {
                    throw new ArgumentException($"The basic types key index must never exceed {KnownTypes.MaxIndexForBasicTypes}");
                }
                _types[type.Index] = type;
                _typesByUniqueName[type.UniqueName] = type;
            }

            _typeIndex = KnownTypes.MaxIndexForBasicTypes;
            _propertyIndex = KnownTypes.MaxIndexForBasicProperties;
        }

        public BindingFlags DefaultBindings { get; set; } =
            BindingFlags.Instance | BindingFlags.Public;

        [System.Text.Json.Serialization.JsonConstructor]
        public TypeSystem(IReadOnlyDictionary<UInt64, SurrogateType> types,
            IReadOnlyDictionary<UInt64, SurrogatePropertyInfo> properties)
        {
            _types = new ConcurrentDictionaryEx<UInt64, SurrogateType>(types);
            _properties = new ConcurrentDictionaryEx<UInt64, SurrogatePropertyInfo>(properties);
            _typesByUniqueName = new ConcurrentDictionaryEx<string, SurrogateType>();
            UInt64 typeIndex = 0;
            foreach (var type in types.Values)
            {
                _typesByUniqueName[type.UniqueName] = type;
                typeIndex = Math.Max(typeIndex, type.Index);
            }

            _typeIndex = typeIndex;
            if (properties == null)
            {
                _properties = new();
            }

            if (properties.Count == 0)
            {
                _propertyIndex = KnownTypes.MaxIndexForBasicProperties;
            }
            else
            {
                _propertyIndex = properties.Values.Select(v => v.Index).Max();
            }

        }

        public IReadOnlyDictionary<UInt64, SurrogateType> Types => _types;
        public IReadOnlyDictionary<UInt64, SurrogatePropertyInfo> Properties => _properties;
        public IReadOnlyDictionary<string, SurrogateType> TypesByUniqueName => _typesByUniqueName;

        public SurrogateType GetOrCreate(Type type)
        {
            if (type == null) return null;
            var unique = SurrogateType.GetFullName(type);
            if (!_typesByUniqueName.TryGetValue(unique, out SurrogateType surrogate))
            {
                var newIndex = Interlocked.Increment(ref _typeIndex);
                var (flags, inner1, inner2) = type.Classify();
                var innerSurrogate1 = GetOrCreate(inner1);
                var innerSurrogate2 = GetOrCreate(inner2);

                surrogate = new SurrogateType(newIndex,
                    type.Assembly.GetName().Name, type.Namespace, type.Name, unique,
                    flags, innerSurrogate1?.Index ?? 0, innerSurrogate2?.Index ?? 0, null);
                _types[newIndex] = surrogate;
                _typesByUniqueName[surrogate.UniqueName] = surrogate;

                // Don't walk the graph when a type belongs to the System library
                ListEx<UInt64> propertyIndexes = new();
                if (!type.Namespace.StartsWith("System"))
                {
                    var allProperties = type.GetProperties(DefaultBindings);
                    foreach (var pi in allProperties)
                    {
                        if (!pi.CanRead || !pi.CanWrite) continue;
                        var sp = pi.ToSurrogate(Interlocked.Increment(ref _propertyIndex), this, newIndex);
                        _properties[sp.Index] = sp;
                        propertyIndexes.Add(sp.Index);
                    }
                }

                surrogate._propertyIndexes = propertyIndexes;
            }

            return surrogate;
        }

        public SurrogateType GetSurrogateType(UInt64 index) => index == 0 ? null : Types[index];
        public bool TryGetSurrogateType(UInt64 index, out SurrogateType surrogateType)
            => Types.TryGetValue(index, out surrogateType);

        public SurrogatePropertyInfo GetSurrogatePropertyInfo(UInt64 index) => Properties[index];
        public bool TryGetSurrogatePropertyInfo(UInt64 index, out SurrogatePropertyInfo surrogatePropertyInfo)
            => Properties.TryGetValue(index, out surrogatePropertyInfo);

        public void UpdateCache()
        {
            foreach (var type in Types.Values)
            {
                type.UpdateCache(this);
            }
        }

        public override string ToString()
        {
            Debug.Assert(_types.Count == _typesByUniqueName.Count);

            var basicTypes = Types.Count(t => t.Key < KnownTypes.MaxIndexForBasicTypes);
            var otherTypes = Types.Count(t => t.Key > KnownTypes.MaxIndexForBasicTypes);
            return $"Basic types={basicTypes}, Other types={otherTypes}, Properties={Properties.Count}, LastTypeIndex={_typeIndex}, LastPropIndex={_propertyIndex}";
        }
    }
}
