using Bannerlord.BUTRLoader.Features.Interceptor.Patches;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bannerlord.BUTRLoader.Features.Interceptor
{
    internal static class InterceptorFeature
    {
        private static IEnumerable<Type> GetInterceptorTypes() => AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Where(asm => asm.CodeBase.Contains("Modules"))
            .SelectMany(asm => asm.DefinedTypes.Where(type => type.GetCustomAttributes().Any(att =>
                string.Equals(att.GetType().FullName, typeof(BUTRLoaderInterceptorAttribute).FullName, StringComparison.Ordinal))));

        public static void Enable(Harmony harmony)
        {
            ModulePatch.OnInitializeSubModulesPrefix += OnInitializeSubModulesPrefix;
            ModulePatch.OnLoadSubModulesPostfix += OnLoadSubModulesPostfix;
            ModulePatch.Enable(harmony);
        }

        private static void OnInitializeSubModulesPrefix()
        {
            foreach (var interceptorType in GetInterceptorTypes())
            {
                if (AccessTools2.Method(interceptorType, "OnInitializeSubModulesPrefix") is { IsStatic: true } methodInfo)
                {
                    methodInfo.Invoke(null, Array.Empty<object>());
                }
            }
        }

        private static void OnLoadSubModulesPostfix()
        {
            foreach (var interceptorType in GetInterceptorTypes())
            {
                if (AccessTools2.Method(interceptorType, "OnLoadSubModulesPostfix") is { IsStatic: true } methodInfo)
                {
                    methodInfo.Invoke(null, Array.Empty<object>());
                }
            }
        }
    }
}