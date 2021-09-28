using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    internal interface ITypeSystemFactory<T>
        //where TType : SurrogateType
        //where TProperty : SurrogateProperty
    {
        SurrogateType<T> CreateSurrogateType(UInt64 index, Type type);

        SurrogateType<T> CreateSurrogateType(UInt64 index, string assemblyName,
                string @namespace, string name, string fullName, TypeFields typeFields1,
                UInt64 innerTypeIndex1, UInt64 innerTypeIndex2, IReadOnlyList<UInt64> propertyIndexes, T info);

        SurrogateProperty<T> CreateSurrogateProperty(UInt64 index, string name,
                UInt64 propertyTypeIndex, UInt64 ownerTypeIndex);
    }
}
