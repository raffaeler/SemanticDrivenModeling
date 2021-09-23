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
        private string _fullName;
        internal List<SurrogatePropertyInfo> _properties;

        /// <summary>
        /// Only Basic Types
        /// </summary>
        internal SurrogateType(UInt64 index, Type type)
        {
            Index = index;
            AssemblyName = type.Assembly.GetName().Name;
            Namespace = type.Namespace;
            Name = type.Name;
            _properties = new List<SurrogatePropertyInfo>();
        }

        [System.Text.Json.Serialization.JsonConstructor]
        public SurrogateType(UInt64 index, string assemblyName, 
            string @namespace, string name, IReadOnlyList<SurrogatePropertyInfo> properties) =>
            (Index, AssemblyName, Namespace, Name, Properties) =
            (index, assemblyName, @namespace, name, properties);

        public UInt64 Index {  get; init; }
        public string AssemblyName { get; init; }
        public string Namespace { get; init; }
        public string Name { get; init; }
        public IReadOnlyList<SurrogatePropertyInfo> Properties
        {
            get => _properties;
            init => _properties = value == null ? null : new List<SurrogatePropertyInfo>(value);
        }

        public string FullName => _fullName ??= GetUniqueName(Namespace, Name);

        public static string GetUniqueName(string @namespace, string name) => $"{@namespace}.{name}";

        /// <summary>
        /// This is used in dictionaries to ensure uniqueness
        /// It was "AssemblyQualifiedName" and replaced with "FullName" to avoid dependencies on the assembly name
        /// By using "FullName", if this is used to create a type, it will succeed only if the assembly is already in memory
        /// We now use the GetUniqueTypeName() extension method in order to rollback, if needed, to "AssemblyQualifiedName"
        /// </summary>
        public string UniqueName => FullName;

        /// <summary>
        /// This only works when the Type is already loaded in memory
        /// otherwise an Exception will be thrown;
        /// </summary>
        public Type GetOriginalType() => _type ??= TypeHelper.GetEntityType(FullName, AssemblyName);

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
