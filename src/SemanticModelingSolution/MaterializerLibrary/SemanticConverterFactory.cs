using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SemanticLibrary;

namespace MaterializerLibrary
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
#if DEBUG
            Console.Write($"SemanticConverterFactory.CanConvert> {typeToConvert.Name} ");
#endif
            if (Map == null ||
                (typeToConvert.FullName != Map.TargetModelTypeNode.Type.FullName &&
                typeToConvert.FullName != Map.SourceModelTypeNode.Type.FullName))
            {
#if DEBUG
                Console.WriteLine("No");
#endif
                return false;
            }

#if DEBUG
            Console.WriteLine("Yes");
#endif
            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
#if DEBUG
            Console.WriteLine($"SemanticConverterFactory.CreateConverter> {typeToConvert.Name}");
#endif
            var converterType = typeof(SemanticConverter<>).MakeGenericType(typeToConvert);
            //var converterType = typeof(VisualizeMappingFakeConverter<>).MakeGenericType(typeToConvert);

            var converter = Activator.CreateInstance(converterType, new object[] { Map }) as JsonConverter;
            return converter;
        }
    }
}
