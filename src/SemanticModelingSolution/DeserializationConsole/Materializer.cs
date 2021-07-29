using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SemanticLibrary;

namespace DeserializationConsole
{
    public class Materializer
    {
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
                object instance;

                var path = temp.GetObjectMapPath();
                if(_objects.TryGetValue(path, out CurrentInstance cached))
                {
                    instance = cached.Instance;
                    if (isFirst)
                    {
                        return instance;
                    }
                    else
                    {
                        return result;
                    }
                }

                var isCollection = temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToDomain ||
                    temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToUnknown;

                if (isCollection)
                {
                    var property = temp.ModelPropertyNode.Property;
                    var collectionType = property.PropertyType;
                    instance = Activator.CreateInstance(collectionType);
                    var addMethod = collectionType.GetMethod("Add");
                    addMethod.Invoke(instance, new object[] { lastCreatedInstance });
                    _objects[path] = new CurrentInstance
                    {
                        Instance = instance,
                        IsCollection = true,
                        SourceCollectionElementPath = scoredPropertyMapping.Source.GetObjectMapPath(),
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
                            rootInstance = Activator.CreateInstance(rootType);
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
                    instance = Activator.CreateInstance(parentType);
                    _objects[path] = new CurrentInstance
                    {
                        Instance = instance,
                        IsCollection = false,
                        SourceCollectionElementPath = string.Empty,
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

        public void RemoveObjectsWithPath(string path)
        {
            var deleteKeys = _objects
                .Where(i => i.Value.SourceCollectionElementPath.StartsWith(path) && i.Key.Contains("$"))
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


        public record CurrentInstance
        {
            public string SourceCollectionElementPath { get; set; } = string.Empty;
            //public object Collection { get; set; }
            public bool IsCollection { get; set; }
            public object Instance { get; set; }
        }

    }
}
