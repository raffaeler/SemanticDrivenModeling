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
            UInt64 propertyTypeIndex, UInt64 ownerTypeIndex) =>
            (Index, Name, PropertyTypeIndex, OwnerTypeIndex) =
            (index, name, propertyTypeIndex, ownerTypeIndex);

        public UInt64 Index { get; init; }
        public string Name { get; init; }
        public UInt64 OwnerTypeIndex { get; init; }
        public UInt64 PropertyTypeIndex { get; init; }
        //public UInt64 CorePropertyTypeIndex { get; init; }

        public override string ToString()
        {
            //return $"[{Index}] {PropertyType.Name} {OwnerType.Name}.{Name}";
            return $"[{Index}] [{OwnerTypeIndex}].{Name}";
        }
    }
}
