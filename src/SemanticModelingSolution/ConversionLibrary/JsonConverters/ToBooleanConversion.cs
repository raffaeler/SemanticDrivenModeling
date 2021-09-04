using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToJsonBooleanConversion : ToBooleanConversion
    {
        public ToJsonBooleanConversion(ConversionContext conversionContext) : base(conversionContext) { }
    }
}
