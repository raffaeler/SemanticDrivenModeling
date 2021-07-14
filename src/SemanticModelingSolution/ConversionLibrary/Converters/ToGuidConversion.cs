using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToGuidConversion : ConversionBase, IConversion
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
            typeof(byte),typeof(sbyte),
            typeof(string),
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
        };

        public ToGuidConversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual Guid From(bool value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(Guid value) => value;
        public virtual Guid From(byte value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(sbyte value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(DateTime value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(DateTimeOffset value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(TimeSpan value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(decimal value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(double value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(float value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(string value)
            => Guid.TryParseExact(value, GetGuidFormat(), out Guid res) ? res : (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(Int16 value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(Int32 value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(Int64 value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(UInt16 value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(UInt32 value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual Guid From(UInt64 value) => (Guid)_conversionContext?.OnNotSupported?.Invoke(this, value);

    }
}
