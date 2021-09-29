using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public record SurrogateProperty<T> : SurrogateProperty
    {
        [JsonConstructor]
        public SurrogateProperty(UInt64 index, string name,
            UInt64 propertyTypeIndex, UInt64 ownerTypeIndex, T info)
            : base(index, name, propertyTypeIndex, ownerTypeIndex) =>
            Info = info;


        [JsonIgnore]
        public SurrogateType<T> OwnerType { get; private set; }

        [JsonIgnore]
        public SurrogateType<T> PropertyType { get; private set; }

        public T Info { get; internal set; }

        public PropertyKind GetKind()
        {
            if (PropertyType == null) throw new InvalidOperationException($"Calling UpdateCache is required before this method");

            if (PropertyType.IsBasicType) return PropertyKind.BasicType;
            if (PropertyType.IsEnum()) return PropertyKind.Enum;
            if (PropertyType.IsCollection() || PropertyType.IsDictionary())
            {
                if (PropertyType.InnerType1 == null)
                {
                    // non-generic or array
                    return PropertyKind.OneToMany;
                }

                if (PropertyType.InnerType1.IsBasicType)
                {
                    // non-generic or array
                    return PropertyKind.OneToManyBasicType;
                }

                if (PropertyType.InnerType1.IsEnum())
                {
                    // non-generic or array
                    return PropertyKind.OneToManyEnum;
                }

                return PropertyKind.OneToMany;
            }

            return PropertyKind.OneToOne;
        }

        internal void UpdateCache(ITypeSystem<T> typeSystem)
        {
            OwnerType = typeSystem.GetSurrogateType(OwnerTypeIndex);
            PropertyType = typeSystem.GetSurrogateType(PropertyTypeIndex);
        }

        public override string ToString()
        {
            //return $"[{Index}] {PropertyType.Name} {OwnerType.Name}.{Name}";
            return $"[{Index}] {OwnerType.Name}.{Name}";
        }

    }
}
