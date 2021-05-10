using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Runtime.CompilerServices;

using TaleWorlds.MountAndBlade;

namespace Bannerlord.BUTRLoader.Features.Interceptor.Patches
{
    internal static class ModulePatch
    {
        public static event Action? OnInitializeSubModulesPrefix;
        public static event Action? OnLoadSubModulesPostfix;

        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools.Method(typeof(Module), "LoadSubModules"),
                postfix: AccessTools.Method(typeof(ModulePatch), nameof(LoadSubModulesPostfix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                AccessTools.Method(typeof(Module), "InitializeSubModules"),
                prefix: AccessTools.Method(typeof(ModulePatch), nameof(InitializeSubModulesPrefix)));
            if (!res2) return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeSubModulesPrefix() => OnInitializeSubModulesPrefix?.Invoke();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadSubModulesPostfix() => OnLoadSubModulesPostfix?.Invoke();
    }
}