using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionLibrary.Converters;

namespace ConversionLibrary
{
    public class ConversionContext
    {
        public Func<ConversionBase, string> GuidFormat { get; set; }
        public Func<ConversionBase, string[]> DateFormat { get; set; }
        public Func<ConversionBase, string[]> TimeFormat { get; set; }
        public Func<ConversionBase, string> NumericFormat { get; set; }
        public Func<ConversionBase, DateTimeStyles> DateTimeStyles { get; set; }
        public Func<ConversionBase, TimeSpanStyles> TimeSpanStyles { get; set; }
        public Func<ConversionBase, NumberStyles> NumberStyles { get; set; }
        public Func<ConversionBase, IFormatProvider> FormatProvider { get; set; }

        public Func<ConversionBase, object, object> OnNotSupported { get; set; }
    }
}
