using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SemanticLibrary;

namespace CodeGenerationLibrary.Serialization
{
    public class SemanticConverterBase<T> : JsonConverter<T>
    {
        private const string _arrayItemPlaceholder = "$";

        private Stack<string> _collectionElementStack;
        private Stack<JsonSegment> _sourcePath;
        private Dictionary<string, CurrentInstance> _objects;

        protected ScoredTypeMapping _map;
        protected Dictionary<string, ScoredPropertyMapping<ModelNavigationNode>> _lookup = new();

        /// <summary>
        /// Important note: the instance of the JsonConverter is recycled during the same deserialization
        /// therefore anything that should not be recycled must be re-initialized in the Read/Write method
        /// </summary>
        public SemanticConverterBase(ScoredTypeMapping map)
        {
            _map = map;
            foreach (var propertyMapping in _map.PropertyMappings)
            {
                var sourcePath = propertyMapping.Source.GetMapPath();
                //var targetPath = propertyMapping.Target.GetMapPath();
                _lookup[sourcePath] = propertyMapping;
            }
        }

        protected Type SourceType => _map.SourceModelTypeNode.Type;
        protected bool LogObjectArrayEnabled { get; set; } = true;

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == _map.TargetModelTypeNode.Type);
            _collectionElementStack = new();
            _sourcePath = new();
            _objects = new();

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
                                _sourcePath.Push(new JsonSegment(SourceType.Name, false));
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
                                return (T)_objects[typeof(T).Name].Instance;
                                //return _result;
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
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            var value = reader.GetString();
                            if (nodeMapping != null)
                            {
                                //var instance = GetOrCreateTree(nodeMapping.Target);
                                var instance = Materialize(nodeMapping);
                                SetValue(nodeMapping.Source, nodeMapping.Target, instance, value);
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, value);
                            _sourcePath.Pop();
                            break;
                        }

                    case JsonTokenType.Number:
                        {
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            reader.Skip();
                            if (nodeMapping != null)
                            {
                                //var instance = GetOrCreateTree(nodeMapping.Target);
                                var instance = Materialize(nodeMapping);
                                //SetValue(nodeMapping.Source, nodeMapping.Target, instance, ...);
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, "(number)");
                            _sourcePath.Pop();
                            break;
                        }

                    case JsonTokenType.Null:
                        {
                            // I assume not to call Skip when the value is null // TODO: verify

                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            if (nodeMapping != null)
                            {
                                //var instance = GetOrCreateTree(nodeMapping.Target);
                                var instance = Materialize(nodeMapping);
                                SetNull(nodeMapping.Source, nodeMapping.Target, instance);
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, "null");
                            _sourcePath.Pop();
                        }
                        break;

                    case JsonTokenType.True:
                        {
                            // skip???
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            if (nodeMapping != null)
                            {
                                //var instance = GetOrCreateTree(nodeMapping.Target);
                                var instance = Materialize(nodeMapping);
                                SetBoolean(nodeMapping.Source, nodeMapping.Target, instance, true);
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, "true");
                            _sourcePath.Pop();
                        }
                        break;

                    case JsonTokenType.False:
                        {
                            // skip???
                            var (sourcePath, nodeMapping) = GetSourcePathAndMapping();
                            if (nodeMapping != null)
                            {
                                //var instance = GetOrCreateTree(nodeMapping.Target);
                                var instance = Materialize(nodeMapping);
                                SetBoolean(nodeMapping.Source, nodeMapping.Target, instance, false);
                            }

                            LogState(reader.TokenType, reader.CurrentDepth, sourcePath, nodeMapping, "false");
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
                var sourceType = nodeMapping.Source.ModelPropertyNode.Property.PropertyType;
                var targetType = nodeMapping.Target.ModelPropertyNode.Property.PropertyType;
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
                        temp.ModelPropertyNode.Property.SetValue(instance, lastCreatedInstance);
                    }

                    return isFirst ? instance : result;
                }

                if (isCollection)
                {
                    var property = temp.ModelPropertyNode.Property;
                    var collectionType = property.PropertyType;
                    instance = CreateInstance(collectionType);
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
                            rootInstance = CreateInstance(rootType);
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
                    var parentType = temp.ModelPropertyNode.Parent.Type;
                    instance = CreateInstance(parentType);
                    var sourcePath = scoredPropertyMapping.Source.GetObjectMapPath();

                    _objects[path] = new CurrentInstance
                    {
                        Instance = instance,
                        IsCollection = false,
                        SourceCollectionElementPath = sourcePath.Contains(_arrayItemPlaceholder) ? sourcePath : string.Empty,
                    };

                    if (!isFirst)
                    {
                        var property = temp.ModelPropertyNode.Property;
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

        private void RemoveObjectsWithPath(string path)
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

        protected virtual K CreateObject<K>() => Activator.CreateInstance<K>();
        private object CreateInstance(Type type) => Activator.CreateInstance(type);

        protected virtual void SetValue(ModelNavigationNode source, ModelNavigationNode target, object instance, string value)
        {
            var sourceProperty = source.ModelPropertyNode.Property;
            var targetProperty = target.ModelPropertyNode.Property;
            if (targetProperty.PropertyType != typeof(string) && targetProperty.PropertyType != typeof(Guid))
            {
                throw new NotImplementedException($"Conversions not supported at this moment");
            }

            if (targetProperty.PropertyType == typeof(string))
            {
                targetProperty.SetValue(instance, value);
            }
            else if (targetProperty.PropertyType == typeof(Guid))
            {
                targetProperty.SetValue(instance, Guid.Parse(value));
            }
            else
            {
                throw new NotSupportedException($"The json string comes from the unsupported {sourceProperty.PropertyType.Name} type");
            }
        }

        protected virtual void SetValue(ModelNavigationNode source, ModelNavigationNode target, object instance, int value)
        {
            var sourceProperty = source.ModelPropertyNode.Property;
            var targetProperty = target.ModelPropertyNode.Property;
        }

        protected virtual void SetNull(ModelNavigationNode source, ModelNavigationNode target, object instance)
        {
            var sourceProperty = source.ModelPropertyNode.Property;
            var targetProperty = target.ModelPropertyNode.Property;

            targetProperty.SetValue(instance, null);
        }

        protected virtual void SetBoolean(ModelNavigationNode source, ModelNavigationNode target, object instance, bool value)
        {
            var sourceProperty = source.ModelPropertyNode.Property;
            var targetProperty = target.ModelPropertyNode.Property;
            if (targetProperty.PropertyType != typeof(bool))
            {
                throw new NotImplementedException($"Conversions not supported at this moment");
            }

            targetProperty.SetValue(instance, value);
        }


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
