using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CodeGenerationLibrary.Serialization
{
    public class TesterConverter<T> : JsonConverter<T>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Console.WriteLine($"TesterConverter.Read> ");
            var r = reader;
            reader.Skip();


            return default(T);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            Console.WriteLine($"TesterConverter.Write> ");
        }
    }
}
