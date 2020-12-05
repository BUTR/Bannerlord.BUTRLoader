using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bannerlord.BUTRLoader.WithHarmony
{
    internal sealed class Harmony
    {
        private static Assembly? HarmonyAssembly { get; } = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic)
            .FirstOrDefault(a => Path.GetFileNameWithoutExtension(a.CodeBase) == "0Harmony");
        private static Type? HarmonyType { get; } = HarmonyAssembly?.GetType("HarmonyLib.Harmony");
        private static MethodInfo? HarmonyPatchMethod { get; } = HarmonyType?.GetMethod("Patch");

        public readonly object? RealHarmony;

        public Harmony(string id)
        {
            if (HarmonyType == null)
                return;

            RealHarmony = Activator.CreateInstance(HarmonyType, id);
        }

        public void Patch(MethodInfo toPatch, HarmonyMethod? prefix = null, HarmonyMethod? postfix = null, HarmonyMethod? transpiler = null, HarmonyMethod? finalizer = null)
        {
            if (RealHarmony == null || HarmonyPatchMethod == null)
                return;

            HarmonyPatchMethod.Invoke(RealHarmony, new[] { toPatch, prefix?.RealHarmonyMethod, postfix?.RealHarmonyMethod, transpiler?.RealHarmonyMethod, finalizer?.RealHarmonyMethod });
        }
    }
}