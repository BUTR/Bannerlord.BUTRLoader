using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Runtime.CompilerServices;

using Module = TaleWorlds.MountAndBlade.Module;

namespace Bannerlord.BUTRLoader.Features.Interceptor.Patches
{
    internal static class ModulePatch
    {
        public static event Action? OnInitializeSubModulesPrefix;
        public static event Action? OnLoadSubModulesPostfix;

        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(typeof(Module), "LoadSubModules"),
                postfix: AccessTools2.Method(typeof(ModulePatch), nameof(LoadSubModulesPostfix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                AccessTools2.Method(typeof(Module), "InitializeSubModules"),
                prefix: AccessTools2.Method(typeof(ModulePatch), nameof(InitializeSubModulesPrefix)));
            if (!res2) return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeSubModulesPrefix() => OnInitializeSubModulesPrefix?.Invoke();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadSubModulesPostfix() => OnLoadSubModulesPostfix?.Invoke();
    }
}