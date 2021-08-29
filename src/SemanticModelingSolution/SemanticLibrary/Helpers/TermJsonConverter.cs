using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SemanticLibrary.Helpers
{
    public class TermJsonConverter : JsonConverter<Term>
    {
        private DomainBase _domain;
        public TermJsonConverter(DomainBase domain)
        {
            _domain = domain;
        }

        public override Term Read(ref Utf8JsonReader reader,
            Type type, JsonSerializerOptions options)
        {
            //Console.WriteLine("OnDeserializing");

            // Don't pass in options when recursively calling Deserialize.
            var instance = JsonSerializer.Deserialize<string>(ref reader);
            var item = _domain.Links
                .Select(l => l.Term)
                .FirstOrDefault(t => t.Name == instance);
            if (item == null) item = new Term(instance);    // this is an unknown term to the Domain. Is throwing better?

            //Console.WriteLine("OnDeserialized");

            return item;
        }

        public override void Write(Utf8JsonWriter writer,
            Term instance, JsonSerializerOptions options)
        {
            //Console.WriteLine("OnSerializing");

            // Don't pass in options when recursively calling Serialize.
            JsonSerializer.Serialize(writer, instance.Name);

            //Console.WriteLine("OnSerialized");
        }
    }

}
