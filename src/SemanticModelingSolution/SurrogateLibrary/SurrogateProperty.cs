using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace SurrogateLibrary
{
    public record SurrogateProperty
    {
        internal PropertyInfo _propertyInfo;

        [JsonConstructor]
        public SurrogateProperty(UInt64 index, string name,
            UInt64 propertyTypeIndex, UInt64 ownerTypeIndex/*, UInt64 corePropertyTypeIndex*/) =>
            (Index, Name, PropertyTypeIndex, OwnerTypeIndex/*, CorePropertyTypeIndex*/) =
            (index, name, propertyTypeIndex, ownerTypeIndex/*, corePropertyTypeIndex*/);

        public UInt64 Index { get; init; }
        public string Name { get; init; }
        public UInt64 OwnerTypeIndex { get; init; }
        public UInt64 PropertyTypeIndex { get; init; }
        //public UInt64 CorePropertyTypeIndex { get; init; }

        [JsonIgnore]
        public SurrogateType OwnerType { get; private set; }

        [JsonIgnore]
        public SurrogateType PropertyType { get; private set; }

        public PropertyKind GetKind()
        {
            if (OwnerType == null) throw new InvalidOperationException($"Calling UpdateCache is required before this method");

            if (OwnerType.IsBasicType) return PropertyKind.BasicType;
            if (OwnerType.IsEnum()) return PropertyKind.Enum;
            if (OwnerType.IsCollection() || OwnerType.IsDictionary())
            {
                if (OwnerType.InnerType1 == null)
                {
                    // non-generic or array
                    return PropertyKind.OneToMany;
                }

                if (OwnerType.InnerType1.IsBasicType)
                {
                    // non-generic or array
                    return PropertyKind.OneToManyBasicType;
                }

                if (OwnerType.InnerType1.IsEnum())
                {
                    // non-generic or array
                    return PropertyKind.OneToManyEnum;
                }

                return PropertyKind.OneToMany;
            }

            return PropertyKind.OneToOne;
        }

        internal void UpdateCache(TypeSystem typeSystem)
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
