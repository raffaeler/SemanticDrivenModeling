﻿using System;
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


            foreach (var m in map.Mappings
                .Select(m => (mapping: m, source:m.Source.GetLeafPathAlt(), target: m.Target.GetLeafPath()))
                .OrderBy(m => m.source))
            {
                // retrieve all the "collected item" nodes from left to right
                var source = m.mapping.Source.GetRoot();
                List<NavigationSegment<Metadata>> sourceCollectedItems = new();
                source.OnAllAfter(item =>
                {
                    if (item.IsCollectedItem) sourceCollectedItems.Add(item);
                    return true;
                });

                // retrieve all the "collected item" nodes from left to right
                var target = m.mapping.Target.GetRoot();
                List<NavigationSegment<Metadata>> targetCollectedItems = new();
                target.OnAllAfter(item =>
                {
                    if (item.IsCollectedItem) targetCollectedItems.Add(item);
                    return true;
                });


                var j = 0;
                for (int i = 0; i < sourceCollectedItems.Count; i++)
                {
                    var sourceCollectedItem = sourceCollectedItems[i];
                    if (targetCollectedItems.Count <= j) break;

                    var key = sourceCollectedItem.PathAlt;
                    if (!_targetDeletablePaths.TryGetValue(key, out HashSet<string> deletables))
                    {
                        deletables = new HashSet<string>();
                        _targetDeletablePaths[key] = deletables;
                    }

                    var targetTemp = targetCollectedItems[j];
                    while (targetTemp != null && !targetTemp.IsLeaf)
                    {
                        deletables.Add(targetTemp.PathAlt);
                        targetTemp = targetTemp.Next;
                    }

                    j++;
                }


            }
        }

        protected string SourceTypeName => _map.Source.Name;
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
                    Console.Write(nodeMapping.Target.GetLeafPath());
                    Console.WriteLine();
                }
            }
            else
            {
                Console.Write(sourcePath.PadRight(50));
                Console.WriteLine();
            }
        }

        public K Materialize2<K>(NavigationPair navigationPair) where K : new()
        {
            var targetNavigation = navigationPair.Target.GetLeaf();
            var instance = GetOrCreateInstance(targetNavigation);

            return default(K);
        }

        public object Materialize(NavigationPair navigationPair)
        {
            //var sourcePath = scoredPropertyMapping.Source.GetObjectMapPath();
            //var targetPath = scoredPropertyMapping.Target.GetObjectMapPath();
            //Console.WriteLine();
            //Console.WriteLine($"Source: {sourcePath}");
            //Console.WriteLine($"Target: {targetPath}");

            var targetNavigation = navigationPair.Target.GetLeaf();
            var inst =  GetOrCreateInstance(targetNavigation);
            var instItem = ((Container<object>)inst).Item;
            return instItem;
        }

        public IContainer GetOrCreateInstance(NavigationSegment<Metadata> targetNavigation)
        {
            IContainer parentContainer = null;
            NavigationSegment<Metadata> tempTarget = targetNavigation;

            // step 1: walk down the path and find the first cached object
            while (tempTarget != null)
            {
                var targetPath = tempTarget.PathAlt;
                if (Instances.TryGetValue(targetPath, out parentContainer)) break;

                tempTarget = tempTarget.Previous;
            }

            // if nothing is found, take the root
            // if a cached object is find, walk to the next (the first object to be created)
            if (tempTarget == null)
                tempTarget = targetNavigation.GetRoot();
            else
                tempTarget = tempTarget.Next;

            // step 2: walk forward creating the new object and connecting it to the previous
            // the connection can be a 1-1 or adding an item to the list
            // We need to keep trace of the "collected" items because as the array ends
            // the entire sub-graph has to be removed from the cache
            while (tempTarget != null)
            {
                if (tempTarget.IsLeaf) break;

                var type = tempTarget.GetSegmentType();
                // create object
                // embed in container
                var newContainer = new Container<object>(Activator.CreateInstance(type.GetOriginalType()));
                if (tempTarget.Property != null)
                {
                    //parentContainer.Item.property = newContainer.Item
                    var parentItem = ((Container<object>)parentContainer).Item;
                    var property = tempTarget.Property.GetOriginalPropertyInfo(_targetTypeSystem);
                    var newItem = ((Container<object>)newContainer).Item;
                    property.SetValue(parentItem, newItem);
                }
                else if (tempTarget.IsCollectedItem)
                {
                    // add
                    var parentItem = ((Container<object>)parentContainer).Item; // collection
                    var newItem = ((Container<object>)newContainer).Item;

                    var addMethod = parentItem.GetType().GetMethod("Add");
                    addMethod.Invoke(parentItem, new object[] { newItem });
                }

                Instances[tempTarget.PathAlt] = newContainer;
                parentContainer = newContainer;

                tempTarget = tempTarget.Next;
            }

            return parentContainer;
        }

        private IContainer CreateAndSetProperty(IContainer parentObject,
            NavigationSegment<Metadata> segment, SurrogateType<Metadata> childType)
        {
            // TODO: check cache using segment.Path

            var childTypeToCreate = childType.GetOriginalType();
            var property = segment.Property.GetOriginalPropertyInfo(_targetTypeSystem);

            // var newChild = new Something();
            var newChildVar = Expression.Variable(childTypeToCreate, "newInstance");
            var newChildTypeObject = Expression.New(childTypeToCreate);
            var assignNewChildObject = Expression.Assign(newChildVar, newChildTypeObject);

            // var newContainer = new Container<Something>(newChild);
            var newContainerType = typeof(Container<>).MakeGenericType(childTypeToCreate);
            var newContainerCtor = newContainerType.GetConstructor(new Type[] { childTypeToCreate });
            var newContainerVar = Expression.Variable(newContainerType, "newContainer");
            var newContainerObject = Expression.New(newContainerCtor, newChildVar);
            var assignNewContainer = Expression.Assign(newContainerVar, newContainerObject);

            throw new NotImplementedException();

        }



    }
}
