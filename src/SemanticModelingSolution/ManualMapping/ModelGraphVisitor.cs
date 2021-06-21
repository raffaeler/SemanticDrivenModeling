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
        private HashSet<string> _visited = new();
        private Type[] _types;
        Action<string> _onType;
        Action<string, PropertyInfo, PropertyKind, Type> _onProperty;

        public ModelGraphVisitor(params Type[] domainTypes)
        {
            _types = domainTypes;
        }

        public virtual void Visit(Action<string> onType, Action<string, PropertyInfo, PropertyKind, Type> onProperty)
        {
            _onType = onType;
            _onProperty = onProperty;

            foreach (var type in _types)
            {
                VisitType(type);
            }
        }

        private void VisitType(Type type)
        {
            if (_visited.Contains(type.AssemblyQualifiedName)) return;

            _visited.Add(type.AssemblyQualifiedName);

            _onType(type.Name);

            var properties = type.GetProperties(_flags);
            List<Type> toVisit = new();
            foreach (var property in properties)
            {
                (PropertyKind classification, Type coreType) = Classify(property.PropertyType);
                OnVisitProperty(type, property, classification, coreType);

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
                        toVisit.Add(coreType == null ? property.PropertyType : coreType);
                        break;
                }
            }

            foreach (var visitType in toVisit)
            {
                VisitType(visitType);
            }
        }

        protected virtual void OnVisitProperty(Type type, PropertyInfo propertyInfo, PropertyKind classification, Type coreType)
        {
            //Console.WriteLine($"{type.Name} - {propertyInfo.Name} - {propertyInfo.PropertyType.Name} - {classification} - {coreType?.Name}");
            _onProperty(type.Name, propertyInfo, classification, coreType);
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

                if(innerClassification.typeKind == PropertyKind.OneToOneToDomain)
                    return (PropertyKind.OneToManyToDomain, innerClassification.inner);

                if (innerClassification.typeKind == PropertyKind.OneToOneToUnknown)
                    return (PropertyKind.OneToManyToUnknown, innerClassification.inner);

                if (TypeHelper.IsBasicType(underlying)) return (PropertyKind.OneToManyToDomain, underlying);
                return (PropertyKind.OneToManyToUnknown, underlying);
            }

            if (BelongToDomain(type)) return (PropertyKind.OneToOneToDomain, type);

            return (PropertyKind.OneToManyToUnknown, type);
        }


        public enum PropertyKind
        {
            BasicType,
            OneToManyBasicType,

            Enum,
            OneToManyEnum,

            OneToOneToDomain,
            OneToManyToDomain,

            OneToOneToUnknown,
            OneToManyToUnknown,
        }
    }
}
