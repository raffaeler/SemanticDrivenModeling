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

/*

Deserialization using partial reflection
The generated code for the conversion cannot be avoided because the "ref struct"
used by System.Text.Json can only be used typed or via Expression, but not via reflection

|                     Method |         Mean |      Error |     StdDev |
|--------------------------- |-------------:|-----------:|-----------:|
|                PlainOrders |     18.83 us |   0.376 us |   0.386 us |
|          PlainOnlineOrders |     11.42 us |   0.202 us |   0.189 us |
| SemanticOrderToOnlineOrder | 18,175.24 us | 235.091 us | 196.312 us |
| SemanticOnlineOrderToOrder | 21,196.31 us | 304.601 us | 284.924 us |
*/

namespace MaterializerLibrary
{
    public partial class SemanticConverter<T> : JsonConverter<T>
    {
        public T ReadReflection(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Instances = new();
            var isFinished = false;
            JsonPathStack jsonPathStack = new();

            //Debug.Assert(SurrogateType.GetFullName(typeToConvert) == _deserializeMap.Target.FullName);

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

                            //DebugOnlyLogState(reader.TokenType, reader.CurrentDepth, jsonPathStack.CurrentPath, null, "");
                        }
                        break;

                    case JsonTokenType.EndArray:
                        {
                            //DebugOnlyLogState(reader.TokenType, reader.CurrentDepth, jsonPathStack.CurrentPath, null, "");
                            jsonPathStack.Pop();
                        }
                        break;

                    case JsonTokenType.StartObject:
                        {
                            var found = jsonPathStack.TryPeek(out JsonSourcePath path);
                            if (!found)
                            {
                                path = jsonPathStack.Push(ExternalType.Name, false);
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

                            //DebugOnlyLogState(reader.TokenType, reader.CurrentDepth, jsonPathStack.CurrentPath, null, "");
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
                            //DebugOnlyLogState(reader.TokenType, reader.CurrentDepth, jsonPathStack.CurrentPath, null, "");
                        }
                        break;


                    case JsonTokenType.PropertyName:
                        {
                            var currentProperty = reader.GetString();
                            jsonPathStack.Push(currentProperty);
                            //DebugOnlyLogState(reader.TokenType, reader.CurrentDepth, jsonPathStack.CurrentPath, null, currentProperty);
                        }

                        break;

                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.Null:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        {
                            var mappings = GetMappingsFor(jsonPathStack.CurrentPath);
                            if (mappings.Count > 0)
                            {
                                var instances = MaterializeManyReflection(mappings);
                                var converter = _conversionGenerator.GetConverterMultiple(jsonPathStack.CurrentPath, mappings, false);
                                converter(ref reader, instances);
                            }
                            else
                            {
                                reader.Skip();
                            }

                            DebugOnlyLogState(reader.TokenType, reader.CurrentDepth, jsonPathStack.CurrentPath, mappings);
                            jsonPathStack.Pop();
                            break;
                        }

                    default:
                        {
                            reader.Skip();
                            DebugOnlyLogState(reader.TokenType, reader.CurrentDepth, jsonPathStack.CurrentPath, null, "");
                        }
                        break;
                }
            }
            while (!isFinished && reader.Read());

