using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public abstract class ConversionBase
    {
        protected readonly ConversionContext _conversionContext;

        public ConversionBase(ConversionContext conversionContext)
        {
            _conversionContext = conversionContext;

            DateFormats = new[]
            {
                "yyyy/MM/dd HH:mm",
                "yyyy/MM/dd HH:mm:ss",
                "yyyy/MM/dd HH:mm:ss:fff",
            };

            TimeFormats = new[] { "HH:mm:ss:fff" };

            GuidFormat = "B";
        }

        public virtual Type[] BasicTypes => new Type[]
        {
            typeof(bool), typeof(Guid),
            typeof(byte),typeof(sbyte),
            typeof(DateTime),typeof(DateTimeOffset), typeof(TimeSpan),
            typeof(decimal),typeof(double),typeof(float),
            typeof(string),
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        public abstract Type TargetType { get; }
        public virtual Type[] LossyOrDangerous => Array.Empty<Type>();

        public virtual string GuidFormat { get; set; }
        public virtual string[] DateFormats { get; set; }
        public virtual string[] TimeFormats { get; set; }
        public virtual string NumericFormat { get; set; }
        public virtual DateTimeStyles DateTimeStyles { get; set; }
        public virtual TimeSpanStyles TimeSpanStyles { get; set; }
        public virtual NumberStyles NumberStyles { get; set; }
        public virtual IFormatProvider FormatProvider { get; set; }

        protected virtual string GetGuidFormat() => _conversionContext?.GuidFormat?.Invoke(this) ?? GuidFormat;
        protected virtual string[] GetDateFormats() => _conversionContext?.DateFormat?.Invoke(this) ?? DateFormats;
        protected virtual string[] GetTimeFormats() => _conversionContext?.TimeFormat?.Invoke(this) ?? TimeFormats;
        protected virtual string GetNumericFormat() => _conversionContext?.NumericFormat?.Invoke(this) ?? NumericFormat;
        protected virtual DateTimeStyles GetDateTimeStyles() => _conversionContext?.DateTimeStyles?.Invoke(this) ?? DateTimeStyles;
        protected virtual TimeSpanStyles GetTimeSpanStyles() => _conversionContext?.TimeSpanStyles?.Invoke(this) ?? TimeSpanStyles;
        protected virtual NumberStyles GetNumberStyles() => _conversionContext?.NumberStyles?.Invoke(this) ?? NumberStyles;
        protected virtual IFormatProvider GetFormatProvider() => _conversionContext?.FormatProvider?.Invoke(this) ?? FormatProvider;
    }
}
