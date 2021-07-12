using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CodeGenerationLibrary.Serialization
{
    public class TesterConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            Console.WriteLine($"TestConverterFactory.CanConvert> {typeToConvert.Name}");
            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Console.WriteLine($"TestConverterFactory.CreateConverter> {typeToConvert.Name}");
            var converterType = typeof(TesterConverter<>).MakeGenericType(typeToConvert);
            var converter = Activator.CreateInstance(converterType) as JsonConverter;
            return converter;
        }
    }
}
