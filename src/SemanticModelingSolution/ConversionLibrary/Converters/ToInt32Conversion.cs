using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToInt32Conversion : ConversionBase, IConversion
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
            typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        public ToInt32Conversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type TargetType => typeof(Int32);
        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual Int32 From(string value)
            => Int32.TryParse(value, GetNumberStyles(), GetFormatProvider(), out Int32 res)
                ? res : (Int32)_conversionContext?.OnNotSupported?.Invoke(this, value);

        public virtual Int32 From(bool value) => (Int32)(value ? 1 : 0);
        public virtual Int32 From(Guid value) => (Int32)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Int32 From(byte value) => value;
        public virtual Int32 From(sbyte value) => value;
        public virtual Int32 From(DateTime value) => (Int32)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Int32 From(DateTimeOffset value) => (Int32)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Int32 From(TimeSpan value) => (Int32)value.TotalSeconds;
        public virtual Int32 From(decimal value) => (Int32)value;
        public virtual Int32 From(double value) => (Int32)value;
        public virtual Int32 From(float value) => (Int32)value;
        public virtual Int32 From(Int16 value) => value;
        public virtual Int32 From(Int32 value) => value;
        public virtual Int32 From(Int64 value) => (Int32)value;
        public virtual Int32 From(UInt16 value) => (Int32)value;
        public virtual Int32 From(UInt32 value) => (Int32)value;
        public virtual Int32 From(UInt64 value) => (Int32)value;
        public virtual Int32 FromNull() => default(Int32);

    }
}
