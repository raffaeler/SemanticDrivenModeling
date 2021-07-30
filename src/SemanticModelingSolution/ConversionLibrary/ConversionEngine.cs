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
            ConversionTypes[typeof(bool)] = new ToBooleanConversion(conversionContext);
            ConversionTypes[typeof(Guid)] = new ToGuidConversion(conversionContext);
            ConversionTypes[typeof(byte)] = new ToByteConversion(conversionContext);
            ConversionTypes[typeof(sbyte)] = new ToSByteConversion(conversionContext);
            ConversionTypes[typeof(DateTime)] = new ToDateTimeConversion(conversionContext);
            ConversionTypes[typeof(DateTimeOffset)] = new ToDateTimeOffsetConversion(conversionContext);
            ConversionTypes[typeof(TimeSpan)] = new ToTimeSpanConversion(conversionContext);
            ConversionTypes[typeof(decimal)] = new ToDecimalConversion(conversionContext);
            ConversionTypes[typeof(double)] = new ToDoubleConversion(conversionContext);
            ConversionTypes[typeof(float)] = new ToSingleConversion(conversionContext);
            ConversionTypes[typeof(string)] = new ToStringConversion(conversionContext);
            ConversionTypes[typeof(Int16)] = new ToInt16Conversion(conversionContext);
            ConversionTypes[typeof(Int32)] = new ToInt32Conversion(conversionContext);
            ConversionTypes[typeof(Int64)] = new ToInt64Conversion(conversionContext);
            ConversionTypes[typeof(UInt16)] = new ToUInt16Conversion(conversionContext);
            ConversionTypes[typeof(UInt32)] = new ToUInt32Conversion(conversionContext);
            ConversionTypes[typeof(UInt64)] = new ToUInt64Conversion(conversionContext);

            ConversionStrings[typeof(bool).Name] = new ToBooleanConversion(conversionContext);
            ConversionStrings[typeof(Guid).Name] = new ToGuidConversion(conversionContext);
            ConversionStrings[typeof(byte).Name] = new ToByteConversion(conversionContext);
            ConversionStrings[typeof(sbyte).Name] = new ToSByteConversion(conversionContext);
            ConversionStrings[typeof(DateTime).Name] = new ToDateTimeConversion(conversionContext);
            ConversionStrings[typeof(DateTimeOffset).Name] = new ToDateTimeOffsetConversion(conversionContext);
            ConversionStrings[typeof(TimeSpan).Name] = new ToTimeSpanConversion(conversionContext);
            ConversionStrings[typeof(decimal).Name] = new ToDecimalConversion(conversionContext);
            ConversionStrings[typeof(double).Name] = new ToDoubleConversion(conversionContext);
            ConversionStrings[typeof(float).Name] = new ToSingleConversion(conversionContext);
            ConversionStrings[typeof(string).Name] = new ToStringConversion(conversionContext);
            ConversionStrings[typeof(Int16).Name] = new ToInt16Conversion(conversionContext);
            ConversionStrings[typeof(Int32).Name] = new ToInt32Conversion(conversionContext);
            ConversionStrings[typeof(Int64).Name] = new ToInt64Conversion(conversionContext);
            ConversionStrings[typeof(UInt16).Name] = new ToUInt16Conversion(conversionContext);
            ConversionStrings[typeof(UInt32).Name] = new ToUInt32Conversion(conversionContext);
            ConversionStrings[typeof(UInt64).Name] = new ToUInt64Conversion(conversionContext);
        }

        /// <summary>
        /// Converters by Type
        /// </summary>
        public IDictionary<Type, IConversion> ConversionTypes { get; } = new Dictionary<Type, IConversion>();

        /// <summary>
        /// Converters by Type.Name
        /// </summary>
        public IDictionary<string, IConversion> ConversionStrings { get; } = new Dictionary<string, IConversion>();
    }
}
