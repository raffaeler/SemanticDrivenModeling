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
                Console.WriteLine($"********> {path} [{modelTypeNode.Type.Name}]");
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
                    Console.WriteLine($"PathProp> {path} unmapped");
                    return;
                }

                var sourcePath = scoredPropertyMapping.Source.GetMapPath();
                Console.Write($"PathProp> {path} <== Source: {sourcePath}   |   ");

                var expressions = GeneratorUtilities.CreateGetValue<T>(scoredPropertyMapping.Source);

                if (modelPropertyNode.PropertyKind.IsOneToOne())
                {
                    // one-to-one
                    Console.Write("1-1");
                    writer.WritePropertyName(modelPropertyNode.Name);
                    //return;
                }
                else if (modelPropertyNode.PropertyKind.IsOneToMany())
                {
                    // collection of something
                    Console.Write("1-Many");
                    writer.WritePropertyName(modelPropertyNode.Name);
                    //writer.WriteStartArray();
                    //writer.WriteEndArray();   // when?
                    //return;
                }
                else
                {
                    Console.Write("B");
                    writer.WritePropertyName(modelPropertyNode.Name);
                    // basic types
                }

                Console.WriteLine();
                foreach(var expression in expressions)
                {
                    Console.WriteLine($"     [E]>{expression}");
                }
            },
            (modelPropertyNode, path) =>
            {
                // begin collection
                //var sourcePath = scoredPropertyMapping.Source.GetMapPath();
                Console.WriteLine($"Start  C> {path} ");
            },
            (modelPropertyNode, path) =>
            {
                // end collection
                Console.WriteLine($"End    C> {path} ");
            });
        }

        private void WriteBasicType(Utf8JsonWriter writer, string propertyName)
        {
            //writer.WriteNumber()
        }

    }
}
