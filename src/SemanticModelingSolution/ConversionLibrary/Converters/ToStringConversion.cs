using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToStringConversion : ConversionBase, IConversion
    {
        public ToStringConversion(ConversionContext conversionContext) : base(conversionContext) { }

        public virtual bool CanConvertFrom(Type type) => BasicTypes.Contains(type);

        public virtual string From(bool value) => value.ToString(GetFormatProvider());
        public virtual string From(Guid value) => value.ToString(GetGuidFormat(), GetFormatProvider());
        public virtual string From(byte value) => value.ToString(GetFormatProvider());
        public virtual string From(sbyte value) => value.ToString(GetFormatProvider());
        public virtual string From(DateTime value) => value.ToString(GetDateFormats()?[0], GetFormatProvider());
        public virtual string From(DateTimeOffset value) => value.ToString(GetDateFormats()?[0], GetFormatProvider());
        public virtual string From(TimeSpan value) => value.ToString(GetTimeFormats()?[0], GetFormatProvider());
        public virtual string From(decimal value) => value.ToString(GetNumericFormat(), GetFormatProvider());
        public virtual string From(double value) => value.ToString(GetNumericFormat(), GetFormatProvider());
        public virtual string From(float value) => value.ToString(GetNumericFormat(), GetFormatProvider());
        public virtual string From(string value) => value;
        public virtual string From(Int16 value) => value.ToString(GetNumericFormat(), GetFormatProvider());
        public virtual string From(Int32 value) => value.ToString(GetNumericFormat(), GetFormatProvider());
        public virtual string From(Int64 value) => value.ToString(GetNumericFormat(), GetFormatProvider());
        public virtual string From(UInt16 value) => value.ToString(GetNumericFormat(), GetFormatProvider());
        public virtual string From(UInt32 value) => value.ToString(GetNumericFormat(), GetFormatProvider());
        public virtual string From(UInt64 value) => value.ToString(GetNumericFormat(), GetFormatProvider());
    }
}
