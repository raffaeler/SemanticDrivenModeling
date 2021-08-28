using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SemanticLibrary.Helpers
{
    public static class TypeHelper
    {
        public static readonly Type[] BasicTypes = new Type[]
            {
                typeof(bool),
                typeof(string),
                typeof(byte), typeof(sbyte), typeof(char),
                typeof(Int16),typeof(UInt16),
                typeof(Int32),typeof(UInt32),
                typeof(Int64),typeof(UInt64),
                /*typeof(System.Half), */typeof(float),typeof(double), typeof(decimal),
                typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
                typeof(Guid),
            };

        /// <summary>
        /// This does not use "AssemblyQualifiedName" to avoid dependencies on the assembly name
        /// See ModelTypeNode class for its usage
        /// </summary>
        public static string GetUniqueTypeName(this Type type) => type.FullName;

        public static Type GetBasicType(string typeFullName)
        {
            if (typeFullName == null) throw new ArgumentNullException(nameof(typeFullName));
            if (!typeFullName.StartsWith("System"))
            {
                throw new ArgumentException("This method must only be used with basic types belonging to the 'System' namespace", nameof(typeFullName));
            }

            var type = Type.GetType(typeFullName);
            if (type == null) throw new ArgumentException($"The type {typeFullName} cannot be retrieved");
            return type;
        }

        public static Type GetEntityType(string typeFullName, string assemblyName = null)
        {
            if (typeFullName == null) throw new ArgumentNullException(nameof(typeFullName));

            var finalTypeName = assemblyName == null ? typeFullName : $"{typeFullName},{assemblyName}";
            var type = Type.GetType(finalTypeName);
            if (type == null) throw new ArgumentException($"The type {finalTypeName} cannot be retrieved");
            return type;
        }

        public static object GetDefaultForType(this Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;


        public static bool IsBasicType(Type type) => BasicTypes.Contains(type);
        public static Type GetUnderlyingNullable(Type type) => Nullable.GetUnderlyingType(type);
        public static Type GetUnderlyingArray(Type type) => type.HasElementType ? type.GetElementType() : null;
        public static Type GetUnderlyingCollection(Type type) => IsCollection(type) ? GetUnderlyingCollectionInternal(type) : null;
        public static bool IsEnumberable(Type type) => typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
        public static bool IsValueType(Type type) => type.IsValueType;
        public static bool IsEnum(Type type) => type.IsEnum;
        public static bool IsArray(Type type) => type.IsArray;
        public static bool IsGenericType(Type type) => type.IsGenericType;
        public static bool IsCollection(Type type) => !IsBasicType(type) && IsEnumberable(type);
        public static bool IsDictionary(Type type) => typeof(IDictionary<,>).IsAssignableFrom(type);
        public static bool IsGenericCollection(Type type) => IsGenericType(type) && IsEnumberable(type);
        public static Type[] GetGenericTypes(Type type) => IsGenericType(type) ? type.GetGenericArguments() : Array.Empty<Type>();

        private static Type GetUnderlyingCollectionInternal(Type type)
        {
            if (IsGenericCollection(type))
            {
                var genTypes = type.GetGenericArguments();
                if (genTypes.Length == 1) return genTypes.Single();
                if (IsDictionary(type)) return genTypes[1];

                // unsupported case
                Debug.WriteLine($"The type {type.FullName} is not supported");
                return null;
            }

            return typeof(object);
        }
    }
}
