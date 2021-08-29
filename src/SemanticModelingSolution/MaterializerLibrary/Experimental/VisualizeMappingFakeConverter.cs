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

namespace CodeGenerationLibrary.Serialization
{
    public class VisualizeMappingFakeConverter<T> : JsonConverter<T>
    {
        private const string _arrayItemPlaceholder = "$";

        private Stack<string> _collectionElementStack;
        private Stack<JsonSegment> _sourcePath;
        private ConversionGenerator _conversionGenerator;

        protected ScoredTypeMapping _map;
        protected Dictionary<string, ScoredPropertyMapping<ModelNavigationNode>> _lookup = new();

        /// <summary>
        /// Important note: the instance of the JsonConverter is recycled during the same deserialization
        /// therefore anything that should not be recycled must be re-initialized in the Read/Write method
        /// </summary>
        public VisualizeMappingFakeConverter(ScoredTypeMapping map)
        {
            _map = map;
            foreach (var propertyMapping in _map.PropertyMappings)
            {
                var sourcePath = propertyMapping.Source.GetMapPath();
                //var targetPath = propertyMapping.Target.GetMapPath();
                _lookup[sourcePath] = propertyMapping;
            }

            var context = new ConversionLibrary.ConversionContext()
            {
                OnNotSupported = (converter, value) =>
                {
                    Console.WriteLine($"Conversion of a value from {value} To {converter.TargetType.Name} is not supported");
                    return converter.TargetType.GetDefaultForType();
                },
            };

            _conversionGenerator = new(context);   // the new is here in order to recycle the generator cache
        }

        protected string SourceTypeName => _map.SourceModelTypeNode.Type.Name;
        protected bool LogObjectArrayEnabled { get; set; } = true;

        protected virtual void InitializeForEachObject()
        {
            _collectionElementStack = new();
            _sourcePath = new();
            //_objects = new();
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert.FullName == _map.TargetModelTypeNode.Type.FullName);
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

                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, "");
                        }
                        break;

                    case JsonTokenType.EndArray:
                        {
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, "");
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
                                // element of a one-to-many
                                _sourcePath.Push(new JsonSegment(_arrayItemPlaceholder, true));
                            }
                            else
                            {
                                // element of a one-to-one
                                seg.IsObject = true;
                            }

                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            if (seg != null && seg.IsArray)
                            {
                                _collectionElementStack.Push(sourcePath);
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, "");
                        }
                        break;

                    case JsonTokenType.EndObject:
                        {
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, "");
                            var endObject = _sourcePath.Pop();
                            if (endObject.IsArrayElement)
                            {
                                var removed = _collectionElementStack.Pop();
                                RemoveObjectsWithPath(sourcePath);
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
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, currentProperty);
                        }

                        break;

                    case JsonTokenType.String:
                        {
                            string stringValue = string.Empty;
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            if (nodeMapping != null)
                            {
                                var converter = _conversionGenerator.GetValueConverter(nodeMapping);
                                var value = converter(ref reader);
                                stringValue = value.ToString();
                            }
                            else
                            {
                                reader.Skip();
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, stringValue);
                            _sourcePath.Pop();
                            break;
                        }

                    case JsonTokenType.Number:
                        {
                            string stringValue = string.Empty;
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            if (nodeMapping != null)
                            {
                                var converter = _conversionGenerator.GetValueConverter(nodeMapping);
                                var value = converter(ref reader);
                                stringValue = value.ToString();
                            }
                            else
                            {
                                reader.Skip();
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, stringValue);
                            _sourcePath.Pop();
                            break;
                        }

                    case JsonTokenType.Null:
                        {
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            if (nodeMapping != null)
                            {
                                // json reader pretends we either read or skip
                                //
                                // since the value is null, this converter will never read anything
                                // therefore in this specific case we always have to skip
                                var converter = _conversionGenerator.GetValueConverter(nodeMapping);
                                var value = converter(ref reader);
                                Debug.Assert(value == nodeMapping.Target.ModelPropertyNode.PropertyInfo.PropertyType.GetDefaultForType());
                            }

                            reader.Skip();
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, "null");
                            _sourcePath.Pop();
                        }
                        break;

                    case JsonTokenType.True:
                        {
                            string stringValue = "(true)";
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            if (nodeMapping != null)
                            {
                                var converter = _conversionGenerator.GetValueConverter(nodeMapping);
                                var value = converter(ref reader);
                                stringValue = value.ToString();
                            }
                            else
                            {
                                reader.Skip();
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, stringValue);
                            _sourcePath.Pop();
                        }
                        break;

                    case JsonTokenType.False:
                        {
                            string stringValue = "(false)";
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            if (nodeMapping != null)
                            {
                                var converter = _conversionGenerator.GetValueConverter(nodeMapping);
                                var value = converter(ref reader);
                                stringValue = value.ToString();
                            }
                            else
                            {
                                reader.Skip();
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, stringValue);
                            _sourcePath.Pop();
                        }
                        break;

                    default:
                        {
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            if (nodeMapping != null)
                            {
                                //...
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, "");
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

        protected virtual void RemoveObjectsWithPath(string path)
        {
        }

        protected virtual T RootResult => default(T);

        protected virtual void LogState(JsonTokenType jsonTokenType, int depth,
            string sourcePath, ScoredPropertyMapping<ModelNavigationNode> nodeMapping,
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
            Console.Write(sourcePath.PadRight(50));
            if (nodeMapping != null)
            {
                var sourceType = nodeMapping.Source.ModelPropertyNode.PropertyInfo.PropertyType;
                var targetType = nodeMapping.Target.ModelPropertyNode.PropertyInfo.PropertyType;
                Console.Write($"{sourceType.Name} -> {targetType.Name}".PadRight(30));
                Console.Write(nodeMapping.Target.GetMapPath());
            }

            Console.WriteLine();
        }

        private (string sourcePath, ScoredPropertyMapping<ModelNavigationNode> nodeMapping) GetSourcePathAndMapping()
        {
            var sourcePath = string.Join(".", _sourcePath.Reverse().Select(s => s.Name));
            _lookup.TryGetValue(sourcePath, out var node);
            return (sourcePath, node);
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
