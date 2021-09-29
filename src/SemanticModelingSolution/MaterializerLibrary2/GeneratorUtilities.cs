﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using SemanticLibrary;

using SurrogateLibrary;

namespace MaterializerLibrary
{
    internal static class GeneratorUtilities
    {
        public static Expression JsonWriteNull(ParameterExpression writer, string propertyName)
            => JsonWrite(writer, KnownMethods.WriteNullValue, propertyName);
        public static Expression JsonWriteStartArray(ParameterExpression writer, string propertyName)
            => JsonWrite(writer, KnownMethods.WriteStartArray, propertyName);
        public static Expression JsonWriteEndArray(ParameterExpression writer)
            => JsonWrite(writer, KnownMethods.WriteEndArray);
        public static Expression JsonWriteStartObject(ParameterExpression writer)
            => JsonWrite(writer, KnownMethods.WriteStartObject0);
        public static Expression JsonWritePropertyName(ParameterExpression writer, string propertyName)
            => JsonWrite(writer, KnownMethods.WritePropertyName, propertyName);
        public static Expression JsonWriteStartObject(ParameterExpression writer, string propertyName)
            => JsonWrite(writer, KnownMethods.WriteStartObject1, propertyName);
        public static Expression JsonWriteEndObject(ParameterExpression writer)
            => JsonWrite(writer, KnownMethods.WriteEndObject);

        public static Expression JsonWriteValue(ParameterExpression writer, string propertyName, Expression valueExpression)
        {
            if (valueExpression.Type == typeof(string)) return JsonWriteStringString(writer, propertyName, valueExpression);
            if (valueExpression.Type == typeof(Guid)) return JsonWriteStringGuid(writer, propertyName, valueExpression);
            if (valueExpression.Type == typeof(DateTime)) return JsonWriteStringDateTime(writer, propertyName, valueExpression);
            if (valueExpression.Type == typeof(DateTimeOffset)) return JsonWriteStringDateTimeOffset(writer, propertyName, valueExpression);
            if (valueExpression.Type == typeof(bool)) return JsonWriteBoolean(writer, propertyName, valueExpression);

            if (valueExpression.Type == typeof(decimal)) return JsonWriteNumberDecimal(writer, propertyName, valueExpression);
            if (valueExpression.Type == typeof(double)) return JsonWriteNumberDouble(writer, propertyName, valueExpression);
            if (valueExpression.Type == typeof(float)) return JsonWriteNumberFloat(writer, propertyName, valueExpression);
            if (valueExpression.Type == typeof(Int32)) return JsonWriteNumberInt32(writer, propertyName, valueExpression);
            if (valueExpression.Type == typeof(UInt32)) return JsonWriteNumberUInt32(writer, propertyName, valueExpression);
            if (valueExpression.Type == typeof(Int64)) return JsonWriteNumberInt64(writer, propertyName, valueExpression);
            if (valueExpression.Type == typeof(UInt64)) return JsonWriteNumberUInt64(writer, propertyName, valueExpression);
            throw new Exception($"Unsupported basic type: {valueExpression.Type.FullName}");
        }

        /// <summary>
        /// If the valueExpression is a null string, the JsonWriteString will write the null json token
        /// </summary>
        public static Expression JsonWriteStringString(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteStringString, propertyName, valueExpression);
        public static Expression JsonWriteStringGuid(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteStringGuid, propertyName, valueExpression);
        public static Expression JsonWriteStringDateTime(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteStringDateTime, propertyName, valueExpression);
        public static Expression JsonWriteStringDateTimeOffset(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteStringDateTimeOffset, propertyName, valueExpression);
        public static Expression JsonWriteBoolean(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteBoolean, propertyName, valueExpression);

        public static Expression JsonWriteNumberDecimal(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteNumberDecimal, propertyName, valueExpression);
        public static Expression JsonWriteNumberDouble(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteNumberDouble, propertyName, valueExpression);
        public static Expression JsonWriteNumberFloat(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteNumberFloat, propertyName, valueExpression);
        public static Expression JsonWriteNumberInt32(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteNumberInt32, propertyName, valueExpression);
        public static Expression JsonWriteNumberUInt32(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteNumberUInt32, propertyName, valueExpression);
        public static Expression JsonWriteNumberInt64(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteNumberInt64, propertyName, valueExpression);
        public static Expression JsonWriteNumberUInt64(ParameterExpression writer, string propertyName, Expression valueExpression)
            => JsonWrite(writer, KnownMethods.WriteNumberUInt64, propertyName, valueExpression);


        public static Expression JsonWrite(ParameterExpression writer, MethodInfo writeMethod)
        {
            return Expression.Call(writer, writeMethod);
        }

