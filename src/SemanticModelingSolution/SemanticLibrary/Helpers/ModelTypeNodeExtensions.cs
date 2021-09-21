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
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
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
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
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
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            };

            var item = JsonSerializer.Deserialize<ModelTypeNode>(json, options);
            //RestoreParent(visitor, item);
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
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            };

            var items = JsonSerializer.Deserialize<List<ModelTypeNode>>(json, options);
            //foreach (var item in items)
            //{
            //    RestoreParent(visitor, item);
            //}

            return items;
        }

        public static string SerializeMapping(this ScoredTypeMapping mapping, DomainBase domain)
        {
            JsonSerializerOptions options = new()
            {
                Converters =
                {
                    new ConceptJsonConverter(domain),
                    new ConceptSpecifierJsonConverter(domain),
                    new TermJsonConverter(domain),
                },
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
#if DEBUG
                WriteIndented = true,
#endif
            };

            return JsonSerializer.Serialize(mapping, options);
        }

        public static ScoredTypeMapping DeserializeMapping(string json, DomainBase domain)
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
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            };

            var mapping = JsonSerializer.Deserialize<ScoredTypeMapping>(json, options);
            //RestoreParent(visitor, mapping.SourceModelTypeNode);
            //RestoreParent(visitor, mapping.TargetModelTypeNode);

            //foreach (var propertyMapping in mapping.PropertyMappings)
            //{
            //    ModelNavigationNode temp = propertyMapping.Source;
            //    while (temp != null)
            //    {
            //        //temp.ModelPropertyNode.Parent = temp.
            //        temp = temp.Previous;
            //    }
            //    //propertyMapping.Source.ModelPropertyNode.
            //}

            return mapping;
        }

        private static void RestoreParent(ModelTypeNodeVisitor visitor, ModelTypeNode modelTypeNode)
        {
            visitor.Visit(modelTypeNode,
                (typeNode, path) =>
                {
                    foreach (var prop in typeNode.PropertyNodes)
                    {
                        prop.Parent = typeNode;
                    }
                }, (_, _) => { });
            return;
        }
    }
}
