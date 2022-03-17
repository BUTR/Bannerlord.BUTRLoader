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
        private delegate void OnInitializeSubModulesPrefixDelegate();
        private delegate void OnLoadSubModulesPostfixDelegate();

        private static IEnumerable<Type> GetInterceptorTypes()
        {
            static bool CheckType(Type type) => type.GetCustomAttributes()
                .Any(att => string.Equals(att.GetType().FullName, typeof(BUTRLoaderInterceptorAttribute).FullName, StringComparison.Ordinal));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(asm => !asm.IsDynamic && asm.CodeBase.Contains("Modules")))
            {
                IEnumerable<Type> enumerable;
                try
                {
                    var types = assembly.GetTypes(); // Force type resolution
                    enumerable = types.Where(CheckType);
                }
                catch (ReflectionTypeLoadException)
                {
                    enumerable = Enumerable.Empty<Type>(); // ignore the incompatibility, not our problem
                }
                foreach (var type in enumerable)
                {
                    yield return type;
                }
            }
        }

        public static void Enable(Harmony harmony)
        {
            ModulePatch.OnInitializeSubModulesPrefix += OnInitializeSubModulesPrefix;
            ModulePatch.OnLoadSubModulesPostfix += OnLoadSubModulesPostfix;
            ModulePatch.Enable(harmony);
        }

        private static void OnInitializeSubModulesPrefix()
        {
            foreach (var type in GetInterceptorTypes())
            {
                if (AccessTools2.GetDelegate<OnInitializeSubModulesPrefixDelegate>(type, "OnInitializeSubModulesPrefix") is { } method)
                {
                    method();
                }
            }
        }

        private static void OnLoadSubModulesPostfix()
        {
            foreach (var type in GetInterceptorTypes())
            {
                if (AccessTools2.GetDelegate<OnLoadSubModulesPostfixDelegate>(type, "OnLoadSubModulesPostfix") is { } method)
                {
                    method();
                }
            }
        }
    }
}