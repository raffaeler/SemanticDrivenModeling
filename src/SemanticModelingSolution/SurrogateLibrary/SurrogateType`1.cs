using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public record SurrogateType<T> : SurrogateType
    {
        /// <summary>
        /// Only Basic Types
        /// </summary>
        internal SurrogateType(UInt64 index, Type type) : base(index, type)
        {
        }

        [JsonConstructor]
        public SurrogateType(UInt64 index, string assemblyName,
            string @namespace, string name, string fullName, TypeFields typeFields1,
            UInt64 innerTypeIndex1, UInt64 innerTypeIndex2, IReadOnlyList<UInt64> propertyIndexes, T info)
            : base(index, assemblyName,
                  @namespace, name, fullName, typeFields1,
                  innerTypeIndex1, innerTypeIndex2, propertyIndexes) =>
            Info = info;

        public T Info { get; internal set; }
    }
}
