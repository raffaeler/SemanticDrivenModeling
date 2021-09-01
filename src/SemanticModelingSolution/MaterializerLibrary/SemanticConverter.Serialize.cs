using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SemanticLibrary;
using SemanticLibrary.Helpers;

namespace MaterializerLibrary
{
    public partial class SemanticConverter<T>
    {
        protected Dictionary<string, ScoredPropertyMapping<ModelNavigationNode>> _targetLookup = new();

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            Console.WriteLine($"TesterConverter.Write> ");
            var visitor = new ModelTypeNodeVisitor();
            visitor.Visit(_map.TargetModelTypeNode, (modelTypeNode, path) =>
            {
                Console.WriteLine($"==> Path: {path}");
                writer.WriteStartObject();
            },
            (modelTypeNode, path) =>
            {
                writer.WriteEndObject();
            },
            (modelPropertyNode, path) =>
            {
                if (!_targetLookup.TryGetValue(path, out var scoredPropertyMapping))
                {
                    Console.WriteLine($"Path: {path} unmapped");
                    return;
                }

                if (modelPropertyNode.PropertyKind == PropertyKind.OneToOneToDomain ||
                    modelPropertyNode.PropertyKind == PropertyKind.OneToOneToUnknown)
                {
                    // one-to-one
                    //Console.WriteLine("");
                    writer.WritePropertyName(modelPropertyNode.Name);
                    return;
                }

                if (modelPropertyNode.PropertyKind == PropertyKind.OneToManyToUnknown ||
                    modelPropertyNode.PropertyKind == PropertyKind.OneToManyToDomain ||
                    modelPropertyNode.PropertyKind == PropertyKind.OneToManyEnum ||
                    modelPropertyNode.PropertyKind == PropertyKind.OneToManyBasicType)
                {
                    // collection of something
                    //Console.WriteLine("");
                    writer.WritePropertyName(modelPropertyNode.Name);
                    //writer.WriteStartArray();
                    //writer.WriteEndArray();   // when?
                    return;
                }

                // basic types

                Console.WriteLine($"Path: {path}: {scoredPropertyMapping}");
            });
        }

        private void WriteBasicType(Utf8JsonWriter writer, string propertyName)
        {
            //writer.WriteNumber()
        }

    }
}
