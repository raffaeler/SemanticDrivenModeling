using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SemanticLibrary.Helpers
{
    public class ModelTypeNodeJsonConverter : JsonConverter<ModelTypeNode>
    {
        public override ModelTypeNode Read(ref Utf8JsonReader reader,
            Type type, JsonSerializerOptions options)
        {
            //Console.WriteLine("OnDeserializing");

            // Don't pass in options when recursively calling Deserialize.
            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Remove(this);

            var instance = JsonSerializer.Deserialize<ModelTypeNode>(ref reader, newOptions);
            foreach (var node in instance.PropertyNodes)
            {
                node.Parent = instance;
            }

            //Console.WriteLine("OnDeserialized");

            return instance;
        }

        public override void Write(Utf8JsonWriter writer,
            ModelTypeNode instance, JsonSerializerOptions options)
        {
            //Console.WriteLine("OnSerializing");

            // Don't pass in options when recursively calling Serialize.
            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Remove(this);

            JsonSerializer.Serialize(writer, instance, newOptions);

            //Console.WriteLine("OnSerialized");
        }
    }
}
