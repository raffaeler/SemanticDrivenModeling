﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public static class Extensions
    {
        public static SurrogatePropertyInfo ToSurrogate(this PropertyInfo propertyInfo, UInt64 index,
            TypeSystem typeSystem, UInt64 ownerTypeIndex)
        {
            var propertyType = typeSystem.GetOrCreate(propertyInfo.PropertyType);
            var property = new SurrogatePropertyInfo(index, propertyInfo.Name, propertyType.Index, ownerTypeIndex);
            return property;
        }

        public static SurrogateType GetPropertyType(this SurrogatePropertyInfo surrogatePropertyInfo,
            TypeSystem typeSystem)
        {
            var surrogateType = typeSystem.Get(surrogatePropertyInfo.PropertyTypeIndex);
            return surrogateType;
        }

        public static SurrogateType GetOwnerType(this SurrogatePropertyInfo surrogatePropertyInfo,
            TypeSystem typeSystem)
        {
            var surrogateType = typeSystem.Get(surrogatePropertyInfo.OwnerTypeIndex);
            return surrogateType;
        }

        //public static SurrogatePropertyInfo GetSurrogateProperty(this SurrogateType ownerType,
        //    TypeSystem typeSystem, string propertyName)
        //{
        //    var property = ownerType.GetProperty(propertyName);
        //    var propertyType = typeSystem.GetOrCreate(property.PropertyType);
        //    return new SurrogatePropertyInfo(property.Name, propertyType.Index, ownerType.Index);
        //}

        public static PropertyInfo GetOriginalPropertyInfo(this SurrogatePropertyInfo surrogatePropertyInfo,
            TypeSystem typeSystem)
        {
            if (surrogatePropertyInfo._propertyInfo == null)
            {
                var surrogateOwnerType = GetOwnerType(surrogatePropertyInfo, typeSystem);
                var ownerType = surrogateOwnerType.GetOriginalType();
                surrogatePropertyInfo._propertyInfo = ownerType.GetProperty(surrogatePropertyInfo.Name);
                if (surrogatePropertyInfo._propertyInfo == null)
                {
                    throw new InvalidOperationException($"Can't find the property '{surrogatePropertyInfo.Name}' in the type '{ownerType.Name}'");
                }
            }

            return surrogatePropertyInfo._propertyInfo;
        }

        public static object GetValue(this SurrogatePropertyInfo surrogatePropertyInfo,
            TypeSystem typeSystem, object instance)
        {
            var propertyInfo = GetOriginalPropertyInfo(surrogatePropertyInfo, typeSystem);
            var value = propertyInfo.GetValue(instance);
            return value;
        }

        public static void SetValue(this SurrogatePropertyInfo surrogatePropertyInfo,
            TypeSystem typeSystem, object instance, object value)
        {
            var propertyInfo = GetOriginalPropertyInfo(surrogatePropertyInfo, typeSystem);
            propertyInfo.SetValue(instance, value);
        }

        public static string ToString(this SurrogatePropertyInfo surrogatePropertyInfo,
            TypeSystem typeSystem)
        {
            var parentType = GetOwnerType(surrogatePropertyInfo, typeSystem);
            return $"{parentType}.{surrogatePropertyInfo.Name}";
        }

    }
}
