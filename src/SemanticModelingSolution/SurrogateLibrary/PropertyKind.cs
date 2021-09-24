using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public enum PropertyKind
    {
        BasicType,
        OneToManyBasicType,

        Enum,
        OneToManyEnum,

        OneToOneToDomain,
        OneToManyToDomain,

        OneToOneToUnknown,
        OneToManyToUnknown,
    }
}
