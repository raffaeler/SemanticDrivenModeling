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
    public class TesterConverter2<T> : JsonConverter<T>
    {
        private Stack<string> _sourcePath = new Stack<string>();

        private Dictionary<string, CurrentInstance> _objects = new Dictionary<string, CurrentInstance>();
        private Stack<MappingInfo> _collectionStack = new Stack<MappingInfo>();
        private Stack<MappingInfo> _objectsStack = new Stack<MappingInfo>();

        private Stack<string> _stack = new Stack<string>();
        private readonly ScoredTypeMapping _map;
        private string _currentProperty;
        private ScoredPropertyMapping<ModelNavigationNode> _currentMapping;

        private int _rootLevel;
        private string _rootKey;

        public TesterConverter2(ScoredTypeMapping map)
        {
            _map = map;
            // root = _map.TargetModelTypeNode.TypeName
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            _rootLevel = reader.CurrentDepth;
            _rootKey = _map.TargetModelTypeNode.Type.Name;

            do
            {
                //Log(ref reader);
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                        _sourcePath.Push("[]");

                        Console.WriteLine($"-- Start Array D:{reader.CurrentDepth} --");
                        _collectionStack.Push(new MappingInfo()
                        {
                            PropertyName = _currentProperty,
                            Mapping = _currentMapping,
                        });
                        break;
                    case JsonTokenType.EndArray:
                        Console.WriteLine($"-- End Array D:{reader.CurrentDepth} --");
                        {
                            var info = _collectionStack.Pop();
                            // all the object under the following path (or deeper) must be removed because
                            // they have already been filled
                            var path = _currentMapping.Target.GetMapPath(".", true);
                        }
                        _sourcePath.Pop();
                        break;

                    case JsonTokenType.StartObject:
                        if (_sourcePath.Count == 0)
                        {
                            _sourcePath.Push(typeToConvert.Name);
                        }
                        else
                        {
                            _sourcePath.Push("o");
                        }

                        Console.WriteLine($"---- Start Object {StackToPath(_sourcePath)} D:{reader.CurrentDepth} ----");
                        _objectsStack.Push(new MappingInfo());
                        _stack.Push(_currentProperty);

                        if (_currentMapping != null)
                        {
                            //_currentMapping.Target.
                        }
                        break;
                    case JsonTokenType.EndObject:
                        Console.WriteLine($"---- End Object {StackToPath(_sourcePath)} D:{reader.CurrentDepth} ----");
                        {
                            var info = _objectsStack.Pop();
                            var path = info.Mapping.Target.GetMapPath(".", true);   // unique string to the type of this property
                            var current = _objects[path];
                            if (_objectsStack.Count == 0)
                            {
                                _sourcePath.Pop();
                                return (T)current.Instance;
                            }

                            if (current.Collection != null)
                            {
                                // the object was inside the collection
                                // now that we have removed all the objects, we have to put the collection back
                                // in the _objects, but without the current item
                                RemoveObjectsWithPath(path);
                                current.Instance = null;
                                _objects[path] = current;
                            }
                        }

                        _sourcePath.Pop();

                        if (_stack.Count == 0) return default(T);
                        _stack.Pop();
                        break;


                    case JsonTokenType.PropertyName:
                        _currentProperty = reader.GetString();
                        _sourcePath.Push(_currentProperty);

                        // todo: full path comparison as multiple properties may have the same PropertyName (belonging to different objects)
                        _currentMapping = _map.PropertyMappings.FirstOrDefault(p => p.Source.Name == _currentProperty);
                        {
                            var info = _objectsStack.Peek();
                            if (info.Mapping == null)
                            {
                                info.PropertyName = _currentProperty;
                                info.Mapping = _currentMapping;
                            }
                        }
                        //if (isNewArray)
                        //{
                        //    //_currentMapping.Target.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyBasicType
                        //    //_currentMapping.Target.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToUnknown
                        //    if (_currentMapping.Target.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToDomain)
                        //    {

                        //    }

                        //    isNewArray = false;
                        //}
                        break;

                    case JsonTokenType.String:
                        {
                            var value = reader.GetString();
                            if (_currentMapping == null)
                            {
                                // no mapping, skipping this property
                                Log(ref reader, value);
                                _sourcePath.Pop();
                                break;
                            }

                            var currentInstance = GetOrCreateTree(_currentMapping.Target);
                            //var path = _currentMapping.Target.GetMapPath(".", true);
                            //if(!_objects.TryGetValue(path, out object instance))
                            //{
                            //    instance = CreateInstance(_currentMapping.Target.ModelPropertyNode.Parent.Type);
                            //    _objects[path] = instance;
                            //}

                            var currentProperty = _currentMapping.Target.ModelPropertyNode.Property;
                            // conversion goes here
                            if (currentProperty.PropertyType == _currentMapping.Source.ModelPropertyNode.Property.PropertyType)
                            {
                                if (currentProperty.PropertyType.Is(typeof(Guid)))
                                {
                                    var g = Guid.Parse(value);
                                    currentProperty.SetValue(currentInstance.Instance, g);
                                }
                                else
                                {
                                    currentProperty.SetValue(currentInstance.Instance, value);
                                }
                            }

                            _sourcePath.Pop();
                            Log(ref reader, value);
                            break;
                        }
                    case JsonTokenType.Number:
                        {
                            var value = reader.GetInt32();
                            _sourcePath.Pop();
                            Log(ref reader, value);
                            break;
                        }
                    case JsonTokenType.Null:
                        //reader.Skip();
                        _sourcePath.Pop();
                        Log(ref reader, "null  ");
                        break;
                    case JsonTokenType.True:
                        //reader.Skip();
                        _sourcePath.Pop();
                        Log(ref reader, "true  ");
                        break;
                    case JsonTokenType.False:
                        //reader.Skip();
                        _sourcePath.Pop();
                        Log(ref reader, "false ");
                        break;

                    case JsonTokenType.None:
                        break;
                    case JsonTokenType.Comment:
                        break;
                }
            }
            while (reader.Read());


            //var r = reader;
            //reader.Skip();


            return default(T);
            //return (T)_currentInstance;
        }

        //private Type GetObjectType()
        //{
        //    if (_currentMapping == null)
        //    {
        //        return _map.TargetModelTypeNode.Type;
        //    }

        //    return _currentMapping.Target.ModelPropertyNode.Property.PropertyType;
        //}

        private object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        private CurrentInstance GetOrCreateTree(ModelNavigationNode modelNavigationNode)
        {
            var pathToType = modelNavigationNode.GetMapPath(".", true);   // unique string to the type of this property
            if (!_objects.TryGetValue(pathToType, out CurrentInstance current))
            {
                var ownerType = modelNavigationNode.ModelPropertyNode.Parent;
                current = new CurrentInstance();
                if (modelNavigationNode.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToDomain)
                {
                    // instance is a collection
                    current.Collection = ownerType.Type.CreateInstance();
                    current.Instance = modelNavigationNode.ModelPropertyNode.CoreType.CreateInstance();
                    var addMethod = ownerType.Type.GetOriginalType().GetMethod("Add");
                    addMethod.Invoke(current, new object[] { current.Instance });
                }
                else
                {
                    current.Instance = ownerType.Type.CreateInstance();
                }

                _objects[pathToType] = current;


                if (modelNavigationNode.Previous != null)
                {
                    var parentInstance = GetOrCreateTree(modelNavigationNode.Previous);
                    if (parentInstance != null)
                    {
                        //parentInstance.<property> = instance;
                        var property = modelNavigationNode.Previous.ModelPropertyNode.Property;
                        property.SetValue(parentInstance.Instance, current.Instance);
                    }
                }
            }

            if (current.Instance == null)
            {
                Debug.Assert(current.Collection != null);
                // this means it is a new object inside the collection
                var ownerType = modelNavigationNode.ModelPropertyNode.Parent;
                current.Instance = modelNavigationNode.ModelPropertyNode.CoreType.CreateInstance();
                var addMethod = ownerType.Type.GetOriginalType().GetMethod("Add");
                addMethod.Invoke(current, new object[] { current.Instance });
            }

            return current;
        }

        public void RemoveObjectsWithPath(string path)
        {
            var deleteKeys = _objects.Keys.Where(k => k.StartsWith(path)).ToArray();
            foreach (var dk in deleteKeys)
            {
                //if (dk == path)
                //    _objects[dk].Instance = null;
                //else
                _objects.Remove(dk);
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // not supported
            Console.WriteLine($"TesterConverter.Write> ");
            throw new NotSupportedException("This converter can only be used to deserialize");
        }

        public void Log(ref Utf8JsonReader reader, object value, string message = null)
        {
            //var pos = reader.Position.GetInteger();
            //var s1 = $"{9:000}";
            //var s2 = $"P:{9:000}";
            //var s3 = $"P:{reader.Position.GetInteger():000}";
            //var s4 = $"P:{reader.Position.GetInteger():000}";
            //var s5 = $"P:{pos:000}";

            var from = $"{_currentProperty}:{value}";
            var to = $"{_currentMapping?.Target.GetMapPath()}";
            var targetType = (_currentMapping?.Target.ModelPropertyNode.Property.PropertyType.Name) ?? "";

            if (string.IsNullOrEmpty(to)) to = "(unmapped)";
            if (string.IsNullOrEmpty(message)) message = "";

            var json = $"C:{reader.BytesConsumed,4} D:{reader.CurrentDepth} {reader.TokenType}".PadRight(25);
            Console.WriteLine($"{json}{from.PadRight(40)}=> {targetType.PadRight(20)} {to} {message}");
        }

        private string StackToPath(Stack<string> stack)
        {
            return string.Join(".", stack);
        }


        public record CurrentInstance
        {
            public object Collection { get; set; }
            public object Instance { get; set; }
        }

        public record MappingInfo
        {
            public string PropertyName { get; set; }
            public ScoredPropertyMapping<ModelNavigationNode> Mapping { get; set; }
        }
    }
}
