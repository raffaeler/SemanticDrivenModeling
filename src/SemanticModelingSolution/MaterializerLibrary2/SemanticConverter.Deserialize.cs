using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SemanticLibrary;

using SurrogateLibrary;

namespace MaterializerLibrary
{
    public partial class SemanticConverter<T> : JsonConverter<T>
    {
        public record SourcePath
        {
            public SourcePath(string path, bool isArrayElement = false)
                => (Path, IsArray) = (path, isArrayElement);

            public string Path { get; init; }
            public bool IsObject { get; set; }
            public bool IsArray { get; set; }
            public bool IsArrayElement { get; set; }
        }

        private Stack<SourcePath> _sourcePaths = new();
        private bool _isFinished;

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            //var utilities = new DeserializeUtilities<T>(_conversionGenerator);
            //var exp = utilities.CreateExpression();
            //var del = exp.Compile();
            //var instance = del(ref reader, typeToConvert, options);

            //return instance;

            _isFinished = false;
            _sourcePaths.Clear();

            Debug.Assert(SurrogateType.GetFullName(typeToConvert) == _map.Target.FullName);
            InitializeForEachObject();
            IEnumerable<NavigationPair> nodeMappings = null;

            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                        {
                            if (_sourcePaths.TryPeek(out SourcePath path))
                            {
                                path.IsArray = true;
                            }

#if DEBUG
                            var sourcePath = String.Join(".", _sourcePaths.Reverse().Select(s => s.Path));
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
                        }
                        break;

                    case JsonTokenType.EndArray:
                        {
#if DEBUG
                            var sourcePath = String.Join(".", _sourcePaths.Reverse().Select(s => s.Path));
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
                            _sourcePaths.Pop();
                        }
                        break;

                    case JsonTokenType.StartObject:
                        {
                            var found = _sourcePaths.TryPeek(out SourcePath path);
                            if (!found)
                            {
                                _sourcePaths.Push(new SourcePath(SourceTypeName, false));
                            }
                            else if (path.IsArray)
                            {
                                // element of a one-to-many
                                _sourcePaths.Push(new SourcePath(_arrayItemPlaceholder, true));
                            }
                            else
                            {
                                // element of a one-to-one
                                path.IsObject = true;
                            }

#if DEBUG
                            var sourcePath = String.Join(".", _sourcePaths.Reverse().Select(s => s.Path));
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
                        }
                        break;

                    case JsonTokenType.EndObject:
                        {
                            var endObject = _sourcePaths.Pop();

                            // todo

                            if (_sourcePaths.Count == 0) _isFinished = true;

#if DEBUG
                            var sourcePath = String.Join(".", _sourcePaths.Reverse().Select(s => s.Path));
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif

                        }
                        break;


                    case JsonTokenType.PropertyName:
                        {
                            var currentProperty = reader.GetString();
                            _sourcePaths.Push(new(currentProperty));
#if DEBUG
                            var sourcePath = String.Join(".", _sourcePaths.Reverse().Select(s => s.Path));
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, currentProperty);
#endif
                        }

                        break;

                    case JsonTokenType.String:
                        {
                            reader.Skip();

#if DEBUG
                            var sourcePath = String.Join(".", _sourcePaths.Reverse().Select(s => s.Path));
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings);
#endif
                            _sourcePaths.Pop();
                            break;
                        }

                    case JsonTokenType.Number:
                        {
                            reader.Skip();

#if DEBUG
                            var sourcePath = String.Join(".", _sourcePaths.Reverse().Select(s => s.Path));
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "(number)");
#endif
                            _sourcePaths.Pop();
                            break;
                        }

                    case JsonTokenType.Null:
                        {
                            reader.Skip();

#if DEBUG
                            var sourcePath = String.Join(".", _sourcePaths.Reverse().Select(s => s.Path));
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "null");
#endif
                            _sourcePath.Pop();
                        }
                        break;

                    case JsonTokenType.True:
                        {
                            reader.Skip();

#if DEBUG
                            var sourcePath = String.Join(".", _sourcePaths.Reverse().Select(s => s.Path));
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "true");
#endif
                            _sourcePath.Pop();
                        }
                        break;

                    case JsonTokenType.False:
                        {
                            reader.Skip();

#if DEBUG
                            var sourcePath = String.Join(".", _sourcePaths.Reverse().Select(s => s.Path));
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "false");
#endif
                            _sourcePath.Pop();
                        }
                        break;

                    default:
                        {

                            reader.Skip();
#if DEBUG
                            var sourcePath = String.Join(".", _sourcePaths.Reverse().Select(s => s.Path));
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
                        }
                        break;
                }
            }
            while (!_isFinished && reader.Read());

            return default(T);
        }

    }
}
