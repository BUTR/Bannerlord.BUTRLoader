﻿using HarmonyLib;

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Bannerlord.BUTRLoader.Helpers
{
    /// <summary>
    /// https://github.com/BUTR/Bannerlord.ButterLib/blob/dev/src/Bannerlord.ButterLib/Helpers/Harmony/SymbolExtensions2.cs
    /// </summary>
    public static class SymbolExtensions2
    {
        public static AccessTools.FieldRef<TObject, TField>? FieldRefAccess<TObject, TField>(Expression<Func<TObject, TField>> expression)
        {
            return FieldRefAccess<TObject, TField>((LambdaExpression)expression);
        }
        public static AccessTools.FieldRef<TObject, TField>? FieldRefAccess<TObject, TField>(LambdaExpression expression)
        {
            if (expression.Body is MemberExpression { Member: FieldInfo fieldInfo })
                return AccessTools.FieldRefAccess<TObject, TField>(fieldInfo);

            throw new ArgumentException("Invalid Expression. Expression should consist of a Field return only.");
        }


        public static ConstructorInfo GetConstructorInfo<T>(Expression<Func<T>> expression)
        {
            return GetConstructorInfo((LambdaExpression) expression);
        }
        public static ConstructorInfo GetConstructorInfo<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return GetConstructorInfo((LambdaExpression) expression);
        }
        public static ConstructorInfo GetConstructorInfo(LambdaExpression expression)
        {
            if (expression.Body is NewExpression { Constructor: { } } body)
                return body.Constructor;

            throw new ArgumentException("Invalid Expression. Expression should consist of a Field return only.");
        }

        public static FieldInfo GetFieldInfo<T>(Expression<Func<T>> expression)
        {
            return GetFieldInfo((LambdaExpression) expression);
        }
        public static FieldInfo GetFieldInfo<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return GetFieldInfo((LambdaExpression) expression);
        }
        public static FieldInfo GetFieldInfo(LambdaExpression expression)
        {
            if (expression.Body is MemberExpression { Member: FieldInfo fieldInfo })
                return fieldInfo;
            throw new ArgumentException("Invalid Expression. Expression should consist of a Field return only.");
        }

        public static PropertyInfo GetPropertyInfo<T>(Expression<Func<T>> expression)
        {
            return GetPropertyInfo((LambdaExpression) expression);
        }
        public static PropertyInfo GetPropertyInfo<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return GetPropertyInfo((LambdaExpression) expression);
        }
        public static PropertyInfo GetPropertyInfo(LambdaExpression expression)
        {
            if (expression.Body is MemberExpression { Member: PropertyInfo propertyInfo })
                return propertyInfo;

            throw new ArgumentException("Invalid Expression. Expression should consist of a Property return only.");
        }
    }
}