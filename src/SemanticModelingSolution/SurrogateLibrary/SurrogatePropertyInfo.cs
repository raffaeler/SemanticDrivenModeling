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
        public SurrogatePropertyInfo(string name, UInt64 propertyTypeIndex, UInt64 ownerTypeIndex) =>
            (Name, PropertyTypeIndex, OwnerTypeIndex) =
            (name, propertyTypeIndex, ownerTypeIndex);

        public string Name { get; init; }
        public UInt64 OwnerTypeIndex { get; init; }
        public UInt64 PropertyTypeIndex { get; init; }

    }
}
