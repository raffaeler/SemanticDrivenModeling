using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public SemanticConverterFactory(TypeSystem<Metadata> typeSystem, Mapping map)
            : this(new[] { typeSystem }, new[] { map })
        {
            if (typeSystem == null) throw new ArgumentNullException(nameof(typeSystem));
            if (map == null) throw new ArgumentNullException(nameof(map));
        }

        public SemanticConverterFactory(IEnumerable<TypeSystem<Metadata>> typeSystems, IEnumerable<Mapping> maps)
        {
            if (typeSystems == null) throw new ArgumentNullException(nameof(typeSystems));
            if (maps == null) throw new ArgumentNullException(nameof(maps));

            this.TypeSystems = typeSystems;
            this.Maps = maps;
        }

        public IEnumerable<TypeSystem<Metadata>> TypeSystems { get; }
        public IEnumerable<Mapping> Maps { get; }

        public override bool CanConvert(Type typeToConvert)
        {
#if DEBUG
            Console.Write($"SemanticConverterFactory.CanConvert> {typeToConvert.Name} ");
#endif
            var fullName = SurrogateType.GetFullName(typeToConvert);
            var isInMaps = Maps.Any(m =>
                    m.Target.FullName == fullName ||
                    m.Source.FullName == fullName);

            var isInTypeSystems = TypeSystems.Any(t =>
                     t.TypesByFullName.ContainsKey(fullName));

            if (isInMaps && isInTypeSystems)
            {
#if DEBUG
                Console.WriteLine("Yes");
#endif
                return true;
            }

#if DEBUG
            Console.WriteLine("No");
#endif
            return false;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
#if DEBUG
            Console.WriteLine($"SemanticConverterFactory.CreateConverter> {typeToConvert.Name}");
#endif

            var context = new ConversionLibrary.ConversionContext()
            {
                OnNotSupported = (converter, value) =>
                {
#if DEBUG
                    Console.WriteLine($"Conversion of a value from {value} To {converter.TargetType.Name} is not supported");
#endif
                    return converter.TargetType.GetDefaultValue();
                },
            };

            SemanticConverterParameters pars = new(typeToConvert, TypeSystems, Maps, context);

            //var converterType = typeof(SemanticConverter<>).MakeGenericType(typeToConvert);
            //var converter = Activator.CreateInstance(converterType,
            //    new object[] { _destinationTypeSystem, Map }) as JsonConverter;

            var del = GetJsonConverterCreationDelegate(typeToConvert);
            var converter = del(pars);
            return converter;
        }

        private delegate JsonConverter CreateConverterDelegate(
            SemanticConverterParameters parameters);

        private static Dictionary<Type, CreateConverterDelegate> _cache = new();
        private CreateConverterDelegate GetJsonConverterCreationDelegate(Type typeToConvert)
        {
            if (_cache.TryGetValue(typeToConvert, out CreateConverterDelegate del)) return del;

            var converterType = typeof(SemanticConverter<>).MakeGenericType(typeToConvert);
            var constructorInfo = converterType.GetConstructor(new Type[]
            {
                typeof(SemanticConverterParameters)
            });

            var inputParameters = Expression.Parameter(typeof(SemanticConverterParameters));
            var ctor = Expression.New(constructorInfo,
                inputParameters);

            var lambda = Expression.Lambda<CreateConverterDelegate>(ctor, inputParameters);
            del = lambda.Compile();
            _cache[typeToConvert] = del;
            return del;
        }
    }
}
