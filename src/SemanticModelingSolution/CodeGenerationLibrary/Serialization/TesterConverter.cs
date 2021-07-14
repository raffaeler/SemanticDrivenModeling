using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SemanticLibrary;

namespace CodeGenerationLibrary.Serialization
{
    public class TesterConverter<T> : JsonConverter<T>
    {
        private Dictionary<string, CurrentInstance> _objects = new Dictionary<string, CurrentInstance>();
        private Stack<string> _stack = new Stack<string>();
        private readonly ScoredTypeMapping _map;
        private string _currentProperty;
        private ScoredPropertyMapping<ModelNavigationNode> _currentMapping;

        public TesterConverter(ScoredTypeMapping map)
        {
            _map = map;
            // root = _map.TargetModelTypeNode.TypeName
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            //Console.WriteLine($"TesterConverter.Read> ");
            //bool isNewArray = false;

            while (reader.Read())
            {
                //Log(ref reader);
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                        Console.WriteLine($"-- Start Array D:{reader.CurrentDepth} --");
                        //isNewArray = true;
                        break;
                    case JsonTokenType.EndArray:
                        Console.WriteLine($"-- End Array D:{reader.CurrentDepth} --");
                        break;

                    case JsonTokenType.StartObject:
                        Console.WriteLine($"---- Start Object D:{reader.CurrentDepth} ----");
                        _stack.Push(_currentProperty);

                        if (_currentMapping != null)
                        {
                            //_currentMapping.Target.
                        }
                        break;
                    case JsonTokenType.EndObject:
                        Console.WriteLine($"---- End Object D:{reader.CurrentDepth} ----");
                        var currentPathToType = _currentMapping.Target.GetMapPath(".", true);   // unique string to the type of this property
                        var deleteKeys = _objects.Keys.Where(k => k.StartsWith(currentPathToType)).ToArray();
                        foreach(var dk in deleteKeys)
                        {
                            if (dk == currentPathToType)
                                _objects[dk].Instance = null;
                            else
                                _objects.Remove(dk);
                        }

                        if (_stack.Count == 0) return default(T);
                        _stack.Pop();
                        break;


                    case JsonTokenType.PropertyName:
                        _currentProperty = reader.GetString();
                        _currentMapping = _map.PropertyMappings.FirstOrDefault(p => p.Source.Name == _currentProperty);
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
                                currentProperty.SetMethod.Invoke(currentInstance.Instance, new object[] { value });
                            Log(ref reader, value);
                            break;
                        }
                    case JsonTokenType.Number:
                        {
                            var value = reader.GetInt32();
                            Log(ref reader, value);
                            break;
                        }
                    case JsonTokenType.Null:
                        reader.Skip();
                        Log(ref reader, "null  ");
                        break;
                    case JsonTokenType.True:
                        reader.Skip();
                        Log(ref reader, "true  ");
                        break;
                    case JsonTokenType.False:
                        reader.Skip();
                        Log(ref reader, "false ");
                        break;

                    case JsonTokenType.None:
                        break;
                    case JsonTokenType.Comment:
                        break;
                }
            }


            //var r = reader;
            //reader.Skip();

            return default(T);
            //return (T)_currentInstance;
        }

        private Type GetObjectType()
        {
            if (_currentMapping == null)
            {
                return _map.TargetModelTypeNode.Type;
            }

            return _currentMapping.Target.ModelPropertyNode.Property.PropertyType;
        }

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
                    current.Collection = CreateInstance(ownerType.Type);
                    current.Instance = CreateInstance(modelNavigationNode.ModelPropertyNode.CoreType);
                    var addMethod = ownerType.Type.GetMethod("Add");
                    addMethod.Invoke(current, new object[] { current.Instance });
                }
                else
                {
                    current.Instance = CreateInstance(ownerType.Type);
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

            return current;
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

    }

    public record CurrentInstance
    {
        public object Collection { get; set; }
        public object Instance { get; set; }
    }
}
