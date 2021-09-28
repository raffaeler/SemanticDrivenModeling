using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public static class SurrogateTypeExtensions
    {
        public static SurrogateType GetInnerType1(this SurrogateType surrogateType, ITypeSystem typeSystem)
        {
            if (surrogateType.InnerTypeIndex1 == 0) return null;
            var result = typeSystem.GetSurrogateType(surrogateType.InnerTypeIndex1);
            return result;
        }

        public static bool IsValueType(this SurrogateType surrogateType)
            => surrogateType.TypeFields1.HasFlag(TypeFields.ValueType);
        public static bool IsReferenceType(this SurrogateType surrogateType)
            => surrogateType.TypeFields1.HasFlag(TypeFields.ReferenceType);
        public static bool IsInterface(this SurrogateType surrogateType)
            => surrogateType.TypeFields1.HasFlag(TypeFields.Interface);
        public static bool IsAbstract(this SurrogateType surrogateType)
            => surrogateType.TypeFields1.HasFlag(TypeFields.Abstract);

        public static bool IsGenericType(this SurrogateType surrogateType)
            => surrogateType.TypeFields1.HasFlag(TypeFields.GenericType);
        public static bool IsCollection(this SurrogateType surrogateType)
            => surrogateType.TypeFields1.HasFlag(TypeFields.Collection);
        public static bool IsDictionary(this SurrogateType surrogateType)
            => surrogateType.TypeFields1.HasFlag(TypeFields.Dictionary);

        public static bool IsEnum(this SurrogateType surrogateType)
            => surrogateType.TypeFields1.HasFlag(TypeFields.Enum);
        public static bool IsNullable(this SurrogateType surrogateType)
            => surrogateType.TypeFields1.HasFlag(TypeFields.Nullable);


    }
}
