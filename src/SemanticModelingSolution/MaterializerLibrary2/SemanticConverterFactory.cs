using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SemanticLibrary;
using SurrogateLibrary;

namespace MaterializerLibrary
{
    public class SemanticConverterFactory : JsonConverterFactory
    {
        protected TypeSystem<Metadata> _sourceTypeSystem;
        protected TypeSystem<Metadata> _targetTypeSystem;

        public SemanticConverterFactory(TypeSystem<Metadata> sourceTypeSystem,
            TypeSystem<Metadata> targetTypeSystem, Mapping map)
        {
            if(map == null) throw new ArgumentNullException(nameof(map));

            _sourceTypeSystem = sourceTypeSystem;
            _targetTypeSystem = targetTypeSystem;
            this.Map = map;
        }

        public Mapping Map { get; }

        public override bool CanConvert(Type typeToConvert)
        {
#if DEBUG
            Console.Write($"SemanticConverterFactory.CanConvert> {typeToConvert.Name} ");
#endif
            var fullName = SurrogateType.GetFullName(typeToConvert);
            if (Map == null ||
                (fullName != Map.Target.FullName &&
                fullName != Map.Source.FullName))
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

            var converter = Activator.CreateInstance(converterType,
                new object[] { _sourceTypeSystem, _targetTypeSystem, Map }) as JsonConverter;
            return converter;
        }
    }
}
