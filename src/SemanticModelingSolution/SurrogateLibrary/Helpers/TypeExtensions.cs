using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary.Helpers
{
    public static class TypeExtensions
    {
        public static string ToStringEx(this Type type, bool withNamespace = false)
        {
            if (type == null) return null;

            StringBuilder sb = new StringBuilder();
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                sb.Append(GetFullName(underlyingType, withNamespace));
                sb.Append("?");
                return sb.ToString();
            }

            string originalName = GetFullName(type, withNamespace);

            if (type.IsGenericType)
            {
                int index = originalName.IndexOf("`");
                var name = index == -1
                    ? originalName
                    : originalName.Substring(0, index);

                var args = string.Join(", ",
                    type.GetGenericArguments()
                    .Select(t => t.ToStringEx(withNamespace)));
                sb.Append($"{name}<{args}>");
            }
            else if (type.IsGenericTypeDefinition)
            {
                sb.Append("<>");
            }
            else
            {
                sb.Append(originalName);
            }

            return sb.ToString();
        }

        public static string ToStringLoadable(this Type type)
        {
            if (type == null) return null;

            if (type.IsGenericTypeDefinition)
            {
                return GetShortAssemblyName(type.AssemblyQualifiedName);
            }

            StringBuilder sb = new StringBuilder();
            if (type.IsGenericType)
            {
                var args = string.Join("],[", type.GetGenericArguments().Select(t => t.ToStringLoadable()));
                sb.Append($"{type.Namespace}.{type.Name}[[{args}]]");

                var assemblyName = type.Assembly?.GetName();
                if (assemblyName != null)
                {
                    sb.Append($", {assemblyName.Name}");
                }
            }
            else if (type.IsGenericParameter)
            {
                sb.Append(type.Name);
            }
            else
            {
                sb.Append(GetShortAssemblyName(type.AssemblyQualifiedName));
            }

            return sb.ToString();
        }

        public static string ToStringEx(this MethodInfo methodInfo)
        {
            if (methodInfo == null) return null;

            StringBuilder sb = new StringBuilder();
            if (methodInfo.IsGenericMethod)
            {
                string name;
                int index = methodInfo.Name.IndexOf("`");

                name = index == -1
                    ? methodInfo.Name
                    : methodInfo.Name.Substring(0, index);

                var args = string.Join(", ",
                    methodInfo.GetGenericArguments()
                    .Select(t => t.ToStringEx(true)));
                sb.Append($"{name}<{args}>");
            }
            else if (methodInfo.IsGenericMethodDefinition)
            {
                sb.Append("<>");
            }
            else
            {
                sb.Append(methodInfo.Name);
            }

            return sb.ToString();
        }

        public static string GetFullName(this Type type, bool withNamespace)
        {
            //
            // warning: type.FullName in certain rare cases can be null
            // For example it is null for this type:
            // type.GetMethod("ConvertAll").ReturnType;
            //

            var name = type.IsNested ? type.Name.Replace('+', '.') : type.Name;

            if (withNamespace && !type.IsGenericParameter)
            {
                return $"{type.Namespace}.{name}";
            }

            return name;
        }

        private static string GetShortAssemblyName(string fullTypeName)
        {
            var parts = fullTypeName.Split(new[] { ", " },
                StringSplitOptions.RemoveEmptyEntries);

            var filtered = parts
                .Where(p =>
                    !p.StartsWith("Version=") &&
                    !p.StartsWith("Culture=") &&
                    !p.StartsWith("PublicKeyToken="));

            return string.Join(", ", filtered);
        }

    }
}
