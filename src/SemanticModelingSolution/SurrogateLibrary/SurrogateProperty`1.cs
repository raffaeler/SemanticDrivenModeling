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

        public T Info { get; internal set; }
    }
}
