using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public interface ITypeSystem
    {
        SurrogateType GetSurrogateType(UInt64 index);
        SurrogateProperty GetSurrogateProperty(UInt64 index);
        IReadOnlyDictionary<UInt64, SurrogateProperty> Properties { get; }
    }
}
