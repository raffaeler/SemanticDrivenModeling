using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToUInt16Conversion : ConversionBase, IConversion
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
            typeof(UInt32), typeof(UInt64),
        };

        public ToUInt16Conversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual UInt16 From(string value)
            => UInt16.TryParse(value, GetNumberStyles(), GetFormatProvider(), out UInt16 res)
                ? res : (UInt16)_conversionContext?.OnNotSupported?.Invoke(this, value);

        public virtual UInt16 From(bool value) => (UInt16)(value ? 1 : 0);
        public virtual UInt16 From(Guid value) => (UInt16)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual UInt16 From(byte value) => value;
        public virtual UInt16 From(sbyte value) => (UInt16)value;
        public virtual UInt16 From(DateTime value) => (UInt16)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual UInt16 From(DateTimeOffset value) => (UInt16)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual UInt16 From(TimeSpan value) => (UInt16)value.TotalSeconds;
        public virtual UInt16 From(decimal value) => (UInt16)value;
        public virtual UInt16 From(double value) => (UInt16)value;
        public virtual UInt16 From(float value) => (UInt16)value;
        public virtual UInt16 From(Int16 value) => (UInt16)value;
        public virtual UInt16 From(Int32 value) => (UInt16)value;
        public virtual UInt16 From(Int64 value) => (UInt16)value;
        public virtual UInt16 From(UInt16 value) => value;
        public virtual UInt16 From(UInt32 value) => (UInt16)value;
        public virtual UInt16 From(UInt64 value) => (UInt16)value;
        public virtual UInt16 FromNull() => default(UInt16);

    }
}
