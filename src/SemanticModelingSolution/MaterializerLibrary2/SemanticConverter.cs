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
        private const string _arrayItemPlaceholder = "$";

        private Stack<string> _collectionElementStack;
        private Stack<JsonSegment> _sourcePath;
        private ConversionGenerator _conversionGenerator;
        private Dictionary<string, CurrentInstance> _objects;

        protected TypeSystem<Metadata> _sourceTypeSystem;
        protected TypeSystem<Metadata> _targetTypeSystem;
        protected Mapping _map;

        //ScoredPropertyMapping<ModelNavigationNode>
        protected Dictionary<string, List<NavigationPair>> _sourceLookup = new();

        /// <summary>
        /// Important note: the instance of the JsonConverter is recycled during the same deserialization
        /// therefore anything that should not be recycled must be re-initialized in the Read/Write method
        /// </summary>
        public SemanticConverter(
            TypeSystem<Metadata> sourceTypeSystem,
            TypeSystem<Metadata> targetTypeSystem,
            Mapping map)
        {
            _sourceTypeSystem = sourceTypeSystem;
            _targetTypeSystem = targetTypeSystem;
            _map = map;
            (_sourceLookup, _targetLookup) = _map.CreateLookups();

            var context = new ConversionLibrary.ConversionContext()
            {
                OnNotSupported = (converter, value) =>
                {
#if DEBUG
                    Console.WriteLine($"Conversion of a value from {value} To {converter.TargetType.Name} is not supported");
#endif
                    return converter.TargetType.GetDefaultValue();
                },
            };

            _conversionGenerator = new(context);   // the new is here in order to recycle the generator cache
        }

        protected string SourceTypeName => _map.Source.Name;
        protected bool LogObjectArrayEnabled { get; set; } = true;

        protected virtual void InitializeForEachObject()
        {
            _collectionElementStack = new();
            _sourcePath = new();
            _objects = new();
        }
/*
        // deserialization:
        // json has the format of a type described in _targetTypeSystem
        // deserialized object is part of the _sourceTypeSystem
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            //var utilities = new DeserializeUtilities<T>(_conversionGenerator);
            //var exp = utilities.CreateExpression();
            //var del = exp.Compile();
            //var instance = del(ref reader, typeToConvert, options);

            //return instance;



            Debug.Assert(SurrogateType.GetFullName(typeToConvert) == _map.Target.FullName);
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
#if DEBUG
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
                        }
                        break;

                    case JsonTokenType.EndArray:
                        {
                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
#if DEBUG
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
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

#if DEBUG
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
                        }
                        break;

                    case JsonTokenType.EndObject:
                        {
                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
#if DEBUG
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
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
#if DEBUG
                            //LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, currentProperty);
#endif
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

#if DEBUG
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings);
#endif
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

#if DEBUG
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "(number)");
#endif
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


#if DEBUG
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "null");
#endif
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

#if DEBUG
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "true");
#endif
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

#if DEBUG
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "false");
#endif
                            _sourcePath.Pop();
                        }
                        break;

                    default:
                        {
                            Debug.Fail($"Unsupported JsonTokenType == {reader.TokenType}");
                            var (sourcePath, nodeMappings) = GetSourcePathAndMapping();
                            //if (nodeMappings != null && nodeMappings.Count > 0)
                            //{
                            //    //...
                            //}

                            reader.Skip();
#if DEBUG
                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMappings, "");
#endif
                        }
                        break;
                }
            }
            while (reader.Read());

            return default(T);
        }
*/
        protected virtual void LogState(JsonTokenType jsonTokenType, int depth,
            string sourcePath, IEnumerable<NavigationPair> nodeMappings,
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


                    //var sourceType = nodeMapping.Source.ModelPropertyNode.PropertyInfo.PropertyType;
                    //var targetType = nodeMapping.Target.ModelPropertyNode.PropertyInfo.PropertyType;
                    var sourceType = nodeMapping.Source.Property.PropertyType.GetOriginalType();
                    var targetType = nodeMapping.Target.Property.PropertyType.GetOriginalType();
                    Console.Write(sourcePath.PadRight(50));
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

        private (string sourcePath, List<NavigationPair> nodeMapping) GetSourcePathAndMapping()
        {
            var sourcePath = string.Join(".", _sourcePath.Reverse().Select(s => s.Name));
            _sourceLookup.TryGetValue(sourcePath, out var node);
            return (sourcePath, node);
        }

        public object Materialize(NavigationPair navigationPair)
        {
            //var sourcePath = scoredPropertyMapping.Source.GetObjectMapPath();
            //var targetPath = scoredPropertyMapping.Target.GetObjectMapPath();
            //Console.WriteLine();
            //Console.WriteLine($"Source: {sourcePath}");
            //Console.WriteLine($"Target: {targetPath}");

            var temp = navigationPair.Target;
            bool isFirst = true;
            object result = null;
            object lastCreatedInstance = null;
            while (temp != null)
            {
                //var isCollection = temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToDomain ||
                //    temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToUnknown;
                var isCollection = temp.IsOneToMany;

                object instance;

                var path = temp.GetObjectMapPath();
                if (_objects.TryGetValue(path, out CurrentInstance cached))
                {
                    var sourcePath = navigationPair.Source.GetObjectMapPath();
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
                        temp.Property.SetValue(_sourceTypeSystem, instance, lastCreatedInstance);
                    }

                    return isFirst ? instance : result;
                }

                if (isCollection)
                {
                    var property = temp.Property.GetOriginalPropertyInfo(_sourceTypeSystem);
                    var collectionType = property.PropertyType;
                    instance = Activator.CreateInstance(collectionType);
                    var addMethod = collectionType.GetMethod("Add");
                    addMethod.Invoke(instance, new object[] { lastCreatedInstance });
                    _objects[path] = new CurrentInstance
                    {
                        Instance = instance,
                        IsCollection = true,
                        SourceCollectionElementPath = navigationPair.Source.GetObjectMapPath(),
                        AddCollectionMethod = addMethod,
                    };

                    // root object:
                    // the while(...) navigation never returns the root object because
                    // the navigation relies on the properties, and there is no property pointing to the root
                    if (temp.Previous == null)
                    {
                        var rootType = temp.Property.OwnerType;
                        object rootInstance;
                        if (_objects.TryGetValue(rootType.Name, out CurrentInstance cachedRoot))
                        {
                            rootInstance = cachedRoot.Instance;
                        }
                        else
                        {
                            rootInstance = Activator.CreateInstance(rootType.GetOriginalType());
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
                    instance = Activator.CreateInstance(temp.Property.OwnerType.GetOriginalType());
                    var sourcePath = navigationPair.Source.GetObjectMapPath();

                    _objects[path] = new CurrentInstance
                    {
                        Instance = instance,
                        IsCollection = false,
                        SourceCollectionElementPath = sourcePath.Contains(_arrayItemPlaceholder) ? sourcePath : string.Empty,
                    };

                    if (!isFirst)
                    {
                        var property = temp.Property.GetOriginalPropertyInfo(_sourceTypeSystem);
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
