using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToUInt64Conversion : ConversionBase, IConversion
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
        };

        public ToUInt64Conversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type TargetType => typeof(UInt64);
        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual UInt64 From(string value)
            => UInt64.TryParse(value, GetNumberStyles(), GetFormatProvider(), out UInt64 res)
                ? res : (UInt64)_conversionContext?.OnNotSupported?.Invoke(this, value);

        public virtual UInt64 From(bool value) => (UInt64)(value ? 1 : 0);
        public virtual UInt64 From(Guid value) => (UInt64)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual UInt64 From(byte value) => value;
        public virtual UInt64 From(sbyte value) => (UInt64)value;
        public virtual UInt64 From(DateTime value) => (UInt64)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual UInt64 From(DateTimeOffset value) => (UInt64)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual UInt64 From(TimeSpan value) => (UInt64)value.TotalSeconds;
        public virtual UInt64 From(decimal value) => (UInt64)value;
        public virtual UInt64 From(double value) => (UInt64)value;
        public virtual UInt64 From(float value) => (UInt64)value;
        public virtual UInt64 From(Int16 value) => (UInt64)value;
        public virtual UInt64 From(Int32 value) => (UInt64)value;
        public virtual UInt64 From(Int64 value) => (UInt64)value;
        public virtual UInt64 From(UInt16 value) => value;
        public virtual UInt64 From(UInt32 value) => value;
        public virtual UInt64 From(UInt64 value) => value;
        public virtual UInt64 FromNull() => default(UInt64);

    }
}
