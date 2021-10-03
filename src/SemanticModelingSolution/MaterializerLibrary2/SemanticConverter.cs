using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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

        //private Stack<string> _collectionElementStack;
        //private Stack<JsonSegment> _sourcePath;
        //private Dictionary<string, CurrentInstance> _objects;

        private ConversionGenerator _conversionGenerator;
        protected TypeSystem<Metadata> _sourceTypeSystem;
        protected TypeSystem<Metadata> _targetTypeSystem;
        protected Mapping _map;

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
            _targetDeletablePaths = _map.CreateDeletableLookup();

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

        protected bool LogObjectArrayEnabled { get; set; } = true;

        /*
        protected virtual void InitializeForEachObject()
        {
            //_collectionElementStack = new();
            //_sourcePath = new();
            //_objects = new();
        }
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


    }
}
