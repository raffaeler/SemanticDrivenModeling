using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SurrogateLibrary
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

        public static object GetDefaultValue(this Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;



        public static bool IsBasicType(Type type) => BasicTypes.Contains(type);
        public static Type GetUnderlyingNullable(Type type) => Nullable.GetUnderlyingType(type);
        public static Type GetUnderlyingArray(Type type) => type.HasElementType ? type.GetElementType() : null;
        public static Type GetUnderlyingCollection(Type type) => IsCollection(type) ? GetUnderlyingCollectionInternal(type) : null;
        public static bool IsEnumerable(Type type) => typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
        public static bool IsValueType(Type type) => type.IsValueType;
        public static bool IsEnum(Type type) => type.IsEnum;
        public static bool IsArray(Type type) => type.IsArray;
        public static bool IsGenericType(Type type) => type.IsGenericType;
        public static bool IsCollection(Type type) => !IsBasicType(type) && IsEnumerable(type);
        public static bool IsDictionary(Type type) => typeof(IDictionary<,>).IsAssignableFrom(type);
        public static bool IsGenericCollection(Type type) => IsGenericType(type) && IsEnumerable(type);
        public static Type[] GetGenericTypes(Type type) => IsGenericType(type) ? type.GetGenericArguments() : Array.Empty<Type>();

        public static (bool isCollection, bool isDictionary, Type type1, Type type2) GetCollectionOrDictionaryInfo(Type type)
        {
            if (type == typeof(string)) return (false, false, null, null);
            if (!IsEnumerable(type)) return (false, false, null, null);
            if (IsGenericType(type))
            {
                var args = type.GetGenericArguments();
                if (args.Length == 1) return (true, false, args[0], null);
                if (args.Length == 2) return (false, true, args[0], args[1]);
                throw new Exception("Unsupported type with more than two generic parameters");
            }

            if (type.IsArray)
            {
                var inner = type.GetElementType();
                if (type.GetArrayRank() == 1) return (true, false, inner, null);
                if (type.GetArrayRank() == 2) return (false, true, inner, inner);
                throw new Exception("Unsupported array type with rank greater than 2");
            }

            var o = typeof(object);
            if (type == typeof(System.Collections.ArrayList)) return (true, false, o, null);
            if (type == typeof(System.Collections.BitArray)) return (true, false, o, null);
            if (type == typeof(System.Collections.DictionaryEntry)) return (false, true, o, o);
            if (type == typeof(System.Collections.Hashtable)) return (false, true, o, o);
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(type)) return (false, true, o, o);
            if (typeof(System.Collections.IList).IsAssignableFrom(type)) return (true, false, o, null);
            if (typeof(System.Collections.ICollection).IsAssignableFrom(type)) return (true, false, o, null);
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type)) return (true, false, o, null);
            if (type == typeof(System.Collections.Queue)) return (true, false, o, null);
            if (type == typeof(System.Collections.SortedList)) return (true, false, o, null);
            if (type == typeof(System.Collections.Stack)) return (true, false, o, null);

            throw new Exception("Unsupported enumerable type");
        }

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

        public static (TypeFields fields, Type inner1, Type inner2) Classify(this Type type)
        {
            TypeFields f = TypeFields.None;
            f |= (type.IsValueType) ? TypeFields.ValueType : TypeFields.ReferenceType;  // overhead
            if (IsGenericType(type)) f |= TypeFields.GenericType;
            if (type.IsInterface) f |= TypeFields.Interface;
            if (type.IsAbstract) f |= TypeFields.Abstract;

            var innerNullable = TypeHelper.GetUnderlyingNullable(type);
            if (innerNullable != null)
            {
                f |= TypeFields.Nullable;
                return (f, innerNullable, null);
            }

            var isEnum = TypeHelper.IsEnum(type);
            if (isEnum)
            {
                f |= TypeFields.Enum;
                return (f, null, null);
            }

            var (isCollection, isDictionary, inner1, inner2) = GetCollectionOrDictionaryInfo(type);
            if (isCollection)
            {
                f |= TypeFields.Collection;
                return (f, inner1, inner2);
            }

            if (isDictionary)
            {
                f |= TypeFields.Dictionary;
                return (f, inner1, inner2);
            }

            return (f, null, null);
        }


        //public static (PropertyKind typeKind, Type inner) Classify(this Type type)
        //{
        //    ?if (TypeHelper.IsBasicType(type)) return (PropertyKind.BasicType, type);

        //    var innerNullable = TypeHelper.GetUnderlyingNullable(type);
        //    if (innerNullable != null) return Classify(innerNullable);

        //    var isEnum = TypeHelper.IsEnum(type);
        //    if (isEnum) return (PropertyKind.Enum, type);

        //    if (TypeHelper.IsCollection(type))
        //    {
        //        var underlying = TypeHelper.GetUnderlyingCollection(type);
        //        var innerClassification = Classify(underlying);

        //        if (innerClassification.typeKind == PropertyKind.BasicType)
        //            return (PropertyKind.OneToManyBasicType, innerClassification.inner);

        //        if (innerClassification.typeKind == PropertyKind.Enum)
        //            return (PropertyKind.OneToManyEnum, innerClassification.inner);

        //        if (innerClassification.typeKind == PropertyKind.OneToOneToDomain)
        //            return (PropertyKind.OneToManyToDomain, innerClassification.inner);

        //        if (innerClassification.typeKind == PropertyKind.OneToOneToUnknown)
        //            return (PropertyKind.OneToManyToUnknown, innerClassification.inner);

        //        if (TypeHelper.IsBasicType(underlying)) return (PropertyKind.OneToManyToDomain, underlying);
        //        return (PropertyKind.OneToManyToUnknown, underlying);
        //    }

        //    if (BelongToDomain(type)) return (PropertyKind.OneToOneToDomain, type);

        //    return (PropertyKind.OneToManyToUnknown, type);
        //}

    }
}
