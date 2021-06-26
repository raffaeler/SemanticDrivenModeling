using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CodeGenerationLibrary.Serialization
{
    public abstract class BaseConverter<T> : JsonConverter<T>
    {
        public override bool CanConvert(Type typeToConvert) => base.CanConvert(typeToConvert);

        public override bool HandleNull => base.HandleNull;

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
