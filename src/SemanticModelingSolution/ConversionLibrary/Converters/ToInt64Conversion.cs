using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToInt64Conversion : ConversionBase, IConversion
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
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        public ToInt64Conversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual Int64 From(string value)
            => Int64.TryParse(value, GetNumberStyles(), GetFormatProvider(), out Int64 res)
                ? res : (Int64)_conversionContext?.OnNotSupported?.Invoke(this, value);

        public virtual Int64 From(bool value) => (Int64)(value ? 1 : 0);
        public virtual Int64 From(Guid value) => (Int64)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Int64 From(byte value) => value;
        public virtual Int64 From(sbyte value) => value;
        public virtual Int64 From(DateTime value) => (Int64)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Int64 From(DateTimeOffset value) => (Int64)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Int64 From(TimeSpan value) => (Int64)value.TotalSeconds;
        public virtual Int64 From(decimal value) => (Int64)value;
        public virtual Int64 From(double value) => (Int64)value;
        public virtual Int64 From(float value) => (Int64)value;
        public virtual Int64 From(Int16 value) => value;
        public virtual Int64 From(Int32 value) => value;
        public virtual Int64 From(Int64 value) => value;
        public virtual Int64 From(UInt16 value) => (Int64)value;
        public virtual Int64 From(UInt32 value) => (Int64)value;
        public virtual Int64 From(UInt64 value) => (Int64)value;

    }
}
