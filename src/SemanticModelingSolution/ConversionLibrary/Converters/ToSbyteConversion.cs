using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToSbyteConversion : ConversionBase, IConversion
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
            typeof(byte),
            typeof(string),
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        public ToSbyteConversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual sbyte From(string value)
            => sbyte.TryParse(value, out sbyte res) 
                ? res : (sbyte)_conversionContext?.OnNotSupported?.Invoke(this, value);

        public virtual sbyte From(bool value) => (sbyte)(value ? 1 : 0);
        public virtual sbyte From(Guid value) => (sbyte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual sbyte From(byte value) => (sbyte)value;
        public virtual sbyte From(sbyte value) => value;
        public virtual sbyte From(DateTime value) => (sbyte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual sbyte From(DateTimeOffset value) => (sbyte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual sbyte From(TimeSpan value) => (sbyte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual sbyte From(decimal value) => (sbyte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual sbyte From(double value) => (sbyte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual sbyte From(float value) => (sbyte)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual sbyte From(Int16 value) => (sbyte)value;
        public virtual sbyte From(Int32 value) => (sbyte)value;
        public virtual sbyte From(Int64 value) => (sbyte)value;
        public virtual sbyte From(UInt16 value) => (sbyte)value;
        public virtual sbyte From(UInt32 value) => (sbyte)value;
        public virtual sbyte From(UInt64 value) => (sbyte)value;
        public virtual sbyte FromNull() => default(sbyte);

    }
}
