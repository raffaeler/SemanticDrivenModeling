using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public record VoidType;
    public record TypeSystem : TypeSystemBase<VoidType>
    {
        public TypeSystem() : base(new TypeSystemFactory()) { }

        [JsonConstructor]
        public TypeSystem(IReadOnlyDictionary<UInt64, SurrogateType<VoidType>> types,
            IReadOnlyDictionary<UInt64, SurrogateProperty<VoidType>> properties)
            : base(types, properties, new TypeSystemFactory())
        {
        }

        internal record TypeSystemFactory : ITypeSystemFactory<VoidType>
        {
            public SurrogateType<VoidType> CreateSurrogateType(UInt64 index, Type type)
                => new SurrogateType<VoidType>(index, type);

            public SurrogateType<VoidType> CreateSurrogateType(UInt64 index, string assemblyName,
                string @namespace, string name, string fullName, TypeFields typeFields1,
                UInt64 innerTypeIndex1, UInt64 innerTypeIndex2, IReadOnlyList<UInt64> propertyIndexes, VoidType info)
                => new SurrogateType<VoidType>(index, assemblyName,
                  @namespace, name, fullName, typeFields1,
                  innerTypeIndex1, innerTypeIndex2, propertyIndexes, info);

            public SurrogateProperty<VoidType> CreateSurrogateProperty(UInt64 index, string name,
                UInt64 propertyTypeIndex, UInt64 ownerTypeIndex)
                => new SurrogateProperty<VoidType>(index, name, propertyTypeIndex, ownerTypeIndex, default(VoidType));
        }

    }
}
