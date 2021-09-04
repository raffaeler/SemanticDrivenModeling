using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToJsonStringConversion : ToStringConversion
    {
        public ToJsonStringConversion(ConversionContext conversionContext) : base(conversionContext) { }
    }
}
