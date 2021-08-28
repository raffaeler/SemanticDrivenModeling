using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SemanticLibrary;

namespace DeserializationConsole
{
    public class Materializer
    {
        private const string _arrayItemPlaceholder = "$";

        public Materializer()
        {
        }

        public object Materialize(ScoredTypeMapping map)
        {
            foreach (var mapping in map.PropertyMappings)
            {
                var obj = Materialize(mapping);
            }

            return null;
        }

        Dictionary<string, CurrentInstance> _objects = new();

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
                    instance = collectionType.CreateInstance();
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
                        var rootType = temp.ModelPropertyNode.Parent;
                        object rootInstance;
                        if (_objects.TryGetValue(rootType.Type.Name, out CurrentInstance cachedRoot))
                        {
                            rootInstance = cachedRoot.Instance;
                        }
                        else
                        {
                            rootInstance = rootType.Type.CreateInstance();
                            _objects[rootType.Type.Name] = new CurrentInstance
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
                    var parentType = temp.ModelPropertyNode.Parent;
                    instance = parentType.Type.CreateInstance();
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

        private object CreateInstance(Type type) => Activator.CreateInstance(type);


        private record CurrentInstance
        {
            public string SourceCollectionElementPath { get; set; } = string.Empty;
            public bool IsCollection { get; set; }
            public object Instance { get; set; }
            public System.Reflection.MethodInfo AddCollectionMethod { get; set; }
        }

    }
}
