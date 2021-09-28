using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public interface ITypeSystem<T>
    {
        SurrogateType<T> GetSurrogateType(UInt64 index);
        SurrogateProperty<T> GetSurrogateProperty(UInt64 index);
        IReadOnlyDictionary<UInt64, SurrogateProperty<T>> Properties { get; }
    }
}
