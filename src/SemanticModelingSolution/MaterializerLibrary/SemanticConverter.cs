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
    public class SemanticConverter<T> : JsonConverter<T>
    {
        private const string _arrayItemPlaceholder = "$";

        private Stack<string> _collectionElementStack;
        private Stack<JsonSegment> _sourcePath;
        private ConversionGenerator _conversionGenerator;
        private Dictionary<string, CurrentInstance> _objects;

        protected ScoredTypeMapping _map;
        protected Dictionary<string, List<ScoredPropertyMapping<ModelNavigationNode>>> _lookup = new();

        /// <summary>
        /// Important note: the instance of the JsonConverter is recycled during the same deserialization
        /// therefore anything that should not be recycled must be re-initialized in the Read/Write method
        /// </summary>
        public SemanticConverter(ScoredTypeMapping map)
        {
            _map = map;
            foreach (var propertyMapping in _map.PropertyMappings)
            {
                var sourcePath = propertyMapping.Source.GetMapPath();
                //var targetPath = propertyMapping.Target.GetMapPath();
                if (!_lookup.TryGetValue(sourcePath, out var list))
                {
                    list = new();
                    _lookup[sourcePath] = list;
                }

                list.Add(propertyMapping);
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
            _objects = new();
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

                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
                        }
                        break;

                    case JsonTokenType.EndArray:
                        {
                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
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

                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
                            if (seg != null && seg.IsArray)
                            {
                                _collectionElementStack.Push(sourcePath);
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
                        }
                        break;

                    case JsonTokenType.EndObject:
                        {
                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
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
                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
                            if (nodeMappings != null && nodeMappings.Count > 0)
                            {
                                var instances = nodeMappings.Select(nodeMapping => Materialize(nodeMapping)).ToArray();
                                var converter = _conversionGenerator.GetConverterMultiple(sourcePath, nodeMappings);
                                converter(ref reader, instances);
                            }
                            else
                            {
                                reader.Skip();
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings);
                            _sourcePath.Pop();
                            break;
                        }

                    case JsonTokenType.Number:
                        {
                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
                            if (nodeMappings != null && nodeMappings.Count > 0)
                            {
                                var instances = nodeMappings.Select(nodeMapping => Materialize(nodeMapping)).ToArray();
                                var converter = _conversionGenerator.GetConverterMultiple(sourcePath, nodeMappings);
                                converter(ref reader, instances);
                            }
                            else
                            {
                                reader.Skip();
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "(number)");
                            _sourcePath.Pop();
                            break;
                        }

                    case JsonTokenType.Null:
                        {
                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
                            if (nodeMappings != null && nodeMappings.Count > 0)
                            {
                                var instances = nodeMappings.Select(nodeMapping => Materialize(nodeMapping)).ToArray();
                                var converter = _conversionGenerator.GetConverterMultiple(sourcePath, nodeMappings);
                                converter(ref reader, instances);
                                //Debug.Assert(value == GetDefaultForType(nodeMapping.Target.ModelPropertyNode.Property.PropertyType));
                            }
                            else
                            {
                                reader.Skip();
                            }


                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "null");
                            _sourcePath.Pop();
                        }
                        break;

                    case JsonTokenType.True:
                        {
                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
                            if (nodeMappings != null && nodeMappings.Count > 0)
                            {
                                var instances = nodeMappings.Select(nodeMapping => Materialize(nodeMapping)).ToArray();
                                var converter = _conversionGenerator.GetConverterMultiple(sourcePath, nodeMappings);
                                converter(ref reader, instances);
                            }
                            else
                            {
                                reader.Skip();
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "true");
                            _sourcePath.Pop();
                        }
                        break;

                    case JsonTokenType.False:
                        {
                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
                            if (nodeMappings != null && nodeMappings.Count > 0)
                            {
                                var instances = nodeMappings.Select(nodeMapping => Materialize(nodeMapping)).ToArray();
                                var converter = _conversionGenerator.GetConverterMultiple(sourcePath, nodeMappings);
                                converter(ref reader, instances);
                            }
                            else
                            {
                                reader.Skip();
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "false");
                            _sourcePath.Pop();
                        }
                        break;

                    default:
                        {
                            Debug.Fail($"Unsupported JsonTokenType == {reader.TokenType}");
                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
                            if (nodeMappings != null && nodeMappings.Count > 0)
                            {
                                //...
                            }

                            reader.Skip();
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
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

        protected virtual void LogState(JsonTokenType jsonTokenType, int depth,
            string sourcePath, IEnumerable<ScoredPropertyMapping<ModelNavigationNode>> nodeMappings,
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

            Console.Write($"[{depth} {jsonTokenType}] {message}".PadRight(25));
            bool isFirst = true;
            if (nodeMappings != null)
            {
                foreach (var nodeMapping in nodeMappings)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        Console.Write("".PadRight(25));
                    }

                    Console.Write(sourcePath.PadRight(50));

                    var sourceType = nodeMapping.Source.ModelPropertyNode.PropertyInfo.PropertyType;
                    var targetType = nodeMapping.Target.ModelPropertyNode.PropertyInfo.PropertyType;
                    Console.Write($"{sourceType.Name} -> {targetType.Name}".PadRight(30));
                    Console.Write(nodeMapping.Target.GetMapPath());
                    Console.WriteLine();
                }
            }
            else
            {
                Console.Write(sourcePath.PadRight(50));
                Console.WriteLine();
            }

        }

        private (string sourcePath, List<ScoredPropertyMapping<ModelNavigationNode>> nodeMapping) GetSourcePathAndMapping()
        {
            var sourcePath = string.Join(".", _sourcePath.Reverse().Select(s => s.Name));
            _lookup.TryGetValue(sourcePath, out var node);
            return (sourcePath, node);
        }

        public object Materialize(ScoredPropertyMapping<ModelNavigationNode> scoredPropertyMapping)
        {
            //var sourcePath = scoredPropertyMapping.Source.GetObjectMapPath();
            //var targetPath = scoredPropertyMapping.Target.GetObjectMapPath();
            //Console.WriteLine();
            //Console.WriteLine($"Source: {sourcePath}");
            //Console.WriteLine($"Target: {targetPath}");

            var temp = scoredPropertyMapping.Target;
            bool isFirst = true;
            object result = null;
            object lastCreatedInstance = null;
            while (temp != null)
            {
                var isCollection = temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToDomain ||
                    temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToUnknown;

                object instance;

                var path = temp.GetObjectMapPath();
                if (_objects.TryGetValue(path, out CurrentInstance cached))
                {
                    var sourcePath = scoredPropertyMapping.Source.GetObjectMapPath();
                    if (sourcePath.Contains(_arrayItemPlaceholder))
                    {
                        cached.SourceCollectionElementPath = sourcePath;
                    }

                    instance = cached.Instance;
                    if (isCollection)
                    {
                        cached.AddCollectionMethod.Invoke(instance, new object[] { lastCreatedInstance });
                    }
                    else
                    {
                        temp.ModelPropertyNode.PropertyInfo.SetValue(instance, lastCreatedInstance);
                    }

                    return isFirst ? instance : result;
                }

                if (isCollection)
                {
                    var property = temp.ModelPropertyNode.PropertyInfo;
                    var collectionType = property.PropertyType;
                    instance = collectionType.CreateInstance();
                    var addMethod = collectionType.GetMethod("Add");
                    addMethod.Invoke(instance, new object[] { lastCreatedInstance });
                    _objects[path] = new CurrentInstance
                    {
                        Instance = instance,
                        IsCollection = true,
                        SourceCollectionElementPath = scoredPropertyMapping.Source.GetObjectMapPath(),
                        AddCollectionMethod = addMethod,
                    };

                    // root object:
                    // the while(...) navigation never returns the root object because
                    // the navigation relies on the properties, and there is no property pointing to the root
                    if (temp.Previous == null)
                    {
                        var rootType = temp.ModelPropertyNode.Parent.Type;
                        object rootInstance;
                        if (_objects.TryGetValue(rootType.Name, out CurrentInstance cachedRoot))
                        {
                            rootInstance = cachedRoot.Instance;
                        }
                        else
                        {
                            rootInstance = rootType.CreateInstance();
                            _objects[rootType.Name] = new CurrentInstance
                            {
                                Instance = rootInstance,
                                IsCollection = false,
                                SourceCollectionElementPath = string.Empty,
                            };
                        }

                        property.SetValue(rootInstance, instance);
                    }
                }
                else
                {
                    instance = temp.ModelPropertyNode.Parent.Type.CreateInstance();
                    var sourcePath = scoredPropertyMapping.Source.GetObjectMapPath();

                    _objects[path] = new CurrentInstance
                    {
                        Instance = instance,
                        IsCollection = false,
                        SourceCollectionElementPath = sourcePath.Contains(_arrayItemPlaceholder) ? sourcePath : string.Empty,
                    };

                    if (!isFirst)
                    {
                        var property = temp.ModelPropertyNode.PropertyInfo;
                        property.SetValue(instance, lastCreatedInstance);
                    }
                }

                //Console.WriteLine($"{parentType.Name}.{property.Name} (IsCollection: {isCollection})");

                if (isFirst)
                {
                    result = instance;
                    isFirst = false;
                }

                temp = temp.Previous;
                lastCreatedInstance = instance;
            }

            return result;
        }

        protected virtual void RemoveObjectsWithPath(string path)
        {
            var deleteKeys = _objects
                .Where(i => i.Value.SourceCollectionElementPath.StartsWith(path) && i.Key.Contains(_arrayItemPlaceholder))
                .Select(i => i.Key)
                .ToArray();

            //var deleteKeys = _objects.Keys.Where(k => k.StartsWith(path)).ToArray();
            foreach (var dk in deleteKeys)
            {
                //if (dk == path)
                //    _objects[dk].Instance = null;
                //else
                _objects.Remove(dk);
            }
        }

        protected virtual T RootResult => (T)_objects[typeof(T).Name].Instance;
        //protected virtual K CreateObject<K>() => Activator.CreateInstance<K>();
        private object CreateInstance(Type type) => Activator.CreateInstance(type);

        private record CurrentInstance
        {
            public string SourceCollectionElementPath { get; set; } = string.Empty;
            public bool IsCollection { get; set; }
            public object Instance { get; set; }
            public System.Reflection.MethodInfo AddCollectionMethod { get; set; }
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
