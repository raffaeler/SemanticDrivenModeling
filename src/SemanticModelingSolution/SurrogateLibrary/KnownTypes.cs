using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public static class KnownTypes
    {
        public const UInt64 MaxIndexForBasicTypes = 100;
        public const UInt64 MaxIndexForBasicProperties = 1000;

        private static IList<SurrogateType> _basicTypes;
        public static IList<SurrogateType> BasicTypes => _basicTypes ??= CreateBasicTypes();

        private static IList<SurrogateType> CreateBasicTypes()
        {
            UInt64 index = 0;
            var result = new List<SurrogateType>();
            result.Add(new(++index, typeof(bool)));
            result.Add(new(++index, typeof(Guid)));
            result.Add(new(++index, typeof(string)));

            result.Add(new(++index, typeof(sbyte)));
            result.Add(new(++index, typeof(byte)));
            result.Add(new(++index, typeof(Int16)));
            result.Add(new(++index, typeof(UInt16)));
            result.Add(new(++index, typeof(Int32)));
            result.Add(new(++index, typeof(UInt32)));
            result.Add(new(++index, typeof(Int64)));
            result.Add(new(++index, typeof(UInt64)));
            
            result.Add(new(++index, typeof(DateTime)));
            result.Add(new(++index, typeof(DateTimeOffset)));

            result.Add(new(++index, typeof(Decimal)));
            result.Add(new(++index, typeof(Single)));
            result.Add(new(++index, typeof(Double)));

            return result;
        }
    }
}
