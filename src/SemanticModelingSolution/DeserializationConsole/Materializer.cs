using System;
using System.Collections.Generic;
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

        public object Materialize(ScoredPropertyMapping<ModelNavigationNode> scoredPropertyMapping)
        {
            var sourcePath = scoredPropertyMapping.Source.GetObjectMapPath();
            var targetPath = scoredPropertyMapping.Target.GetObjectMapPath();
            Console.WriteLine();
            Console.WriteLine($"Source: {sourcePath}");
            Console.WriteLine($"Target: {targetPath}");

            Dictionary<string, CurrentInstance> objects = new();

            var temp = scoredPropertyMapping.Target;
            object result = null;
            object lastCreatedInstance = null;
            while (temp != null)
            {
                var path = temp.GetObjectMapPath();

                var parentType = temp.ModelPropertyNode.Parent.Type;
                var property = temp.ModelPropertyNode.Property;
                var isCollection = temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToDomain ||
                    temp.ModelPropertyNode.PropertyKind == PropertyKind.OneToManyToUnknown;

                var instance = Activator.CreateInstance(parentType);
                if (result != null)
                {
                    if (isCollection)
                    {
                        var collectionType = property.PropertyType;
                        var collectionInstance = Activator.CreateInstance(collectionType);
                        property.SetValue(instance, collectionInstance);
                        var addMethod = collectionType.GetMethod("Add");
                        addMethod.Invoke(collectionInstance, new object[] { lastCreatedInstance });
                        objects[path] = new CurrentInstance
                        {
                            Instance = collectionInstance,
                            IsCollection = true,
                            SourceCollectionElementPath = sourcePath,
                        };

                        if(temp.Previous == null)
                        {
                            objects[parentType.Name] = new CurrentInstance
                            {
                                Instance = instance,
                                IsCollection = false,
                                SourceCollectionElementPath = string.Empty,
                            };
                        }
                    }
                    else
                    {
                        property.SetValue(instance, result);
                        objects[path] = new CurrentInstance
                        {
                            Instance = instance,
                            IsCollection = false,
                            SourceCollectionElementPath = string.Empty,
                        };
                    }
                }
                else
                {
                    result = instance;
                    objects[path] = new CurrentInstance
                    {
                        Instance = instance,
                        IsCollection = false,
                        SourceCollectionElementPath = string.Empty,
                    };
                }

                Console.WriteLine($"{parentType.Name}.{property.Name} (IsCollection: {isCollection})");
                temp = temp.Previous;
                lastCreatedInstance = instance;
            }



            return result;
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
