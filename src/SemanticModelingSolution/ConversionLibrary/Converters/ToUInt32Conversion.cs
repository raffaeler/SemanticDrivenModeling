using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToUInt32Conversion : ConversionBase, IConversion
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
            typeof(sbyte),
            typeof(string),
            typeof(decimal),typeof(double),typeof(float),
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt64),
        };

        public ToUInt32Conversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type TargetType => typeof(UInt32);
        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual UInt32 From(string value)
        {
            if (value == null) return FromNull();
            return UInt32.TryParse(value, GetNumberStyles(), GetFormatProvider(), out UInt32 res)
                           ? res : (UInt32)_conversionContext?.OnNotSupported?.Invoke(this, value);
        }

        public virtual UInt32 From(bool value) => (UInt32)(value ? 1 : 0);
        public virtual UInt32 From(Guid value) => (UInt32)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual UInt32 From(byte value) => value;
        public virtual UInt32 From(sbyte value) => (UInt32)value;
        public virtual UInt32 From(DateTime value) => (UInt32)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual UInt32 From(DateTimeOffset value) => (UInt32)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual UInt32 From(TimeSpan value) => (UInt32)value.TotalSeconds;
        public virtual UInt32 From(decimal value) => (UInt32)value;
        public virtual UInt32 From(double value) => (UInt32)value;
        public virtual UInt32 From(float value) => (UInt32)value;
        public virtual UInt32 From(Int16 value) => (UInt32)value;
        public virtual UInt32 From(Int32 value) => (UInt32)value;
        public virtual UInt32 From(Int64 value) => (UInt32)value;
        public virtual UInt32 From(UInt16 value) => value;
        public virtual UInt32 From(UInt32 value) => value;
        public virtual UInt32 From(UInt64 value) => (UInt32)value;
        public virtual UInt32 FromNull() => default(UInt32);

    }
}
