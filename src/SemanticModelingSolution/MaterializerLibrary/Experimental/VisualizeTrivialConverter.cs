using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SemanticLibrary;

namespace MaterializerLibrary
{
    public class VisualizeTrivialConverter<T> : JsonConverter<T>
    {
        private const string _arrayItemPlaceholder = "$";

        private Stack<string> _collectionElementStack;
        private Stack<JsonSegment> _sourcePath;
        private ConversionGenerator _conversionGenerator;

        /// <summary>
        /// Important note: the instance of the JsonConverter is recycled during the same deserialization
        /// therefore anything that should not be recycled must be re-initialized in the Read/Write method
        /// </summary>
        public VisualizeTrivialConverter()
        {
            var context = new ConversionLibrary.ConversionContext()
            {
                OnNotSupported = (converter, value) =>
                {
                    Console.WriteLine($"Conversion of a value from {value} To {converter.TargetType.Name} is not supported");
                    return converter.TargetType.IsValueType ? Activator.CreateInstance(converter.TargetType) : null;
                },
            };

            _conversionGenerator = new(context);   // the new is here in order to recycle the generator cache
        }

        protected string SourceTypeName => typeof(T).Name;
        protected bool LogObjectArrayEnabled { get; set; } = true;

        protected virtual void InitializeForEachObject()
        {
            _collectionElementStack = new();
            _sourcePath = new();
            //_objects = new();
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            InitializeForEachObject();

            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                        {
                            if (_sourcePath.TryPeek(out JsonSegment segment))
                            {
                                segment.IsArray = true;
                            }
                            else
                            {
                                Debug.Fail("This converter does not accept root of kind array");
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, "");
                        }
                        break;

                    case JsonTokenType.EndArray:
                        {
                            LogState(reader.TokenType, reader.CurrentDepth, "");
                            _sourcePath.Pop();
                        }
                        break;

                    case JsonTokenType.StartObject:
                        {
                            var found = _sourcePath.TryPeek(out JsonSegment seg);
                            if (!found)
                            {
                                _sourcePath.Push(new JsonSegment(SourceTypeName, false));
                            }
                            else if (seg.IsArray)
                            {
                                // element of a many-to-many
                                _sourcePath.Push(new JsonSegment(_arrayItemPlaceholder, true));
                            }
                            else
                            {
                                // element of a one-to-one
                                seg.IsObject = true;
                            }

                            if (seg != null && seg.IsArray)
                            {
                                //_collectionElementStack.Push(sourcePath);
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, "");
                        }
                        break;

                    case JsonTokenType.EndObject:
                        {
                            LogState(reader.TokenType, reader.CurrentDepth, "");
                            var endObject = _sourcePath.Pop();
                            if (endObject.IsArrayElement)
                            {
                                var removed = _collectionElementStack.Pop();
                                //RemoveObjectsWithPath(sourcePath);
                            }

                            if (_sourcePath.Count == 0)
                            {
                                return RootResult;
                            }
                        }
                        break;


                    case JsonTokenType.PropertyName:
                        {
                            var currentProperty = reader.GetString();
                            _sourcePath.Push(new JsonSegment(currentProperty));
                            //var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            //LogState(reader.TokenType, reader.CurrentDepth, currentProperty);
                        }

                        break;

                    case JsonTokenType.String:
                        {
                            string stringValue = reader.GetString();
                            LogState(reader.TokenType, reader.CurrentDepth, stringValue);
                            _sourcePath.Pop();
                            break;
                        }

                    case JsonTokenType.Number:
                        {
                            string stringValue = string.Empty;
                            reader.Skip();

                            LogState(reader.TokenType, reader.CurrentDepth, "(number)");
                            _sourcePath.Pop();
                            break;
                        }

                    case JsonTokenType.Null:
                        {
                            reader.Skip();

                            LogState(reader.TokenType, reader.CurrentDepth, "null");
                            _sourcePath.Pop();
                        }
                        break;

                    case JsonTokenType.True:
                        {
                            string stringValue = "(true)";
                            reader.Skip();

                            LogState(reader.TokenType, reader.CurrentDepth, stringValue);
                            _sourcePath.Pop();
                        }
                        break;

                    case JsonTokenType.False:
                        {
                            string stringValue = "(false)";
                            reader.Skip();

                            LogState(reader.TokenType, reader.CurrentDepth, stringValue);
                            _sourcePath.Pop();
                        }
                        break;

                    default:
                        {
                            LogState(reader.TokenType, reader.CurrentDepth, "");
                        }
                        break;
                }
            }
            while (reader.Read());

            return default(T);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // not supported
            Console.WriteLine($"TesterConverter.Write> ");
            throw new NotSupportedException("This converter can only be used to deserialize");
        }


        protected virtual T RootResult => default(T);

        protected virtual void LogState(JsonTokenType jsonTokenType, int depth,
            string message = null)
        {
            if (!LogObjectArrayEnabled && (
                jsonTokenType == JsonTokenType.StartArray ||
                jsonTokenType == JsonTokenType.EndArray ||
                jsonTokenType == JsonTokenType.StartObject ||
                jsonTokenType == JsonTokenType.EndObject))
            {
                return;
            }

            Console.Write($"[{depth} {jsonTokenType}] {message}".PadRight(60));
            Console.WriteLine();
        }

        private record JsonSegment()
        {
            public JsonSegment(string name, bool isArrayElement = false)
                : this()
            {
                this.Name = name;
                this.IsObject = false;
                this.IsArray = false;
                this.IsArrayElement = isArrayElement;
            }

            public static JsonSegment FromArray() => new JsonSegment()
            {
                IsArray = true,
            };

            public string Name { get; set; }
            public bool IsObject { get; set; }
            public bool IsArray { get; set; }
            public bool IsArrayElement { get; set; }
        }
    }
}
