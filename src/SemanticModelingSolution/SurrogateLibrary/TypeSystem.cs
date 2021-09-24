using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public class TypeSystem
    {
        private ConcurrentDictionary<UInt64, SurrogateType> _types;
        private ConcurrentDictionary<string, SurrogateType> _typesByUniqueName;
        private ConcurrentDictionary<UInt64, SurrogatePropertyInfo> _properties;
        private UInt64 _typeIndex = 0;
        private UInt64 _propertyIndex = 0;

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
            _types = new ConcurrentDictionary<UInt64, SurrogateType>(types);
            _properties = new ConcurrentDictionary<UInt64, SurrogatePropertyInfo>(properties);
            _typesByUniqueName = new ConcurrentDictionary<string, SurrogateType>();
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
            var unique = SurrogateType.GetUniqueName(type.Namespace, type.Name);
            if (!_typesByUniqueName.TryGetValue(unique, out SurrogateType surrogate))
            {
                var newIndex = Interlocked.Increment(ref _typeIndex);
                surrogate = new SurrogateType(newIndex,
                    type.Assembly.GetName().Name, type.Namespace, type.Name, null);
                _types[newIndex] = surrogate;
                _typesByUniqueName[surrogate.UniqueName] = surrogate;

                // Don't walk the graph when a type belongs to the System library
                List<SurrogatePropertyInfo> properties = new();
                if (!type.Namespace.StartsWith("System"))
                {
                    var allProperties = type.GetProperties(DefaultBindings);
                    foreach (var pi in allProperties)
                    {
                        if (!pi.CanRead || !pi.CanWrite) continue;
                        var sp = pi.ToSurrogate(Interlocked.Increment(ref _propertyIndex), this, newIndex);
                        _properties[sp.Index] = sp;
                        properties.Add(sp);
                    }
                }

                surrogate._properties = properties;
            }

            return surrogate;
        }

        public SurrogateType Get(UInt64 index) => Types[index];

        public override string ToString()
        {
            Debug.Assert(_types.Count == _typesByUniqueName.Count);

            var basicTypes = Types.Count(t => t.Key < KnownTypes.MaxIndexForBasicTypes);
            var otherTypes = Types.Count(t => t.Key > KnownTypes.MaxIndexForBasicTypes);
            return $"Basic Types={basicTypes}, Other s={otherTypes}, LastIndex={_typeIndex}";
        }
    }
}
