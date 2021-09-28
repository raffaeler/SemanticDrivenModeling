using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public interface ITypeSystem
    {
        /// <summary>
        /// This is used in the basic types belonging to CoreLib to replace
        /// the assembly name. This may change in different .NET implementations
        /// but it is meaningless to classify our type system
        /// On other types the AssemblyName is used to create the type with Type.GetType.
        /// </summary>
        static readonly string PlaceholderForSystemAssemblyName = null;

        SurrogateType GetSurrogateType(UInt64 index);
        SurrogateProperty GetSurrogateProperty(UInt64 index);
        IReadOnlyDictionary<UInt64, SurrogateProperty> Properties { get; }
    }
}
