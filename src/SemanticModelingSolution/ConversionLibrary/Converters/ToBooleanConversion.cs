using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionLibrary.Converters
{
    public class ToBooleanConversion : ConversionBase, IConversion
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

        public ToBooleanConversion(ConversionContext conversionContext) : base(conversionContext) { }

        public override Type TargetType => typeof(bool);
        public override Type[] LossyOrDangerous => _lossyOrDangerous;
        public virtual bool CanConvertFrom(Type type) => _allowed.Contains(type);

        public virtual bool From(string value)
            => bool.TryParse(value, out bool res) ? res : (bool)_conversionContext?.OnNotSupported?.Invoke(this, value);

        public virtual bool From(bool value) => value;
        public virtual bool From(Guid value) => (bool)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual bool From(byte value) => value != 0;
        public virtual bool From(sbyte value) => value != 0;
        public virtual bool From(DateTime value) => (bool)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual bool From(DateTimeOffset value) => (bool)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual bool From(TimeSpan value) => (bool)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual bool From(decimal value) => (bool)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual bool From(double value) => (bool)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual bool From(float value) => (bool)_conversionContext?.OnNotSupported?.Invoke(this, value);
        public virtual bool From(Int16 value) => value != 0;
        public virtual bool From(Int32 value) => value != 0;
        public virtual bool From(Int64 value) => value != 0;
        public virtual bool From(UInt16 value) => value != 0;
        public virtual bool From(UInt32 value) => value != 0;
        public virtual bool From(UInt64 value) => value != 0;
        public virtual bool FromNull() => default(bool);
    }
}
