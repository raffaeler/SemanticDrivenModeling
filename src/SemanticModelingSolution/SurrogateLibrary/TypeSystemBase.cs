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
    public abstract record TypeSystemBase<T>: ITypeSystem<T>, ITypeSystem
    //where TType : SurrogateType
    //where TProperty : SurrogateProperty
    {
        private readonly ITypeSystemFactory<T> _factory;
        private ConcurrentDictionaryEx<UInt64, SurrogateType<T>> _types;
        private ConcurrentDictionaryEx<string, SurrogateType<T>> _typesByFullName;
        private ConcurrentDictionaryEx<UInt64, SurrogateProperty<T>> _properties;
        private UInt64 _typeIndex = 0;
        private UInt64 _propertyIndex = 0;

        internal TypeSystemBase(ITypeSystemFactory<T> factory)
        {
            _factory = factory;
            _types = new();
            _typesByFullName = new();
            _properties = new();
            foreach (var type in CreateBasicTypes())
            {
                if (type.Index > KnownConstants.MaxIndexForBasicTypes)
                {
                    throw new ArgumentException($"The basic types key index must never exceed {KnownConstants.MaxIndexForBasicTypes}");
                }
                _types[type.Index] = type;
                _typesByFullName[type.FullName] = type;
            }

            _typeIndex = KnownConstants.MaxIndexForBasicTypes;
            _propertyIndex = KnownConstants.MaxIndexForBasicProperties;
        }

        public BindingFlags DefaultBindings { get; set; } =
            BindingFlags.Instance | BindingFlags.Public;

        [System.Text.Json.Serialization.JsonConstructor]
        internal TypeSystemBase(IReadOnlyDictionary<UInt64, SurrogateType<T>> types,
            IReadOnlyDictionary<UInt64, SurrogateProperty<T>> properties, ITypeSystemFactory<T> factory)
        {
            _factory = factory;
            _types = new ConcurrentDictionaryEx<UInt64, SurrogateType<T>>(types);
            _properties = new ConcurrentDictionaryEx<UInt64, SurrogateProperty<T>>(properties);
            _typesByFullName = new ConcurrentDictionaryEx<string, SurrogateType<T>>();
            UInt64 typeIndex = 0;
            foreach (var type in types.Values)
            {
                _typesByFullName[type.FullName] = type;
                typeIndex = Math.Max(typeIndex, type.Index);
            }

            _typeIndex = typeIndex;
            if (properties == null)
            {
                _properties = new();
            }

            if (properties.Count == 0)
            {
                _propertyIndex = KnownConstants.MaxIndexForBasicProperties;
            }
            else
            {
                _propertyIndex = properties.Values.Select(v => v.Index).Max();
            }

        }

        public IReadOnlyDictionary<UInt64, SurrogateType<T>> Types => _types;
        public IReadOnlyDictionary<UInt64, SurrogateProperty<T>> Properties => _properties;
        public IReadOnlyDictionary<string, SurrogateType<T>> TypesByFullName => _typesByFullName;

        public SurrogateType<T> GetOrCreate(Type type)
        {
            if (type == null) return null;
            var unique = SurrogateType<T>.GetFullName(type);
            if (!_typesByFullName.TryGetValue(unique, out SurrogateType<T> surrogate))
            {
                var newIndex = Interlocked.Increment(ref _typeIndex);
                var (flags, inner1, inner2) = type.Classify();
                var innerSurrogate1 = GetOrCreate(inner1);
                var innerSurrogate2 = GetOrCreate(inner2);

                surrogate = _factory.CreateSurrogateType(newIndex,
                    type.Assembly.GetName().Name, type.Namespace, type.Name, unique,
                    flags, innerSurrogate1?.Index ?? 0, innerSurrogate2?.Index ?? 0, null, default(T));

                _types[newIndex] = surrogate;
                _typesByFullName[surrogate.FullName] = surrogate;

                // Don't walk the graph when a type belongs to the System library
                ListEx<UInt64> propertyIndexes = new();
                if (!type.Namespace.StartsWith("System"))
                {
                    var allProperties = type.GetProperties(DefaultBindings);
                    foreach (var pi in allProperties)
                    {
                        if (!pi.CanRead || !pi.CanWrite) continue;
                        var sp = ToSurrogate(pi, Interlocked.Increment(ref _propertyIndex), newIndex);
                        _properties[sp.Index] = sp;
                        propertyIndexes.Add(sp.Index);
                    }
                }

                surrogate._propertyIndexes = propertyIndexes;
            }

            return surrogate;
        }

        public bool Contains(Type type)
        {
            var fullName = SurrogateType.GetFullName(type);
            return TypesByFullName.ContainsKey(fullName);
        }

        public SurrogateProperty<T> ToSurrogate(PropertyInfo propertyInfo, UInt64 index, UInt64 ownerTypeIndex)
        {
            var propertyType = GetOrCreate(propertyInfo.PropertyType);
            var property = _factory.CreateSurrogateProperty(index, propertyInfo.Name, propertyType.Index, ownerTypeIndex);
            return property;
        }

        public SurrogateType<T> GetSurrogateType(UInt64 index) => index == 0 ? null : Types[index];
        public bool TryGetSurrogateType(UInt64 index, out SurrogateType<T> surrogateType)
            => Types.TryGetValue(index, out surrogateType);
        public bool TryGetSurrogateTypeByName(string fullName, out SurrogateType<T> surrogateType) =>
            TypesByFullName.TryGetValue(fullName, out surrogateType);


        public SurrogateProperty<T> GetSurrogateProperty(UInt64 index) => index == 0 ? null : Properties[index];
        public bool TryGetSurrogateProperty(UInt64 index, out SurrogateProperty<T> surrogateProperty)
            => Properties.TryGetValue(index, out surrogateProperty);

        public void UpdateCache()
        {
            foreach (var type in Types.Values)
            {
                type.UpdateCache(this);
            }

            foreach (var prop in Properties.Values)
            {
                prop.UpdateCache(this);
            }
        }

        public override string ToString()
        {
            Debug.Assert(_types.Count == _typesByFullName.Count);

            var basicTypes = Types.Count(t => t.Key < KnownConstants.MaxIndexForBasicTypes);
            var otherTypes = Types.Count(t => t.Key > KnownConstants.MaxIndexForBasicTypes);
            return $"Basic types={basicTypes}, Other types={otherTypes}, Properties={Properties.Count}, LastTypeIndex={_typeIndex}, LastPropIndex={_propertyIndex}";
        }

        private IList<SurrogateType<T>> CreateBasicTypes()
        {
            UInt64 index = 0;
            var result = new List<SurrogateType<T>>();
            result.Add(_factory.CreateSurrogateType(++index, typeof(bool)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(Guid)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(string)));

            result.Add(_factory.CreateSurrogateType(++index, typeof(sbyte)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(byte)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(Int16)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(UInt16)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(Int32)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(UInt32)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(Int64)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(UInt64)));

            result.Add(_factory.CreateSurrogateType(++index, typeof(DateTime)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(DateTimeOffset)));

            result.Add(_factory.CreateSurrogateType(++index, typeof(Decimal)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(Single)));
            result.Add(_factory.CreateSurrogateType(++index, typeof(Double)));

            return result;
        }


        SurrogateType<T> ITypeSystem<T>.GetSurrogateType(UInt64 index) => index == 0 ? null : Types[index];
        SurrogateProperty<T> ITypeSystem<T>.GetSurrogateProperty(UInt64 index) => index == 0 ? null : Properties[index];
        IReadOnlyDictionary<UInt64, SurrogateProperty<T>> ITypeSystem<T>.Properties { get; }


        SurrogateType ITypeSystem.GetSurrogateType(UInt64 index) => index == 0 ? null : Types[index];
        SurrogateProperty ITypeSystem.GetSurrogateProperty(UInt64 index) => index == 0 ? null : Properties[index];
        IReadOnlyDictionary<UInt64, SurrogateProperty> ITypeSystem.Properties { get; }


    }
}
