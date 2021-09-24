using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    [Flags]
    public enum TypeFields : UInt64
    {
        None = 0x0000_0000_0000_0000,
        ValueType = 0x0000_0000_0000_0001,
        ReferenceType = 0x0000_0000_0000_0010,
        Interface = 0x0000_0000_0000_0100,
        Abstract = 0x0000_0000_0000_1000,

        GenericType = 0x0000_0000_0001_0000,
        Collection = 0x0000_0000_0010_0000,
        Dictionary = 0x0000_0000_0100_0000,
        //? = 0x0000_0000_1000_0000,

        Enum = 0x0000_0001_0000_0000,
        Nullable = 0x0000_0010_0000_0000,
        //? = 0x0000_0100_0000_0000,
        //? = 0x0000_1000_0000_0000,

        //? = 0x0001_0000_0000_0000,
        //? = 0x0010_0000_0000_0000,
        //? = 0x0100_0000_0000_0000,
        //? = 0x1000_0000_0000_0000,

    }
}
