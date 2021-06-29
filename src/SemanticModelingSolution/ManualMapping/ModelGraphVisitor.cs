using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary.Helpers;

namespace ManualMapping
{
    /// <summary>
    /// A class navigating a model composed by all the specified types
    /// </summary>
    public class ModelGraphVisitor
    {
        private BindingFlags _flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;
        //private HashSet<string> _visited = new();
        private Dictionary<string, ModelTypeNode> _visited = new();
        private Type[] _types;
        //Action<string> _onType;
        //Action<string, PropertyInfo, PropertyKind, Type> _onProperty;

        Action<ModelTypeNode> _onTypeNode;
        Action<ModelPropertyNode> _onPropertyNode;
        SemanticAnalysis _analysis;


        public ModelGraphVisitor(params Type[] domainTypes)
        {
            _types = domainTypes;
        }

        public virtual IList<ModelTypeNode> Visit(Action<ModelTypeNode> onTypeNode, Action<ModelPropertyNode> onPropertyNode, Type[] visitOnlyTypes = null)
        {
            _analysis = new SemanticAnalysis();
            _onTypeNode = onTypeNode;
            _onPropertyNode = onPropertyNode;

            var typesToVisit = visitOnlyTypes == null ? _types : visitOnlyTypes;

            List<ModelTypeNode> result = new();
            foreach (var type in typesToVisit)
            {
                result.Add(VisitType(type));
            }

            return result;
        }

        //public virtual void VisitOnly(Action<string> onType, Action<string, PropertyInfo, PropertyKind, Type> onProperty,
        //    params Type[] types)
        //{
        //    _analysis = new SemanticAnalysis();
        //    _onType = onType;
        //    _onProperty = onProperty;

        //    foreach (var type in types)
        //    {
        //        VisitType(type);
        //    }
        //}

        //public virtual void VisitAll(Action<string> onType, Action<string, PropertyInfo, PropertyKind, Type> onProperty)
        //{
        //    _analysis = new SemanticAnalysis();
        //    _onType = onType;
        //    _onProperty = onProperty;

        //    foreach (var type in _types)
        //    {
        //        VisitType(type);
        //    }
        //}

        private ModelTypeNode VisitType(Type type)
        {
            ModelTypeNode modelTypeNode;
            if (_visited.TryGetValue(type.AssemblyQualifiedName, out modelTypeNode)) return modelTypeNode;


            //_onType?.Invoke(type.Name);
            var classTermsToConcepts = _analysis.Analyze(type.Name);
            modelTypeNode = new ModelTypeNode()
            {
                TermsToConcepts = classTermsToConcepts,
                Type = type,
            };

            _visited[type.AssemblyQualifiedName] = modelTypeNode;
            _onTypeNode?.Invoke(modelTypeNode);

            var properties = type.GetProperties(_flags);
            List<(ModelPropertyNode, Type)> toVisit = new();
            foreach (var property in properties)
            {
                (PropertyKind classification, Type coreType) = Classify(property.PropertyType);
                var modelPropertyNode = OnVisitProperty(modelTypeNode, type, property, classification, coreType);
                modelTypeNode.PropertyNodes.Add(modelPropertyNode);

                switch (classification)
                {
                    case PropertyKind.BasicType:
                    case PropertyKind.OneToManyBasicType:
                    case PropertyKind.Enum:
                    case PropertyKind.OneToManyEnum:
                        break;

                    case PropertyKind.OneToOneToDomain:
                    case PropertyKind.OneToManyToDomain:
                    case PropertyKind.OneToOneToUnknown:
                    case PropertyKind.OneToManyToUnknown:
                        {
                            var t = coreType == null ? property.PropertyType : coreType;
                            toVisit.Add((modelPropertyNode, t));
                        }
                        break;
                }
            }

            foreach (var (modelPropertyNode, visitType) in toVisit)
            {
                modelPropertyNode.NavigationNode = VisitType(visitType);
            }

            return modelTypeNode;
        }

        protected virtual ModelPropertyNode OnVisitProperty(ModelTypeNode modelTypeNode, Type type, PropertyInfo propertyInfo, PropertyKind classification, Type coreType)
        {
            //Console.WriteLine($"{type.Name} - {propertyInfo.Name} - {propertyInfo.PropertyType.Name} - {classification} - {coreType?.Name}");
            //_onProperty?.Invoke(type.Name, propertyInfo, classification, coreType);
            var propertyTermsToConcepts = _analysis.Analyze(modelTypeNode.TermsToConcepts, propertyInfo.Name, coreType);
            var modelPropertyNode = new ModelPropertyNode()
            {
                Parent = modelTypeNode,
                Property = propertyInfo,
                PropertyKind = classification,
                CoreType = coreType,
                TermsToConcepts = propertyTermsToConcepts,
            };

            _onPropertyNode?.Invoke(modelPropertyNode);
            return modelPropertyNode;
        }

        protected virtual bool BelongToDomain(Type type) => _types.Contains(type);

        private (PropertyKind typeKind, Type inner) Classify(Type type)
        {
            if (TypeHelper.IsBasicType(type)) return (PropertyKind.BasicType, type);

            var innerNullable = TypeHelper.GetUnderlyingNullable(type);
            if (innerNullable != null) return Classify(innerNullable);

            var isEnum = TypeHelper.IsEnum(type);
            if (isEnum) return (PropertyKind.Enum, type);

            if (TypeHelper.IsCollection(type))
            {
                var underlying = TypeHelper.GetUnderlyingCollection(type);
                var innerClassification = Classify(underlying);

                if (innerClassification.typeKind == PropertyKind.BasicType)
                    return (PropertyKind.OneToManyBasicType, innerClassification.inner);

                if (innerClassification.typeKind == PropertyKind.Enum)
                    return (PropertyKind.OneToManyEnum, innerClassification.inner);

                if (innerClassification.typeKind == PropertyKind.OneToOneToDomain)
                    return (PropertyKind.OneToManyToDomain, innerClassification.inner);

                if (innerClassification.typeKind == PropertyKind.OneToOneToUnknown)
                    return (PropertyKind.OneToManyToUnknown, innerClassification.inner);

                if (TypeHelper.IsBasicType(underlying)) return (PropertyKind.OneToManyToDomain, underlying);
                return (PropertyKind.OneToManyToUnknown, underlying);
            }

            if (BelongToDomain(type)) return (PropertyKind.OneToOneToDomain, type);

            return (PropertyKind.OneToManyToUnknown, type);
        }
    }
}
