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
        private enum AssignmentKind
        {
            SetOneToOne,
            AddToCollection,
        }

        private readonly IReadOnlyCollection<NavigationPair> _emptyMappings = Array.Empty<NavigationPair>();

        /// <summary>
        /// The lookup dictionary used in deserialization
        /// </summary>
        protected IDictionary<string, List<NavigationPair>> _sourceLookup;

        /// <summary>
        /// key is the source path
        /// values are the deletable objects
        /// </summary>
        protected IDictionary<string, HashSet<string>> _targetDeletablePaths;

        /// <summary>
        /// The cached object used in deserialization
        /// </summary>
        private Dictionary<string, IContainer> Instances;

        protected string SourceTypeName => _map.Source.Name;

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Instances = new();
            //var utilities = new DeserializeUtilities<T>(_conversionGenerator, _map);
            //var exp = utilities.CreateExpression();
            //var del = exp.Compile();
            //var instance = del(ref reader, typeToConvert, options);

            //return instance;

            var isFinished = false;
            JsonPathStack jsonPathStack = new();

            //Debug.Assert(SurrogateType.GetFullName(typeToConvert) == _map.Target.FullName);

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
                                path = jsonPathStack.Push(SourceTypeName, false);
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
                                var instances = mappings.Select(m => Materialize(m)).ToArray();
                                var converter = _conversionGenerator.GetConverterMultiple(jsonPathStack.CurrentPath, mappings);
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

            var returnItem = ((Container<T>)Instances[typeof(T).Name]).Item;
            Instances = null;
            return returnItem;
        }

        private IReadOnlyCollection<NavigationPair> GetMappingsFor(string sourcePath)
        {
            if (!_sourceLookup.TryGetValue(sourcePath, out var mappings))
            {
                return _emptyMappings;
            }

            return mappings;
        }

        private void RemoveInstance(string sourcePath)
        {
            if (!_targetDeletablePaths.TryGetValue(sourcePath, out var deletables)) return;

            foreach (var item in deletables)
            {
                Instances.Remove(item);
            }
        }

        public IContainer Materialize(NavigationPair navigationPair)
        {
            var targetNavigation = navigationPair.Target.GetLeaf();
            var inst = GetOrCreateInstance(targetNavigation);
            //var instItem = ((IContainerDebug)inst).ObjectItem;//((Container<object>)inst).Item;
            return inst;
        }

        private IContainer GetOrCreateInstance(NavigationSegment<Metadata> targetNavigation)
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
                IContainer newContainer;
                if (tempTarget.Property != null)
                {
                    //// Equivalent code:
                    //// parentContainer.Item.property = newContainer.Item
                    //// Using reflection:
                    //newContainer = new Container<object>(Activator.CreateInstance(type.GetOriginalType()));
                    //var parentItem = ((Container<object>)parentContainer).Item;
                    //var property = tempTarget.Property.GetOriginalPropertyInfo(_targetTypeSystem);
                    //var newItem = ((Container<object>)newContainer).Item;
                    //property.SetValue(parentItem, newItem);

                    newContainer = CreateAndAssignProperty(parentContainer, tempTarget, type, AssignmentKind.SetOneToOne);
                }
                else if (tempTarget.IsCollectedItem)
                {
                    //// Equivalent code:
                    //// parentContainer.Item.Add(newContainer.Item)
                    //// Using reflection:
                    //newContainer = new Container<object>(Activator.CreateInstance(type.GetOriginalType()));
                    //var parentItem = ((Container<object>)parentContainer).Item; // collection
                    //var newItem = ((Container<object>)newContainer).Item;

                    //var addMethod = parentItem.GetType().GetMethod("Add");
                    //addMethod.Invoke(parentItem, new object[] { newItem });

                    newContainer = CreateAndAssignProperty(parentContainer, tempTarget, type, AssignmentKind.AddToCollection);
                }
                else
                {
                    //// Equivalent code:
                    //// new Container<Something>(new Something());
                    //// Using reflection:
                    //newContainer = new Container<object>(Activator.CreateInstance(type.GetOriginalType()));

                    newContainer = CreateOnly(parentContainer, tempTarget, type);
                }

                Instances[tempTarget.PathAlt] = newContainer;
                parentContainer = newContainer;

                tempTarget = tempTarget.Next;
            }

            return parentContainer;
        }

        private Dictionary<string, Func<IContainer, IContainer>> _CreateAndAssignPropertyCache = new();
        private IContainer CreateAndAssignProperty(IContainer parentContainer,
            NavigationSegment<Metadata> segment, SurrogateType<Metadata> childType, AssignmentKind assignmentKind)
        {
            // TODO: check cache using segment.Path
            if (_CreateAndAssignPropertyCache.TryGetValue(segment.Path, out var func))
            {
                return func(parentContainer);
            }

            var childTypeToCreate = childType.GetOriginalType();
            var property = segment.Property?.GetOriginalPropertyInfo(_targetTypeSystem);

            // input variables to the lambda
            var parentContainerParameter = Expression.Parameter(typeof(IContainer), "parentContainer");

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

            // var parentObject = ((Container<Something>)parentContainer).Item
            var parentObjectType = parentContainer.Type;// segment.GetSegmentType().GetOriginalType();//property.DeclaringType; //parentContainer.Type;
            var parentObjectVar = Expression.Variable(parentObjectType, "parentObject");
            var parentContainerType = typeof(Container<>).MakeGenericType(parentObjectType);
            var itemProperty = parentContainerType.GetProperty("Item");
            var parentTypedContainer = Expression.Convert(parentContainerParameter, parentContainerType);
            var assignParentObject = Expression.Assign(parentObjectVar,
                Expression.MakeMemberAccess(parentTypedContainer, itemProperty));

            Expression action = Expression.Empty();
            if (assignmentKind == AssignmentKind.SetOneToOne)
            {
                // parentObject.SomeProperty = newChild;
                action = Expression.Assign(
                    Expression.MakeMemberAccess(parentObjectVar, property),
                    newChildVar);
            }
            else if (assignmentKind == AssignmentKind.AddToCollection)
            {
                var addMethod = parentObjectType.GetMethod("Add");
                action = Expression.Call(parentObjectVar, addMethod, newChildVar);
            }
            else
            {
                // CreateOnly should never call this method
                throw new NotSupportedException();
            }

            var lambda = Expression.Lambda<Func<IContainer, IContainer>>(
                Expression.Block(
                    new ParameterExpression[] { newChildVar, newContainerVar, parentObjectVar },
                    assignNewChildObject,
                    assignNewContainer,
                    assignParentObject,
                    action,
                    newContainerVar
                    ),
                parentContainerParameter);

            var del = lambda.Compile();
            _CreateAndAssignPropertyCache[segment.Path] = del;
            return del(parentContainer);
        }

        private Dictionary<string, Func<IContainer, IContainer>> _createOnlyCache = new();
        private IContainer CreateOnly(IContainer parentContainer, NavigationSegment<Metadata> segment, SurrogateType<Metadata> childType)
        {
            if (_createOnlyCache.TryGetValue(segment.Path, out var func))
            {
                return func(parentContainer);
            }

            var childTypeToCreate = childType.GetOriginalType();

            // input variables to the lambda
            var parentContainerParameter = Expression.Parameter(typeof(IContainer), "parentContainer");

            // new Container<Something>(new Something());
            var newContainerType = typeof(Container<>).MakeGenericType(childTypeToCreate);
            var newContainerCtor = newContainerType.GetConstructor(new Type[] { childTypeToCreate });
            var newContainerObject = Expression.New(newContainerCtor, Expression.New(childTypeToCreate));

            var lambda = Expression.Lambda<Func<IContainer, IContainer>>(
                newContainerObject,
                parentContainerParameter);

            var del = lambda.Compile();
            _createOnlyCache[segment.Path] = del;
            return del(parentContainer);
        }

        [Conditional("DEBUG")]
        private void DebugOnlyLogState(JsonTokenType jsonTokenType, int depth, string sourcePath,
            IReadOnlyCollection<NavigationPair> mappings, string message = null)
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
            if (mappings != null && mappings.Count > 0)
            {
                foreach (var mapping in mappings)
                {
                    if (!isFirst) Console.Write("".PadRight(25));
                    isFirst = false;

                    var sourceType = mapping.Source.GetLeaf().Property.PropertyType.GetOriginalType();
                    var targetType = mapping.Target.GetLeaf().Property.PropertyType.GetOriginalType();
                    Console.Write(sourcePath.PadRight(50));
                    Console.Write($"{sourceType.Name} -> {targetType.Name}".PadRight(30));
                    Console.Write(mapping.Target.GetLeafPath());
                    Console.WriteLine();
                }
            }
            else
            {
                Console.Write(sourcePath.PadRight(50));
                Console.WriteLine();
            }
        }


    }
}
