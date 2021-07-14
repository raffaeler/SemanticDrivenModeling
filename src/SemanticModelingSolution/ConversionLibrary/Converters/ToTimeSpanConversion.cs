using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToTimeSpanConversion : ConversionBase, IConversion
    {
        private Type[] _allowed => new Type[]
        {
            typeof(TimeSpan),
            typeof(string),
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        private Type[] _lossyOrDangerous = new Type[]
        {
            typeof(string),
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        public ToTimeSpanConversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual TimeSpan From(string value) 
            => TimeSpan.TryParseExact(value, GetTimeFormats(), GetFormatProvider(), GetTimeSpanStyles(), out TimeSpan res)
                ? res : (TimeSpan)_conversionContext?.OnNotSupported?.Invoke(this, value);

        public virtual TimeSpan From(bool value) => (TimeSpan)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual TimeSpan From(Guid value) => (TimeSpan)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual TimeSpan From(byte value) => (TimeSpan)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual TimeSpan From(sbyte value) => (TimeSpan)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual TimeSpan From(DateTime value) => (TimeSpan)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual TimeSpan From(DateTimeOffset value) => (TimeSpan)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual TimeSpan From(TimeSpan value) => value;
        public virtual TimeSpan From(decimal value) => (TimeSpan)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual TimeSpan From(double value) => (TimeSpan)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual TimeSpan From(float value) => (TimeSpan)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual TimeSpan From(Int16 value) => TimeSpan.FromSeconds(value);
        public virtual TimeSpan From(Int32 value) => TimeSpan.FromSeconds(value);
        public virtual TimeSpan From(Int64 value) => TimeSpan.FromTicks(value);
        public virtual TimeSpan From(UInt16 value) => TimeSpan.FromSeconds(value);
        public virtual TimeSpan From(UInt32 value) => TimeSpan.FromSeconds(value);
        public virtual TimeSpan From(UInt64 value) => TimeSpan.FromTicks((Int64)value);
    }
}
