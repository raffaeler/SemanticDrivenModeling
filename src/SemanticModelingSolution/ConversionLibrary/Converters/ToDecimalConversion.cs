using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToDecimalConversion : ConversionBase, IConversion
    {
        private Type[] _allowed => new Type[]
        {
            typeof(byte),typeof(sbyte),
            typeof(decimal),typeof(double),typeof(float),
            typeof(string),
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        private Type[] _lossyOrDangerous = new Type[]
        {
            typeof(double),typeof(float),
            typeof(string),
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        public ToDecimalConversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type TargetType => typeof(decimal);
        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual decimal From(string value)
        {
            if (value == null) return FromNull();
            return decimal.TryParse(value, GetNumberStyles(), GetFormatProvider(), out decimal res)
                           ? res : (decimal)_conversionContext?.OnNotSupported?.Invoke(this, value);
        }

        public virtual decimal From(bool value) => (decimal)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual decimal From(Guid value) => (decimal)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual decimal From(byte value) => value;
        public virtual decimal From(sbyte value) => value;
        public virtual decimal From(DateTime value) => (decimal)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual decimal From(DateTimeOffset value) => (decimal)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual decimal From(TimeSpan value) => (decimal)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual decimal From(decimal value) => value;
        public virtual decimal From(double value) => (decimal)value;
        public virtual decimal From(float value) => (decimal)value;
        public virtual decimal From(Int16 value) => value;
        public virtual decimal From(Int32 value) => value;
        public virtual decimal From(Int64 value) => value;
        public virtual decimal From(UInt16 value) => value;
        public virtual decimal From(UInt32 value) => value;
        public virtual decimal From(UInt64 value) => value;
        public virtual decimal FromNull() => default(decimal);
    }
}
