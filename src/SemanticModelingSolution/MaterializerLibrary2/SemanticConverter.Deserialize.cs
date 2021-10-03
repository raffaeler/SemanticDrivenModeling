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
        private readonly IReadOnlyCollection<NavigationPair> _emptyMappings = Array.Empty<NavigationPair>();

        //ScoredPropertyMapping<ModelNavigationNode>
        protected Dictionary<string, List<NavigationPair>> _sourceLookup;

        // key is the source path
        // values are the deletable objects
        protected Dictionary<string, HashSet<string>> _targetDeletablePaths = new();

        public interface IContainer
        {
            Type Type { get; }
        }

        private interface IContainerDebug
        {
            object ObjectItem { get; }
        }

        public class Container<K> : IContainer, IContainerDebug
        {
            public Container(K item) { Item = item; }
            public K Item { get; set; }
            public Type Type => typeof(K);
            public object ObjectItem => Item;
        }

        public Dictionary<string, IContainer> Instances = new();

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var utilities = new DeserializeUtilities<T>(_conversionGenerator, _map);
            var exp = utilities.CreateExpression();
            var del = exp.Compile();
            //var instance = del(ref reader, typeToConvert, options);

            //return instance;

            var isFinished = false;
            JsonPathStack jsonPathStack = new();

            Debug.Assert(SurrogateType.GetFullName(typeToConvert) == _map.Target.FullName);
            IEnumerable<NavigationPair> nodeMappings = null;

            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                        {
                            if (jsonPathStack.TryPeek(out JsonSourcePath path))
                            {
                                path.IsArray = true;
                            }
#if DEBUG
                            var sourcePath = jsonPathStack.CurrentPath;
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
                        }
                        break;

                    case JsonTokenType.EndArray:
                        {
#if DEBUG
                            var sourcePath = jsonPathStack.CurrentPath;
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
                            jsonPathStack.Pop();
                        }
                        break;

                    case JsonTokenType.StartObject:
                        {
                            var found = jsonPathStack.TryPeek(out JsonSourcePath path);
                            if (!found)
                            {
                                path = jsonPathStack.Push(SourceTypeName, false);
                            }
                            else if (path.IsArray)
                            {
                                // element of a one-to-many
                                jsonPathStack.Push(_arrayItemPlaceholder, true);
                            }
                            else
                            {
                                // element of a one-to-one
                                path.IsObject = true;
                            }

#if DEBUG
                            var sourcePath = jsonPathStack.CurrentPath;
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
                        }
                        break;

                    case JsonTokenType.EndObject:
                        {
                            var endObject = jsonPathStack.Pop();
                            if (endObject.IsArray)
                            {
                                RemoveInstance(endObject.Path);
                            }

                            if (jsonPathStack.Count == 0) isFinished = true;

#if DEBUG
                            var sourcePath = jsonPathStack.CurrentPath;
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif

                        }
                        break;


                    case JsonTokenType.PropertyName:
                        {
                            var currentProperty = reader.GetString();
                            jsonPathStack.Push(currentProperty);
#if DEBUG
                            var sourcePath = jsonPathStack.CurrentPath;
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, currentProperty);
#endif
                        }

                        break;

                    case JsonTokenType.String:
                        {
                            var mappings = GetMappingsFor(jsonPathStack.CurrentPath);
                            if (mappings.Count > 0)
                            {
                                var instances = mappings.Select(m => Materialize(m)).ToArray();
                                var converter = _conversionGenerator.GetConverterMultiple(jsonPathStack.CurrentPath, mappings);
                                converter(ref reader, instances);
                            }
                            else
                            {
                                reader.Skip();
                            }

#if DEBUG
                            var sourcePath = jsonPathStack.CurrentPath;
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings);
#endif
                            jsonPathStack.Pop();
                            break;
                        }

                    case JsonTokenType.Number:
                        {
                            reader.Skip();

#if DEBUG
                            var sourcePath = jsonPathStack.CurrentPath;
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "(number)");
#endif
                            jsonPathStack.Pop();
                            break;
                        }

                    case JsonTokenType.Null:
                        {
                            reader.Skip();

#if DEBUG
                            var sourcePath = jsonPathStack.CurrentPath;
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "null");
#endif
                            jsonPathStack.Pop();
                        }
                        break;

                    case JsonTokenType.True:
                        {
                            reader.Skip();

#if DEBUG
                            var sourcePath = jsonPathStack.CurrentPath;
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "true");
#endif
                            jsonPathStack.Pop();
                        }
                        break;

                    case JsonTokenType.False:
                        {
                            reader.Skip();

#if DEBUG
                            var sourcePath = jsonPathStack.CurrentPath;
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "false");
#endif
                            jsonPathStack.Pop();
                        }
                        break;

                    default:
                        {

                            reader.Skip();
#if DEBUG
                            var sourcePath = jsonPathStack.CurrentPath;
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
                        }
                        break;
                }
            }
            while (!isFinished && reader.Read());

            var returnItem = (T)((Container<object>)Instances[typeof(T).Name]).Item;
            Instances.Clear();
            return returnItem;
        }

        private IReadOnlyCollection<NavigationPair> GetMappingsFor(string sourcePath)
        {
            if (!_sourceLookup.TryGetValue(sourcePath, out var mappings))
            {
                return _emptyMappings;
            }

            return mappings;
        }

        private void RemoveInstance(string sourcePath)
        {
            if (!_targetDeletablePaths.TryGetValue(sourcePath, out var deletables)) return;

            foreach (var item in deletables)
            {
                Instances.Remove(item);
            }
        }
    }
}
