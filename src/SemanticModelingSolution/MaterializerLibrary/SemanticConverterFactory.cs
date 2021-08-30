﻿using System;
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
            Console.Write($"SemanticConverterFactory.CanConvert> {typeToConvert.Name} ");
            if (Map == null || typeToConvert.FullName != Map.TargetModelTypeNode.Type.FullName)
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
            var converterType = typeof(SemanticConverter<>).MakeGenericType(typeToConvert);
            //var converterType = typeof(VisualizeMappingFakeConverter<>).MakeGenericType(typeToConvert);

            var converter = Activator.CreateInstance(converterType, new object[] { Map }) as JsonConverter;
            return converter;
        }
    }
}
