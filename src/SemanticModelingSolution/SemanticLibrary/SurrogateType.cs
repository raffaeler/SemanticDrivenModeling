using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using SemanticLibrary.Helpers;

namespace SemanticLibrary
{
    public class SurrogateType
    {
        private Type _type;

        // default constructor is intended only for the serializer
        public SurrogateType()
        {
        }

        public SurrogateType(Type type)
        {
            AssemblyName = type.Assembly.GetName().Name;
            Name = type.Name;
            FullName = type.FullName;
            UniqueName = type.GetUniqueTypeName();
        }

        public string AssemblyName { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }

        /// <summary>
        /// This is used in dictionaries to ensure uniqueness
        /// It was "AssemblyQualifiedName" and replaced with "FullName" to avoid dependencies on the assembly name
        /// By using "FullName", if this is used to create a type, it will succeed only if the assembly is already in memory
        /// We now use the GetUniqueTypeName() extension method in order to rollback, if needed, to "AssemblyQualifiedName"
        /// </summary>
        public string UniqueName { get; set; }

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

        public MethodInfo GetMethod(string methodName)
        {
            var type = GetOriginalType();
            var method = type.GetMethod(methodName);
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
