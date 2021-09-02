using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    public static class PropertyKindExtensions
    {
        public static bool IsBasicType(this PropertyKind propertyKind) =>
            propertyKind == PropertyKind.BasicType ||
            propertyKind == PropertyKind.Enum;

        public static bool IsOneToOne(this PropertyKind propertyKind) =>
            propertyKind == PropertyKind.OneToOneToDomain ||
            propertyKind == PropertyKind.OneToOneToUnknown;
        public static bool IsOneToMany(this PropertyKind propertyKind) =>
            propertyKind == PropertyKind.OneToManyToUnknown ||
            propertyKind == PropertyKind.OneToManyToDomain ||
            propertyKind == PropertyKind.OneToManyEnum ||
            propertyKind == PropertyKind.OneToManyBasicType;
    }
}