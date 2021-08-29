using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

using SemanticLibrary.Helpers;

namespace SemanticLibrary
{
    public static class ModelTypeNodeExtensions
    {
        public static string Serialize(this ModelTypeNode modelTypeNode, DomainBase domain)
        {
            JsonSerializerOptions options = new()
            {
                Converters =
                {
                    new ConceptJsonConverter(domain),
                    new ConceptSpecifierJsonConverter(domain),
                    new TermJsonConverter(domain),
                },
#if DEBUG
                WriteIndented = true,
#endif
            };

            return JsonSerializer.Serialize(modelTypeNode, options);
        }

        public static string Serialize(this IEnumerable<ModelTypeNode> modelTypeNodes, DomainBase domain)
        {
            JsonSerializerOptions options = new()
            {
                Converters =
                {
                    new ConceptJsonConverter(domain),
                    new ConceptSpecifierJsonConverter(domain),
                    new TermJsonConverter(domain),
                },
#if DEBUG
                WriteIndented = true,
#endif
            };

            return JsonSerializer.Serialize(modelTypeNodes, options);
        }

        public static ModelTypeNode DeserializeOne(string json, DomainBase domain)
        {
            JsonSerializerOptions options = new()
            {
                Converters =
                {
                    new ModelTypeNodeJsonConverter(),
                    new ConceptJsonConverter(domain),
                    new ConceptSpecifierJsonConverter(domain),
                    new TermJsonConverter(domain),
                },
            };

            return JsonSerializer.Deserialize<ModelTypeNode>(json, options);
        }

        public static IList<ModelTypeNode> DeserializeMany(string json, DomainBase domain)
        {
            JsonSerializerOptions options = new()
            {
                Converters =
                {
                    new ModelTypeNodeJsonConverter(),
                    new ConceptJsonConverter(domain),
                    new ConceptSpecifierJsonConverter(domain),
                    new TermJsonConverter(domain),
                },
            };

            return JsonSerializer.Deserialize<List<ModelTypeNode>>(json, options);
        }
    }
}
