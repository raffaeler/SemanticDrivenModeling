﻿using System;
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
        protected TypeSystem<Metadata> _sourceTypeSystem;
        protected TypeSystem<Metadata> _targetTypeSystem;

        public SemanticConverterFactory(TypeSystem<Metadata> sourceTypeSystem,
            TypeSystem<Metadata> targetTypeSystem, Mapping map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

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
            //var converterType = typeof(SemanticConverter<>).MakeGenericType(typeToConvert);
            //var converter = Activator.CreateInstance(converterType,
            //    new object[] { _sourceTypeSystem, _targetTypeSystem, Map }) as JsonConverter;

            var del = GetJsonConverterCreationDelegate(typeToConvert);
            var converter = del(_sourceTypeSystem, _targetTypeSystem, Map);
            return converter;
        }

        private delegate JsonConverter CreateConverterDelegate(
            TypeSystem<Metadata> sourceTypeSystem, TypeSystem<Metadata> targetTypeSystem, Mapping mapping);

        private static Dictionary<Type, CreateConverterDelegate> _cache = new();
        private CreateConverterDelegate GetJsonConverterCreationDelegate(Type typeToConvert)
        {
            if (_cache.TryGetValue(typeToConvert, out CreateConverterDelegate del)) return del;
            var converterType = typeof(SemanticConverter<>).MakeGenericType(typeToConvert);
            var constructorInfo = converterType.GetConstructor(new Type[]
            {
                typeof(TypeSystem<Metadata>), typeof(TypeSystem<Metadata>), typeof(Mapping)
            });

            var inputSourceTypeSystem = Expression.Parameter(typeof(TypeSystem<Metadata>));
            var inputTargetTypeSystem = Expression.Parameter(typeof(TypeSystem<Metadata>));
            var inputMapping = Expression.Parameter(typeof(Mapping));
            var ctor = Expression.New(constructorInfo,
                Expression.Constant(_sourceTypeSystem),
                Expression.Constant(_targetTypeSystem),
                Expression.Constant(Map));

            var lambda = Expression.Lambda<CreateConverterDelegate>(ctor, inputSourceTypeSystem, inputTargetTypeSystem, inputMapping);
            del = lambda.Compile();
            _cache[typeToConvert] = del;
            return del;
        }
    }
}