        public static Expression JsonWrite(ParameterExpression writer, MethodInfo writeMethod,
            string propertyName)
        {
            var propertyNameExpression = Expression.Constant(propertyName);
            return Expression.Call(writer, writeMethod, propertyNameExpression);
        }

        public static Expression JsonWrite(ParameterExpression writer, MethodInfo writeMethod,
            string propertyName, Expression valueExpression)
        {
            var propertyNameExpression = Expression.Constant(propertyName);
            return Expression.Call(writer, writeMethod, propertyNameExpression, valueExpression);
        }

        public static (Type root, IList<(PropertyInfo propertyInfo, PropertyKind propertyKind, Type propCoreType)> segments)
            CreateSegments(TypeSystem<Metadata> typeSystem, /*ModelNavigationNode*/NavigationSegment<Metadata> modelNavigationNode)
        {
            var segments = new List<(PropertyInfo, PropertyKind, Type propCoreType)>();
            var temp = modelNavigationNode;
            Type root = null;
            while (temp != null && (!temp.IsOneToMany || segments.Count == 0))
            {
                var propInfo = temp.Property.GetOriginalPropertyInfo(typeSystem);
                var propCoreType = temp.Property.PropertyType.GetCoreType().GetOriginalType();
                var propKind = temp.Property.GetKind();
                if (temp.Previous == null) root = temp.Property.OwnerType.GetOriginalType();
                temp = temp.Previous;
                segments.Insert(0, (propInfo, propKind, propCoreType));
            }

            return (root, segments);
        }

        /// <summary>
        /// This expression cannot return a typed lambda because
        /// the return type is inside the metadata and not known type at compile time
        /// </summary>
        public static Expression CreateGetValue(TypeSystem<Metadata> typeSystem, ParameterExpression inputObject,
            /*ModelNavigationNode*/NavigationSegment<Metadata> modelNavigationNode)
        {
            var (_, segments) = CreateSegments(typeSystem, modelNavigationNode);
            //var segs = string.Join(".", segments.Select(x => x.propertyInfo.Name));

            Expression parent = inputObject;
            for(int i=0; i<segments.Count; i++)
            {
                //a.b.c.d
                //r r v r
                var segment = segments[i];
                if (i != 0 && !parent.Type.IsValueType)
                {
                    // null check
                    var test = Expression.Equal(parent, Expression.Default(parent.Type));
                    parent = Expression.Condition(test,
                        Expression.Default(segment.propertyInfo.PropertyType),
                        Expression.Property(parent, segment.propertyInfo));
                }
                else
                {
                    parent = Expression.Property(parent, segment.propertyInfo);
                }
            }
            //foreach (var segment in segments)
            //{
            //    parent = Expression.Property(parent, segment.propertyInfo);
            //}

            return parent;
        }

        private static void test()
        {
            //Expression.Property(?, propInfo);

            W w = new(
                new X(
                    new Y("YName")
                    ),
                new List<Z>
                {
                    new Z(new List<X>()
                    {
                        new X(new Y("YNameC1")),
                        new X(new Y("YNameC2")),
                        new X(new Y("YNameC3")),
                    })
                });

            Debug.WriteLine(w.X.Y.Name);
            foreach (var v1 in w.Zs)
            {
                foreach (var v2 in v1.Xs)
                {
                    Debug.WriteLine(v2.Y.Name);
                }
            }


            A a = new(new B("B"), new List<B> { new B("B1"), new B("B2") });

            var coll = a.Bs;
            foreach (var item in coll)
            {
                Debug.WriteLine(item.Name);
            }


            var x = a.B;
            var x1 = a.B.Name;
            var x2 = a.Bs;
        }


        // maps:
        // A => W
        // A.B.Name => W.X.Y.Name
        // A.Bs.*.Name => W.Zs.*.Xs.*.Name

        private record A(B B, List<B> Bs);
        private record B(string Name);

        private record W(X X, List<Z> Zs);
        private record X(Y Y);
        private record Y(string Name);
        private record Z(List<X> Xs);




        /// <summary>
        /// https://stackoverflow.com/a/52762241/492913
        /// </summary>
        public static Expression ForEach(Expression enumerable, ParameterExpression loopVar, Expression loopContent)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(enumerable, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);
            var enumeratorDispose = Expression.Call(enumeratorVar, typeof(IDisposable).GetMethod("Dispose"));

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Expression.Label("LoopBreak");

            var trueConstant = Expression.Constant(true);

            var loop =
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, trueConstant),
                        Expression.Block(
                            new[] { loopVar },
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent),
                        Expression.Break(breakLabel)),
                    breakLabel);

            var tryFinally =
                Expression.TryFinally(
                    loop,
                    enumeratorDispose);

            var body =
                Expression.Block(
                    new[] { enumeratorVar },
                    enumeratorAssign,
                    tryFinally);

            return body;
        }

    }
}
