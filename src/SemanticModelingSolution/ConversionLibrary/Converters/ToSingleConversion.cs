using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToSingleConversion : ConversionBase, IConversion
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
            typeof(decimal),typeof(double),
            typeof(string),
            typeof(Int16),
            typeof(UInt16)
        };

        public ToSingleConversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type TargetType => typeof(float);
        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual float From(string value)
        {
            if (value == null) return FromNull();
            return float.TryParse(value, GetNumberStyles(), GetFormatProvider(), out float res)
                           ? res : (float)_conversionContext?.OnNotSupported?.Invoke(this, value);
        }

        public virtual float From(bool value) => (float)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual float From(Guid value) => (float)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual float From(byte value) => value;
        public virtual float From(sbyte value) => value;
        public virtual float From(DateTime value) => (float)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual float From(DateTimeOffset value) => (float)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual float From(TimeSpan value) => (float)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual float From(decimal value) => (float)value;
        public virtual float From(double value) => (float)value;
        public virtual float From(float value) => value;
        public virtual float From(Int16 value) => value;
        public virtual float From(Int32 value) => value;
        public virtual float From(Int64 value) => value;
        public virtual float From(UInt16 value) => value;
        public virtual float From(UInt32 value) => value;
        public virtual float From(UInt64 value) => value;
        public virtual float FromNull() => default(float);

    }
}
