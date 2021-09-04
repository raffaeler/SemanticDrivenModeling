using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MaterializerLibrary
{
    public static class KnownMethods
    {
        public static Type JsonWriterType => typeof(Utf8JsonWriter);
        private static MethodInfo _writeNullValue;
        private static MethodInfo _writePropertyName;
        private static MethodInfo _writeStartArray;
        private static MethodInfo _writeEndArray;
        private static MethodInfo _writeStartObject0;
        private static MethodInfo _writeStartObject1;
        private static MethodInfo _writeEndObject;
        private static MethodInfo _writeString;
        private static MethodInfo _writeBoolean;

        public static MethodInfo WriteNullValue
            => _writeNullValue ??= JsonWriterType.GetMethod("WriteNullValue");

        public static MethodInfo WritePropertyName
            => _writePropertyName ??= JsonWriterType.GetMethod("WritePropertyName", new Type[] { typeof(string) });
        public static MethodInfo WriteStartArray
            => _writeStartArray ??= JsonWriterType.GetMethod("WriteStartArray", new Type[] { typeof(string) });

        public static MethodInfo WriteEndArray
            => _writeEndArray ??= JsonWriterType.GetMethod("WriteEndArray");

        public static MethodInfo WriteStartObject0
            => _writeStartObject0 ??= JsonWriterType.GetMethod("WriteStartObject", new Type[] { });
        public static MethodInfo WriteStartObject1
            => _writeStartObject1 ??= JsonWriterType.GetMethod("WriteStartObject", new Type[] { typeof(string) });

        public static MethodInfo WriteEndObject
            => _writeEndObject ??= JsonWriterType.GetMethod("WriteEndObject");

        public static MethodInfo WriteString
            => _writeString ??= JsonWriterType.GetMethod("WriteString", new Type[] { typeof(string), typeof(string) });

        public static MethodInfo WriteBoolean
            => _writeBoolean ??= JsonWriterType.GetMethod("WriteString", new Type[] { typeof(string), typeof(bool) });

    }
}
