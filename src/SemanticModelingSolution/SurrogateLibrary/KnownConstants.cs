using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public static class KnownConstants
    {
        public const UInt64 MaxIndexForBasicTypes = 100;
        public const UInt64 MaxIndexForBasicProperties = 1000;

        /// <summary>
        /// This is used in the basic types belonging to CoreLib to replace
        /// the assembly name. This may change in different .NET implementations
        /// but it is meaningless to classify our type system
        /// On other types the AssemblyName is used to create the type with Type.GetType.
        /// </summary>
        public const string PlaceholderForSystemAssemblyName = null;

    }
}
