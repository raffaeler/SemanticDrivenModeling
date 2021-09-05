using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToInt16Conversion : ConversionBase, IConversion
    {
        private Type[] _allowed => new Type[]
        {
            typeof(bool),
            typeof(byte),typeof(sbyte),
            typeof(TimeSpan),
            typeof(decimal),typeof(double),typeof(float),
            typeof(string),
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        private Type[] _lossyOrDangerous = new Type[]
        {
            typeof(bool),
            typeof(string),
            typeof(decimal),typeof(double),typeof(float),
            typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        public ToInt16Conversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type TargetType => typeof(Int16);
        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual Int16 From(string value)
        {
            if (value == null) return FromNull();
            return Int16.TryParse(value, GetNumberStyles(), GetFormatProvider(), out Int16 res)
                           ? res : (Int16)_conversionContext?.OnNotSupported?.Invoke(this, value);
        }

        public virtual Int16 From(bool value) => (Int16)(value ? 1 : 0);
        public virtual Int16 From(Guid value) => (Int16)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Int16 From(byte value) => value;
        public virtual Int16 From(sbyte value) => value;
        public virtual Int16 From(DateTime value) => (Int16)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Int16 From(DateTimeOffset value) => (Int16)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Int16 From(TimeSpan value) => (Int16)value.TotalSeconds;
        public virtual Int16 From(decimal value) => (Int16)value;
        public virtual Int16 From(double value) => (Int16)value;
        public virtual Int16 From(float value) => (Int16)value;
        public virtual Int16 From(Int16 value) => value;
        public virtual Int16 From(Int32 value) => (Int16)value;
        public virtual Int16 From(Int64 value) => (Int16)value;
        public virtual Int16 From(UInt16 value) => (Int16)value;
        public virtual Int16 From(UInt32 value) => (Int16)value;
        public virtual Int16 From(UInt64 value) => (Int16)value;
        public virtual Int16 FromNull() => default(Int16);

    }
}
