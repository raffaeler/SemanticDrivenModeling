using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public static class Extensions
    {
        //public static SurrogateProperty ToSurrogate(this PropertyInfo propertyInfo, UInt64 index,
        //    TypeSystem typeSystem, UInt64 ownerTypeIndex)
        //{
        //    var propertyType = typeSystem.GetOrCreate(propertyInfo.PropertyType);
        //    var property = new SurrogateProperty(index, propertyInfo.Name, propertyType.Index, ownerTypeIndex);
        //    return property;
        //}

        //public static SurrogateProperty<T> ToSurrogate<T>(this PropertyInfo propertyInfo, UInt64 index,
        //    TypeSystem typeSystem, UInt64 ownerTypeIndex, T info)
        //{
        //    var propertyType = typeSystem.GetOrCreate(propertyInfo.PropertyType);
        //    var property = new SurrogateProperty<T>(index, propertyInfo.Name, propertyType.Index, ownerTypeIndex, info);
        //    return property;
        //}

        //public static SurrogateType GetPropertyType(this SurrogateProperty surrogatePropertyInfo,
        //    TypeSystem typeSystem)
        //{
        //    var surrogateType = typeSystem.GetSurrogateType(surrogatePropertyInfo.PropertyTypeIndex);
        //    return surrogateType;
        //}

        //public static SurrogateType<T> GetPropertyType<T>(this SurrogateProperty surrogatePropertyInfo,
        //    TypeSystem typeSystem)
        //{
        //    var surrogateType = typeSystem.GetSurrogateType<T>(surrogatePropertyInfo.PropertyTypeIndex);
        //    return surrogateType;
        //}

        public static SurrogateType GetOwnerType(this SurrogateProperty surrogatePropertyInfo,
            ITypeSystem typeSystem)
        {
            var surrogateOwnerType = typeSystem.GetSurrogateType(surrogatePropertyInfo.OwnerTypeIndex);
            return surrogateOwnerType;
        }

        //public static SurrogateType<T> GetOwnerType<T>(this SurrogateProperty surrogatePropertyInfo,
        //    TypeSystem typeSystem)
        //{
        //    var surrogateType = typeSystem.GetSurrogateType<T>(surrogatePropertyInfo.OwnerTypeIndex);
        //    return surrogateType;
        //}

        //public static SurrogatePropertyInfo GetSurrogateProperty(this SurrogateType ownerType,
        //    TypeSystem typeSystem, string propertyName)
        //{
        //    var property = ownerType.GetProperty(propertyName);
        //    var propertyType = typeSystem.GetOrCreate(property.PropertyType);
        //    return new SurrogatePropertyInfo(property.Name, propertyType.Index, ownerType.Index);
        //}

        public static PropertyInfo GetOriginalPropertyInfo(this SurrogateProperty surrogatePropertyInfo,
            ITypeSystem typeSystem)
        {
            if (surrogatePropertyInfo._propertyInfo == null)
            {
                var surrogateOwnerType = typeSystem.GetSurrogateType(surrogatePropertyInfo.OwnerTypeIndex);
                //var surrogateOwnerType = GetOwnerType(surrogatePropertyInfo, typeSystem);

                var ownerType = surrogateOwnerType.GetOriginalType();
                surrogatePropertyInfo._propertyInfo = ownerType.GetProperty(surrogatePropertyInfo.Name);
                if (surrogatePropertyInfo._propertyInfo == null)
                {
                    throw new InvalidOperationException($"Can't find the property '{surrogatePropertyInfo.Name}' in the type '{ownerType.Name}'");
                }
            }

            return surrogatePropertyInfo._propertyInfo;
        }

        public static object GetValue(this SurrogateProperty surrogatePropertyInfo,
            ITypeSystem typeSystem, object instance)
        {
            var propertyInfo = GetOriginalPropertyInfo(surrogatePropertyInfo, typeSystem);
            var value = propertyInfo.GetValue(instance);
            return value;
        }

        public static void SetValue(this SurrogateProperty surrogatePropertyInfo,
            ITypeSystem typeSystem, object instance, object value)
        {
            var propertyInfo = GetOriginalPropertyInfo(surrogatePropertyInfo, typeSystem);
            propertyInfo.SetValue(instance, value);
        }

        public static string ToString(this SurrogateProperty surrogatePropertyInfo,
            ITypeSystem typeSystem)
        {
            var surrogateOwnerType = typeSystem.GetSurrogateType(surrogatePropertyInfo.OwnerTypeIndex);
            return $"{surrogateOwnerType}.{surrogatePropertyInfo.Name}";
        }

        public static void SetInfo<T>(this SurrogateType<T> surrogateType, T info)
        {
            surrogateType.Info = info;
        }

        public static void SetInfo<T>(this SurrogateProperty<T> surrogateProperty, T info)
        {
            surrogateProperty.Info = info;
        }

    }
}
