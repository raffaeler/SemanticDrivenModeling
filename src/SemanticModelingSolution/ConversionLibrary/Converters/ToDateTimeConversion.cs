using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToDateTimeConversion : ConversionBase, IConversion
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
            typeof(DateTimeOffset),
            typeof(string),
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        public ToDateTimeConversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type TargetType => typeof(DateTime);
        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual DateTime From(string value)
            => DateTime.TryParseExact(value, GetDateFormats(), GetFormatProvider(), GetDateTimeStyles(), out DateTime res)
                ? res : (DateTime)_conversionContext?.OnNotSupported?.Invoke(this, value);

        public virtual DateTime From(bool value) => (DateTime)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTime From(Guid value) => (DateTime)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTime From(byte value) => (DateTime)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTime From(sbyte value) => (DateTime)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTime From(DateTime value) => value;
        public virtual DateTime From(DateTimeOffset value) => value.DateTime;
        public virtual DateTime From(TimeSpan value) => (DateTime)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTime From(decimal value) => (DateTime)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTime From(double value) => (DateTime)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTime From(float value) => (DateTime)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual DateTime From(Int16 value) => DateTime.FromBinary(value);
        public virtual DateTime From(Int32 value) => DateTime.FromBinary(value);
        public virtual DateTime From(Int64 value) => DateTime.FromBinary(value);
        public virtual DateTime From(UInt16 value) => DateTime.FromBinary(value);
        public virtual DateTime From(UInt32 value) => DateTime.FromBinary(value);
        public virtual DateTime From(UInt64 value) => DateTime.FromBinary((Int64)value);
        public virtual DateTime FromNull() => default(DateTime);

    }
}
