using System;
using System.Collections.Generic;

using ConversionLibrary.Converters;

namespace ConversionLibrary
{
    public class ConversionEngine
    {
        public static readonly string Iso8601Format_Json = "s";
        public static readonly IFormatProvider Iso8601FormatProvider_Json = System.Globalization.CultureInfo.InvariantCulture;

        public ConversionEngine(ConversionContext conversionContext = null)
        {
            Conversions[typeof(bool)] = new ToBooleanConversion(conversionContext);
            Conversions[typeof(Guid)] = new ToGuidConversion(conversionContext);
            Conversions[typeof(byte)] = new ToByteConversion(conversionContext);
            Conversions[typeof(sbyte)] = new ToSbyteConversion(conversionContext);
            Conversions[typeof(DateTime)] = new ToDateTimeConversion(conversionContext);
            Conversions[typeof(DateTimeOffset)] = new ToDateTimeOffsetConversion(conversionContext);
            Conversions[typeof(TimeSpan)] = new ToTimeSpanConversion(conversionContext);
            Conversions[typeof(decimal)] = new ToDecimalConversion(conversionContext);
            Conversions[typeof(double)] = new ToDoubleConversion(conversionContext);
            Conversions[typeof(float)] = new ToFloatConversion(conversionContext);
            Conversions[typeof(string)] = new ToStringConversion(conversionContext);
            Conversions[typeof(Int16)] = new ToInt16Conversion(conversionContext);
            Conversions[typeof(Int32)] = new ToInt32Conversion(conversionContext);
            Conversions[typeof(Int64)] = new ToInt64Conversion(conversionContext);
            Conversions[typeof(UInt16)] = new ToUInt16Conversion(conversionContext);
            Conversions[typeof(UInt32)] = new ToUInt32Conversion(conversionContext);
            Conversions[typeof(UInt64)] = new ToUInt64Conversion(conversionContext);
        }

        public IDictionary<Type, IConversion> Conversions { get; } = new Dictionary<Type, IConversion>();
    }
}
