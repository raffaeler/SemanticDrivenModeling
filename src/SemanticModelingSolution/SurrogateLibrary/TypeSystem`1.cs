using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public record TypeSystem<T> : TypeSystemBase<T>
    {
        public TypeSystem(string identifier) : base(identifier, new TypeSystemFactory()) { }

        [JsonConstructor]
        public TypeSystem(string identifier, IReadOnlyDictionary<UInt64, SurrogateType<T>> types,
            IReadOnlyDictionary<UInt64, SurrogateProperty<T>> properties)
            : base(identifier, types, properties, new TypeSystemFactory())
        {
        }

        internal record TypeSystemFactory : ITypeSystemFactory<T>
        {
            public SurrogateType<T> CreateSurrogateType(UInt64 index, Type type)
                => new SurrogateType<T>(index, type);

            public SurrogateType<T> CreateSurrogateType(UInt64 index, string assemblyName,
                string @namespace, string name, string fullName, TypeFields typeFields1,
                UInt64 innerTypeIndex1, UInt64 innerTypeIndex2, IReadOnlyList<UInt64> propertyIndexes, T info)
                => new SurrogateType<T>(index, assemblyName,
                  @namespace, name, fullName, typeFields1,
                  innerTypeIndex1, innerTypeIndex2, propertyIndexes, info);

            public SurrogateProperty<T> CreateSurrogateProperty(UInt64 index, string name,
                UInt64 propertyTypeIndex, UInt64 ownerTypeIndex)
                => new SurrogateProperty<T>(index, name, propertyTypeIndex, ownerTypeIndex, default(T));
        }

    }

}