            var returnItem = ((IContainerDebug)Instances[typeof(T).Name]).ObjectItem;
            Instances = null;
            return (T)returnItem;
        }


        public IContainer[] MaterializeManyReflection(IReadOnlyCollection<NavigationPair> navigations)
        {
            var containers = new IContainer[navigations.Count];
            int i = 0;
            foreach (var navigationPair in navigations)
            {
                var targetNavigation = navigationPair.Target.GetLeaf();
                containers[i++] = GetOrCreateInstanceReflection(targetNavigation);
                //var instItem = ((IContainerDebug)inst).ObjectItem;
            }

            return containers;
        }

        private IContainer GetOrCreateInstanceReflection(NavigationSegment<Metadata> targetNavigation)
        {
            IContainer parentContainer = null;
            NavigationSegment<Metadata> tempTarget = targetNavigation;

            // step 1: walk down the path and find the first cached object
            while (tempTarget != null)
            {
                if (Instances.TryGetValue(tempTarget.PathAlt, out parentContainer)) break;
                tempTarget = tempTarget.Previous;
            }

            // if nothing is found, take the root
            // if a cached object is find, walk to the next (the first object to be created)
            tempTarget = (tempTarget == null) ? targetNavigation.GetRoot() : tempTarget.Next;

            // step 2: walk forward creating the new object and connecting it to the previous
            // the connection can be a 1-1 or adding an item to the list
            // We need to keep trace of the "collected" items because as the array ends
            // the entire sub-graph has to be removed from the cache
            while (tempTarget != null)
            {
                if (tempTarget.IsLeaf) break;

                var type = tempTarget.GetSegmentType();

                IContainer newContainer;
                if (tempTarget.Property != null)
                {
                    //// Equivalent code:
                    //// parentContainer.Item.property = newContainer.Item
                    //// Using reflection:
                    newContainer = new Container<object>(Activator.CreateInstance(type.GetOriginalType()));
                    var parentItem = ((Container<object>)parentContainer).Item;
                    var property = tempTarget.Property.GetOriginalPropertyInfo(_deserializationTypeSystem);
                    var newItem = ((Container<object>)newContainer).Item;
                    property.SetValue(parentItem, newItem);

                    //newContainer = CreateAndAssignProperty(parentContainer, tempTarget, type, AssignmentKind.SetOneToOne);
                }
                else if (tempTarget.IsCollectedItem)
                {
                    //// Equivalent code:
                    //// parentContainer.Item.Add(newContainer.Item)
                    //// Using reflection:
                    newContainer = new Container<object>(Activator.CreateInstance(type.GetOriginalType()));
                    var parentItem = ((Container<object>)parentContainer).Item; // collection
                    var newItem = ((Container<object>)newContainer).Item;

                    var addMethod = parentItem.GetType().GetMethod("Add");
                    addMethod.Invoke(parentItem, new object[] { newItem });

                    //newContainer = CreateAndAssignProperty(parentContainer, tempTarget, type, AssignmentKind.AddToCollection);
                }
                else
                {
                    //// Equivalent code:
                    //// new Container<Something>(new Something());
                    //// Using reflection:
                    newContainer = new Container<object>(Activator.CreateInstance(type.GetOriginalType()));

                    //newContainer = CreateOnly(parentContainer, tempTarget, type);
                }

                Instances[tempTarget.PathAlt] = newContainer;
                parentContainer = newContainer;

                tempTarget = tempTarget.Next;
            }

            return parentContainer;
        }

        //private void ConvertMultipleReflection(string sourcePath,
        //    IReadOnlyCollection<NavigationPair> nodeMappings, ref Utf8JsonReader reader, IContainer[] instances)
        //{
        //    var sourceTypeName = nodeMappings.First().Source.GetLeaf().Property.PropertyType.Name;

        //    if (!_conversionGenerator.ReaderGetValue.TryGetValue(sourceTypeName, out System.Reflection.MethodInfo readerMethod))
        //    {
        //        throw new ArgumentException($"The type {sourceTypeName} is not valid for reading data from a json source");
        //    }

        //    object val = readerMethod.Invoke(null, new object[] { reader });

        //    var i = 0;
        //    foreach (var nodeMapping in nodeMappings)
        //    {
        //        Type targetInstanceType = nodeMapping.Target.GetLeaf().Property.OwnerType.GetOriginalType();
        //        string targetPropertyTypeName = nodeMapping.Target.GetLeaf().Property.PropertyType.Name;
        //        string propertyName = nodeMapping.Target.GetLeaf().Property.Name;

        //        if (!_conversionGenerator.BasicTypes.TryGetValue(targetPropertyTypeName, out Type targetPropertyType))
        //        {
        //            throw new ArgumentException($"The type {targetPropertyTypeName} is not valid for converting data in a json source");
        //        }

        //        if (!_conversionGenerator.ConversionEngine.ConversionStrings.TryGetValue(targetPropertyTypeName, out ConversionLibrary.IConversion targetConverter))
        //        {
        //            throw new ArgumentException($"There is no converter instance for the target type {targetPropertyTypeName}");
        //        }

        //        if (!_conversionGenerator.ConverterTypes.TryGetValue(targetPropertyTypeName, out Type converterType))
        //        {
        //            throw new ArgumentException($"There is no converter type for the target type {targetPropertyTypeName}");
        //        }



        //        _conversionGenerator.ReaderGetValue[]
        //    }
        //}

    }
}
