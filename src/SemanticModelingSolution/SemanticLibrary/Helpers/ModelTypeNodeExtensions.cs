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
            ModelTypeNodeVisitor visitor = new();

            JsonSerializerOptions options = new()
            {
                Converters =
                {
                    new ConceptJsonConverter(domain),
                    new ConceptSpecifierJsonConverter(domain),
                    new TermJsonConverter(domain),
                },
            };

            var item = JsonSerializer.Deserialize<ModelTypeNode>(json, options);
            RestoreParent(visitor, item);
            return item;
        }

        public static IList<ModelTypeNode> DeserializeMany(string json, DomainBase domain)
        {
            ModelTypeNodeVisitor visitor = new();

            JsonSerializerOptions options = new()
            {
                Converters =
                {
                    new ConceptJsonConverter(domain),
                    new ConceptSpecifierJsonConverter(domain),
                    new TermJsonConverter(domain),
                },
            };

            var items = JsonSerializer.Deserialize<List<ModelTypeNode>>(json, options);
            foreach (var item in items)
            {
                RestoreParent(visitor, item);
            }

            return items;
        }

        private static void RestoreParent(ModelTypeNodeVisitor visitor, ModelTypeNode modelTypeNode)
        {
            visitor.Visit(modelTypeNode,
                typeNode =>
                {
                    foreach (var prop in typeNode.PropertyNodes)
                    {
                        prop.Parent = typeNode;
                    }
                }, _ => { });
            return;
        }
    }
}
