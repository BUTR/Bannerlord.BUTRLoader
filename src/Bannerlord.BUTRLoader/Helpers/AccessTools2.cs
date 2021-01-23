using Bannerlord.BUTR.Shared.Helpers;

using System;
using System.Reflection;

using static HarmonyLib.AccessTools;

namespace Bannerlord.BUTRLoader.Helpers
{
    /// <summary>
    /// https://github.com/BUTR/Bannerlord.ButterLib/blob/dev/src/Bannerlord.ButterLib/Helpers/Harmony/AccessTools2.cs
    /// </summary>
    internal static class AccessTools2
    {
        public static FieldRef<object, TField>? FieldRefAccess<TField>(Type type, string fieldName)
        {
            var field = Field(type, fieldName);
            return field is null ? null : FieldRefAccess<object, TField>(field);
        }

        public static TDelegate? GetDelegate<TDelegate>(MethodInfo? methodInfo) where TDelegate : Delegate => ReflectionHelper.GetDelegate<TDelegate>(methodInfo);

        public static TDelegate? GetDelegate<TDelegate>(Type type, string method) where TDelegate : Delegate => GetDelegate<TDelegate>(Method(type, method));
    }
}