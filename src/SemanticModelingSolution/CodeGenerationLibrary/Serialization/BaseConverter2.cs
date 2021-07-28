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
    public class BaseConverter2<T> : JsonConverter<T>
    {
        private Stack<string> _collectionElementStack;
        private Stack<JsonSegment> _sourcePath;
        private Dictionary<string, CurrentInstance> _objects;

        protected ScoredTypeMapping _map;
        protected Dictionary<string, ScoredPropertyMapping<ModelNavigationNode>> _lookup = new();

        /// <summary>
        /// Important note: the instance of the JsonConverter is recycled during the same deserialization
        /// therefore anything that should not be recycled must be re-initialized in the Read/Write method
        /// </summary>
        public BaseConverter2(ScoredTypeMapping map)
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

            //_result = CreateObject();
            //_objects[typeof(T).Name] = new CurrentInstance()
            //{
            //    Instance = _result,
            //};

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
                                _sourcePath.Push(new JsonSegment("$", true));
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
                                var instance = GetOrCreateTree(nodeMapping.Target);
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
                                var instance = GetOrCreateTree(nodeMapping.Target);
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
                                var instance = GetOrCreateTree(nodeMapping.Target);
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
                                var instance = GetOrCreateTree(nodeMapping.Target);
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
                                var instance = GetOrCreateTree(nodeMapping.Target);
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
            var sourcePath = GetPath();
            _lookup.TryGetValue(sourcePath, out var node);
            return (sourcePath, node);
        }

        protected virtual string GetPath()
        {
            return string.Join(".", _sourcePath.Reverse().Select(s => s.Name));
        }

        protected virtual T CreateObject() => Activator.CreateInstance<T>();

        private CurrentInstance GetOrCreateTree(ModelNavigationNode modelNavigationNode)
        {
            var pathToType = modelNavigationNode.GetObjectMapPath();// GetMapPath(".", true);   // unique string to the type of this property
            if (!_objects.TryGetValue(pathToType, out CurrentInstance current))
            {
                current = new CurrentInstance();
                if (modelNavigationNode.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToDomain)
                {
                    // instance is a collection
                    var collectionType = modelNavigationNode.ModelPropertyNode.Property.PropertyType;
                    current.Instance = CreateInstance(collectionType);
                    current.IsCollection = true;

                    //var collectionType = modelNavigationNode.ModelPropertyNode.Property.PropertyType;
                    //current.Collection = CreateInstance(collectionType);
                    //current.Instance = CreateInstance(modelNavigationNode.ModelPropertyNode.CoreType);
                    //var addMethod = collectionType.GetMethod("Add");
                    //addMethod.Invoke(current.Collection, new object[] { current.Instance });
                }
                else
                {
                    var ownerType = modelNavigationNode.ModelPropertyNode.Parent;
                    current.Instance = CreateInstance(ownerType.Type);
                }

                _objects[pathToType] = current;


                if (modelNavigationNode.Previous != null)
                {
                    var parentInstance = GetOrCreateTree(modelNavigationNode.Previous);
                    if (parentInstance != null)
                    {
                        if (parentInstance.IsCollection)
                        {
                            var collectionType = modelNavigationNode.Previous.ModelPropertyNode.Property.PropertyType;
                            var addMethod = collectionType.GetMethod("Add");
                            addMethod.Invoke(parentInstance.Instance, new object[] { current.Instance });
                        }
                        else
                        {
                            //parentInstance.<property> = instance;
                            var property = modelNavigationNode.Previous.ModelPropertyNode.Property;
                            property.SetValue(parentInstance.Instance, current.Instance);
                        }
                    }
                }
                else if (modelNavigationNode.ModelPropertyNode.Parent != null)
                {
                    var parent = CreateInstance(modelNavigationNode.ModelPropertyNode.Parent.Type);
                    modelNavigationNode.ModelPropertyNode.Property.SetValue(parent, current.Instance);
                    _objects[modelNavigationNode.ModelPropertyNode.Parent.Type.Name] = new CurrentInstance()
                    {
                        Instance = parent,
                        IsCollection = false,
                    };
                }
            }

            _collectionElementStack.TryPeek(out string currentCollectionElementPath);
            current.SourceCollectionElementPath = currentCollectionElementPath ?? string.Empty;

            if (current.Instance == null)
            {
                Debug.Assert(!current.IsCollection);
                // this means it is a new object inside the collection
                var ownerType = modelNavigationNode.ModelPropertyNode.Parent;
                current.Instance = CreateInstance(modelNavigationNode.ModelPropertyNode.CoreType);
                var addMethod = ownerType.Type.GetMethod("Add");
                addMethod.Invoke(current, new object[] { current.Instance });
            }

            return current;
        }

        private void RemoveObjectsWithPath(string path)
        {
            var deleteKeys = _objects
                .Where(i =>  i.Value.SourceCollectionElementPath.StartsWith(path) && i.Key.Contains("$"))
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

        private object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        protected virtual void SetValue(ModelNavigationNode source, ModelNavigationNode target, CurrentInstance instance, string value)
        {
            var sourceProperty = source.ModelPropertyNode.Property;
            var targetProperty = target.ModelPropertyNode.Property;
            if (targetProperty.PropertyType != typeof(string) && targetProperty.PropertyType != typeof(Guid))
            {
                throw new NotImplementedException($"Conversions not supported at this moment");
            }

            if (targetProperty.PropertyType == typeof(string))
            {
                targetProperty.SetValue(instance.Instance, value);
            }
            else if (targetProperty.PropertyType == typeof(Guid))
            {
                targetProperty.SetValue(instance.Instance, Guid.Parse(value));
            }
            else
            {
                throw new NotSupportedException($"The json string comes from the unsupported {sourceProperty.PropertyType.Name} type");
            }
        }

        protected virtual void SetValue(ModelNavigationNode source, ModelNavigationNode target, CurrentInstance instance, int value)
        {
            var sourceProperty = source.ModelPropertyNode.Property;
            var targetProperty = target.ModelPropertyNode.Property;
        }

        protected virtual void SetNull(ModelNavigationNode source, ModelNavigationNode target, CurrentInstance instance)
        {
            var sourceProperty = source.ModelPropertyNode.Property;
            var targetProperty = target.ModelPropertyNode.Property;

            targetProperty.SetValue(instance.Instance, null);
        }

        protected virtual void SetBoolean(ModelNavigationNode source, ModelNavigationNode target, CurrentInstance instance, bool value)
        {
            var sourceProperty = source.ModelPropertyNode.Property;
            var targetProperty = target.ModelPropertyNode.Property;
            if (targetProperty.PropertyType != typeof(bool))
            {
                throw new NotImplementedException($"Conversions not supported at this moment");
            }

            targetProperty.SetValue(instance.Instance, value);
        }



        public record CurrentInstance
        {
            public string SourceCollectionElementPath { get; set; } = string.Empty;
            //public object Collection { get; set; }
            public bool IsCollection { get; set; }
            public object Instance { get; set; }
        }
    }

    internal record JsonSegment()
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
