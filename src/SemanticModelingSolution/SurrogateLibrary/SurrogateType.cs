using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using SurrogateLibrary.Helpers;

namespace SurrogateLibrary
{
    public record SurrogateType
    {
        private Type _type;
        private ListExShallow<SurrogateProperty> _properties = new();

        internal ListEx<UInt64> _propertyIndexes;

        /// <summary>
        /// Only Basic Types
        /// </summary>
        internal SurrogateType(UInt64 index, Type type)
        {
            var (flags, _, _) = type.Classify();
            Index = index;
            AssemblyName = TypeSystem.PlaceholderForSystemAssemblyName; //type.Assembly.GetName().Name;
            Namespace = type.Namespace;
            Name = type.Name;
            FullName = GetFullName(type);
            TypeFields1 = flags;
            _propertyIndexes = new ListEx<UInt64>();
        }

        [System.Text.Json.Serialization.JsonConstructor]
        public SurrogateType(UInt64 index, string assemblyName,
            string @namespace, string name, string fullName, TypeFields typeFields1, UInt64 innerTypeIndex1, UInt64 innerTypeIndex2,
            IReadOnlyList<UInt64> propertyIndexes) =>
            (Index, AssemblyName, Namespace, Name, FullName, TypeFields1, InnerTypeIndex1, InnerTypeIndex2, PropertyIndexes) =
            (index, assemblyName, @namespace, name, fullName, typeFields1, innerTypeIndex1, innerTypeIndex2, propertyIndexes);

        public UInt64 Index { get; init; }
        public string AssemblyName { get; init; }
        public string Namespace { get; init; }
        public string Name { get; init; }

        /// <summary>
        /// In this type system, FullName must be unique as it used in dictionaries
        /// </summary>
        public string FullName { get; init; }
        public TypeFields TypeFields1 { get; init; }
        public UInt64 InnerTypeIndex1 { get; init; }
        public UInt64 InnerTypeIndex2 { get; init; }

        public IReadOnlyList<UInt64> PropertyIndexes
        {
            get => _propertyIndexes;
            init => _propertyIndexes = value == null ? null : new ListEx<UInt64>(value);
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public IReadOnlyList<SurrogateProperty> Properties
        {
            get => _properties;
            private set => _properties = value == null ? null : new ListExShallow<SurrogateProperty>(value);
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public SurrogateType InnerType1 { get; private set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public SurrogateType InnerType2 { get; private set; }

        public bool IsBasicType => this.Index < KnownTypes.MaxIndexForBasicTypes;

        public static string GetFullName(Type type) => type.ToStringEx(true);

        internal void UpdateCache(TypeSystem typeSystem)
        {
            InnerType1 = typeSystem.GetSurrogateType(InnerTypeIndex1);
            InnerType2 = typeSystem.GetSurrogateType(InnerTypeIndex2);

            _properties.Clear();
            foreach (var property in PropertyIndexes)
            {
                _properties.Add(typeSystem.GetSurrogatePropertyInfo(property));
            }
        }

        /// <summary>
        /// This only works when the Type is already loaded in memory
        /// otherwise an Exception will be thrown;
        /// </summary>
        public Type GetOriginalType()
        {
            var assemblyName = AssemblyName == TypeSystem.PlaceholderForSystemAssemblyName
                ? null : AssemblyName;
            return _type ??= TypeHelper.GetEntityType(FullName, assemblyName);
        }

        public object CreateInstance() => Activator.CreateInstance(GetOriginalType());

        public object GetDefaultForType()
        {
            var type = GetOriginalType();
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public PropertyInfo GetProperty(string propertyName)
        {
            var type = GetOriginalType();
            var property = type.GetProperty(propertyName);
            if (property == null) throw new InvalidOperationException($"Cannot find the method '{propertyName}' in '{FullName}'");
            return property;
        }

        public MethodInfo GetMethod(string methodName)
        {
            var type = GetOriginalType();
            var method = type.GetMethod(methodName);
            if (method == null) throw new InvalidOperationException($"Cannot find the method '{methodName}' in '{FullName}'");
            return method;
        }

        public MethodInfo GetMethod(string methodName, Type[] parameterTypes)
        {
            var type = GetOriginalType();
            var method = type.GetMethod(methodName, parameterTypes);
            if (method == null) throw new InvalidOperationException($"Cannot find the method '{methodName}' in '{FullName}'");
            return method;
        }

        public bool Is(Type type)
        {
            if (FullName == type.FullName) return true;
            return false;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
