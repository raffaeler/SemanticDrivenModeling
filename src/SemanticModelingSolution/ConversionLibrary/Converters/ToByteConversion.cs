using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToByteConversion : ConversionBase, IConversion
    {
        private Type[] _allowed = new Type[]
        {
                typeof(bool),
                typeof(byte),typeof(sbyte),
                typeof(string),
                typeof(Int16), typeof(Int32), typeof(Int64),
                typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        private Type[] _lossyOrDangerous = new Type[]
        {
            typeof(sbyte),
            typeof(string),
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        public ToByteConversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual byte From(string value)
            => byte.TryParse(value, out byte res) ? res : (byte)_conversionContext?.OnNotSupported?.Invoke(this, value);

        public virtual byte From(bool value) => (byte)(value ? 1 : 0);
        public virtual byte From(Guid value) => (byte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual byte From(byte value) => value;
        public virtual byte From(sbyte value) => (byte)value;
        public virtual byte From(DateTime value) => (byte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual byte From(DateTimeOffset value) => (byte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual byte From(TimeSpan value) => (byte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual byte From(decimal value) => (byte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual byte From(double value) => (byte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual byte From(float value) => (byte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual byte From(Int16 value) => (byte)value;
        public virtual byte From(Int32 value) => (byte)value;
        public virtual byte From(Int64 value) => (byte)value;
        public virtual byte From(UInt16 value) => (byte)value;
        public virtual byte From(UInt32 value) => (byte)value;
        public virtual byte From(UInt64 value) => (byte)value;
        public virtual byte FromNull() => default(byte);
    }
}
