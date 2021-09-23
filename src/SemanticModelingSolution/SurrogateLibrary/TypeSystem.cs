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
        private UInt64 _index = 0;

        public TypeSystem()
        {
            _types = new ConcurrentDictionary<UInt64, SurrogateType>();
            _typesByUniqueName = new ConcurrentDictionary<string, SurrogateType>();
            foreach (var type in KnownTypes.BasicTypes)
            {
                if (type.Index > KnownTypes.MaxIndexForBasicTypes)
                {
                    throw new ArgumentException($"The basic types key index must never exceed {KnownTypes.MaxIndexForBasicTypes}");
                }
                _types[type.Index] = type;
                _typesByUniqueName[type.UniqueName] = type;
            }

            _index = KnownTypes.MaxIndexForBasicTypes;
        }

        public BindingFlags DefaultBindings { get; set; } =
            BindingFlags.Instance | BindingFlags.Public;

        [System.Text.Json.Serialization.JsonConstructor]
        public TypeSystem(IReadOnlyDictionary<UInt64, SurrogateType> types)
        {
            _types = new ConcurrentDictionary<UInt64, SurrogateType>(types);
            _typesByUniqueName = new ConcurrentDictionary<string, SurrogateType>();
            foreach (var type in types.Values)
            {
                _typesByUniqueName[type.UniqueName] = type;
            }
        }

        public IReadOnlyDictionary<UInt64, SurrogateType> Types => _types;
        public IReadOnlyDictionary<string, SurrogateType> TypesByUniqueName => _typesByUniqueName;

        public SurrogateType GetOrCreate(Type type)
        {
            var unique = SurrogateType.GetUniqueName(type.Namespace, type.Name);
            if (!_typesByUniqueName.TryGetValue(unique, out SurrogateType surrogate))
            {
                var newIndex = Interlocked.Increment(ref _index);
                surrogate = new SurrogateType(newIndex,
                    type.Assembly.GetName().Name, type.Namespace, type.Name, null);
                _types[newIndex] = surrogate;
                _typesByUniqueName[surrogate.UniqueName] = surrogate;

                List<SurrogatePropertyInfo> properties;
                if (!type.Namespace.StartsWith("System"))
                {
                    var allProperties = type.GetProperties(DefaultBindings);
                    properties = allProperties
                        .Where(p => p.CanRead && p.CanWrite)
                        .Select(p => p.ToSurrogate(this, newIndex))
                        .ToList();
                }
                else
                {
                    properties = new();
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
            return $"Basic Types={basicTypes}, Others={otherTypes}, LastIndex={_index}";
        }
    }
}
