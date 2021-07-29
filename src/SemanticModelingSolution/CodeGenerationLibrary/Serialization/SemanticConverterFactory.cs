using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SemanticLibrary;

namespace CodeGenerationLibrary.Serialization
{
    public class SemanticConverterFactory : JsonConverterFactory
    {
        public SemanticConverterFactory(ScoredTypeMapping map)
        {
            if(map == null) throw new ArgumentNullException(nameof(map));

            this.Map = map;
        }

        public ScoredTypeMapping Map { get; }

        public override bool CanConvert(Type typeToConvert)
        {
            Console.Write($"SemanticConverterFactory.CanConvert> {typeToConvert.Name} ");
            if (Map == null || typeToConvert != Map.TargetModelTypeNode.Type)
            {
                Console.WriteLine("No");
                return false;
            }

            Console.WriteLine("Yes");
            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Console.WriteLine($"SemanticConverterFactory.CreateConverter> {typeToConvert.Name}");
            //var converterType = typeof(TesterConverter<>).MakeGenericType(typeToConvert);
            //var converterType = typeof(TesterConverter2<>).MakeGenericType(typeToConvert);
            var converterType = typeof(SemanticConverterBase<>).MakeGenericType(typeToConvert);
            var converter = Activator.CreateInstance(converterType, new object[] { Map }) as JsonConverter;
            return converter;
        }
    }
}
