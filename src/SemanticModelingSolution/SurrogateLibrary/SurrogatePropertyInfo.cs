using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace SurrogateLibrary
{
    public record SurrogatePropertyInfo
    {
        internal PropertyInfo _propertyInfo;

        [JsonConstructor]
        public SurrogatePropertyInfo(UInt64 index, string name, UInt64 propertyTypeIndex, UInt64 ownerTypeIndex) =>
            (Index, Name, PropertyTypeIndex, OwnerTypeIndex) =
            (index, name, propertyTypeIndex, ownerTypeIndex);

        public UInt64 Index {  get; init; }
        public string Name { get; init; }
        public UInt64 OwnerTypeIndex { get; init; }
        public UInt64 PropertyTypeIndex { get; init; }

    }
}
