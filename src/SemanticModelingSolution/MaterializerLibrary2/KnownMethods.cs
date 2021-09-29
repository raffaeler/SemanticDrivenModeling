using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// WriteNumber:
// Param1: string (propertyName)
// Param2: decimal | double | float | Int32 | Int64 | UInt32 | UInt64

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
        private static MethodInfo _writeStringString;
        private static MethodInfo _writeStringGuid;
        private static MethodInfo _writeStringDateTime;
        private static MethodInfo _writeStringDateTimeOffset;
        private static MethodInfo _writeBoolean;

        private static MethodInfo _writeNumberDecimal;
        private static MethodInfo _writeNumberDouble;
        private static MethodInfo _writeNumberFloat;
        private static MethodInfo _writeNumberInt32;
        private static MethodInfo _writeNumberUInt32;
        private static MethodInfo _writeNumberInt64;
        private static MethodInfo _writeNumberUInt64;

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

        public static MethodInfo WriteStringString
            => _writeStringString ??= JsonWriterType.GetMethod("WriteString", new Type[] { typeof(string), typeof(string) });
        public static MethodInfo WriteStringGuid
            => _writeStringGuid ??= JsonWriterType.GetMethod("WriteString", new Type[] { typeof(string), typeof(Guid) });
        public static MethodInfo WriteStringDateTime
            => _writeStringDateTime ??= JsonWriterType.GetMethod("WriteString", new Type[] { typeof(string), typeof(DateTime) });
        public static MethodInfo WriteStringDateTimeOffset
            => _writeStringDateTimeOffset ??= JsonWriterType.GetMethod("WriteString", new Type[] { typeof(string), typeof(DateTimeOffset) });

        public static MethodInfo WriteBoolean
            => _writeBoolean ??= JsonWriterType.GetMethod("WriteBoolean", new Type[] { typeof(string), typeof(bool) });


        public static MethodInfo WriteNumberDecimal
            => _writeNumberDecimal ??= JsonWriterType.GetMethod("WriteNumber", new Type[] { typeof(string), typeof(decimal) });
        public static MethodInfo WriteNumberDouble
            => _writeNumberDouble ??= JsonWriterType.GetMethod("WriteNumber", new Type[] { typeof(string), typeof(double) });
        public static MethodInfo WriteNumberFloat
            => _writeNumberFloat ??= JsonWriterType.GetMethod("WriteNumber", new Type[] { typeof(string), typeof(float) });
        public static MethodInfo WriteNumberInt32
            => _writeNumberInt32 ??= JsonWriterType.GetMethod("WriteNumber", new Type[] { typeof(string), typeof(Int32) });
        public static MethodInfo WriteNumberUInt32
            => _writeNumberUInt32 ??= JsonWriterType.GetMethod("WriteNumber", new Type[] { typeof(string), typeof(UInt32) });
        public static MethodInfo WriteNumberInt64
            => _writeNumberInt64 ??= JsonWriterType.GetMethod("WriteNumber", new Type[] { typeof(string), typeof(Int64) });
        public static MethodInfo WriteNumberUInt64
            => _writeNumberUInt64 ??= JsonWriterType.GetMethod("WriteNumber", new Type[] { typeof(string), typeof(UInt64) });


    }
}
