using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToDoubleConversion : ConversionBase, IConversion
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
            typeof(decimal),typeof(float),
            typeof(string),
        };

        public ToDoubleConversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type TargetType => typeof(double);
        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual double From(string value)
        {
            if (value == null) return FromNull();
            return double.TryParse(value, GetNumberStyles(), GetFormatProvider(), out double res)
                           ? res : (double)_conversionContext?.OnNotSupported?.Invoke(this, value);
        }

        public virtual double From(bool value) => (double)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual double From(Guid value) => (double)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual double From(byte value) => value;
        public virtual double From(sbyte value) => value;
        public virtual double From(DateTime value) => (double)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual double From(DateTimeOffset value) => (double)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual double From(TimeSpan value) => (double)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual double From(decimal value) => (double)value;
        public virtual double From(double value) => value;
        public virtual double From(float value) => value;
        public virtual double From(Int16 value) => value;
        public virtual double From(Int32 value) => value;
        public virtual double From(Int64 value) => value;
        public virtual double From(UInt16 value) => value;
        public virtual double From(UInt32 value) => value;
        public virtual double From(UInt64 value) => value;
        public virtual double FromNull() => default(double);

    }
}
