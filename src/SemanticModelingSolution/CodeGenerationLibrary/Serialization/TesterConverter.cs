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
        private Stack<string> _stack = new Stack<string>();
        private readonly ScoredTypeMapping _map;
        private string _currentProperty;
        private ScoredPropertyMapping<ModelNavigationNode> _currentMapping;

        public TesterConverter(ScoredTypeMapping map)
        {
            _map = map;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            //Console.WriteLine($"TesterConverter.Read> ");

            while (reader.Read())
            {
                //Log(ref reader);
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                        break;
                    case JsonTokenType.EndArray:
                        break;

                    case JsonTokenType.StartObject:
                        _stack.Push(_currentProperty);
                        break;
                    case JsonTokenType.EndObject:
                        if (_stack.Count == 0) return default(T);
                        _stack.Pop();
                        break;


                    case JsonTokenType.PropertyName:
                        _currentProperty = reader.GetString();
                        _currentMapping = _map.PropertyMappings.FirstOrDefault(p => p.Source.Name == _currentProperty);
                        break;

                    case JsonTokenType.String:
                        {
                            var value = reader.GetString();
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
}
