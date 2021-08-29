using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SemanticLibrary.Helpers
{
    public class ConceptSpecifierJsonConverter : JsonConverter<ConceptSpecifier>
    {
        private DomainBase _domain;
        private ConceptSpecifier _undefined = KnownBaseConceptSpecifiers.None;
        public ConceptSpecifierJsonConverter(DomainBase domain)
        {
            _domain = domain;
        }

        public override ConceptSpecifier Read(ref Utf8JsonReader reader,
            Type type, JsonSerializerOptions options)
        {
            //Console.WriteLine("OnDeserializing");

            // Don't pass in options when recursively calling Deserialize.
            var instance = JsonSerializer.Deserialize<string>(ref reader);
            var item = _domain.Links
                .Select(l => l.ConceptSpecifier)
                .FirstOrDefault(t => t.Name == instance);
            if (item == null) item = _undefined;

            //Console.WriteLine("OnDeserialized");

            return item;
        }

        public override void Write(Utf8JsonWriter writer,
            ConceptSpecifier instance, JsonSerializerOptions options)
        {
            //Console.WriteLine("OnSerializing");

            // Don't pass in options when recursively calling Serialize.
            JsonSerializer.Serialize(writer, instance.Name);

            //Console.WriteLine("OnSerialized");
        }
    }

}
