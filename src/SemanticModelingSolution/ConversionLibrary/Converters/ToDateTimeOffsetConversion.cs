using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToDateTimeOffsetConversion : ConversionBase, IConversion
    {
        private Type[] _allowed => new Type[]
        {
            typeof(DateTime),typeof(DateTimeOffset),
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

        public ToDateTimeOffsetConversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type TargetType => typeof(DateTimeOffset);
        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual DateTimeOffset From(string value)
            => DateTimeOffset.TryParseExact(value, GetDateFormats(), GetFormatProvider(), GetDateTimeStyles(), out DateTimeOffset res)
                ? res : (DateTimeOffset)_conversionContext?.OnNotSupported?.Invoke(this, value);

        public virtual DateTimeOffset From(bool value) => (DateTimeOffset)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTimeOffset From(Guid value) => (DateTimeOffset)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTimeOffset From(byte value) => (DateTimeOffset)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTimeOffset From(sbyte value) => (DateTimeOffset)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTimeOffset From(DateTime value) => value;
        public virtual DateTimeOffset From(DateTimeOffset value) => value;
        public virtual DateTimeOffset From(TimeSpan value) => (DateTimeOffset)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTimeOffset From(decimal value) => (DateTimeOffset)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTimeOffset From(double value) => (DateTimeOffset)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTimeOffset From(float value) => (DateTimeOffset)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTimeOffset From(Int16 value) => new DateTimeOffset(DateTime.FromBinary(value), TimeSpan.Zero);
        public virtual DateTimeOffset From(Int32 value) => new DateTimeOffset(DateTime.FromBinary(value), TimeSpan.Zero);
        public virtual DateTimeOffset From(Int64 value) => new DateTimeOffset(DateTime.FromBinary(value), TimeSpan.Zero);
        public virtual DateTimeOffset From(UInt16 value) => new DateTimeOffset(DateTime.FromBinary(value), TimeSpan.Zero);
        public virtual DateTimeOffset From(UInt32 value) => new DateTimeOffset(DateTime.FromBinary(value), TimeSpan.Zero);
        public virtual DateTimeOffset From(UInt64 value) => new DateTimeOffset(DateTime.FromBinary((Int64)value), TimeSpan.Zero);
        public virtual DateTimeOffset FromNull() => default(DateTimeOffset);
    }
}
