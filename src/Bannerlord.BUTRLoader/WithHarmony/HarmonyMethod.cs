using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bannerlord.BUTRLoader.WithHarmony
{
    internal sealed class HarmonyMethod
    {
        private static Assembly? HarmonyAssembly { get; } = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic)
            .FirstOrDefault(a => Path.GetFileNameWithoutExtension(a.CodeBase) == "0Harmony");
        private static Type? HarmonyMethodType { get; } = HarmonyAssembly?.GetType("HarmonyLib.HarmonyMethod");

        public readonly object? RealHarmonyMethod;

        public HarmonyMethod(MethodInfo methodInfo)
        {
            if (HarmonyMethodType == null)
                return;

            RealHarmonyMethod = Activator.CreateInstance(HarmonyMethodType, methodInfo);
        }
    }
}