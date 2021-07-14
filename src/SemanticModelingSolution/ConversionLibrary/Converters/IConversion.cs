using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
    Possible outcomes:
    - validation y/n, isLossy, Func for external conversion
    - conversion y/n, isLossy
    - nullable validation and conversion
    - collection / array conversion

*/

namespace ConversionLibrary.Converters
{
    public interface IConversion
    {
        bool CanConvertFrom(Type type);
        //object From(object value);
    }
}
