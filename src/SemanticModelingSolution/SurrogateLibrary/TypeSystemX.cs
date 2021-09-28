using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public record Void;
    public record TypeSystem : TypeSystemBase<Void>
    {
        public TypeSystem() : base(new TypeSystemFactory()) { }

        [JsonConstructor]
        public TypeSystem(IReadOnlyDictionary<UInt64, SurrogateType<Void>> types,
            IReadOnlyDictionary<UInt64, SurrogateProperty<Void>> properties)
            : base(types, properties, new TypeSystemFactory())
        {
        }

        internal record TypeSystemFactory : ITypeSystemFactory<Void>
        {
            public SurrogateType<Void> CreateSurrogateType(UInt64 index, Type type)
                => new SurrogateType<Void>(index, type);

            public SurrogateType<Void> CreateSurrogateType(UInt64 index, string assemblyName,
                string @namespace, string name, string fullName, TypeFields typeFields1,
                UInt64 innerTypeIndex1, UInt64 innerTypeIndex2, IReadOnlyList<UInt64> propertyIndexes, Void info)
                => new SurrogateType<Void>(index, assemblyName,
                  @namespace, name, fullName, typeFields1,
                  innerTypeIndex1, innerTypeIndex2, propertyIndexes, info);

            public SurrogateProperty<Void> CreateSurrogateProperty(UInt64 index, string name,
                UInt64 propertyTypeIndex, UInt64 ownerTypeIndex)
                => new SurrogateProperty<Void>(index, name, propertyTypeIndex, ownerTypeIndex, default(Void));
        }

    }
}
