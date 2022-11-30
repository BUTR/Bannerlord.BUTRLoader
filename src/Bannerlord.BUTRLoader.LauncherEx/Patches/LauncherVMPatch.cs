using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherVMPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(typeof(LauncherVM), "ExecuteConfirmUnverifiedDLLStart"),
                transpiler: AccessTools2.DeclaredMethod(typeof(LauncherVMPatch), nameof(BlankTranspiler)));
            if (!res1) return false;

            var res2 = harmony.TryCreateReversePatcher(
                AccessTools2.Method(typeof(LauncherVM), "UpdateAndSaveUserModsData"),
                AccessTools2.DeclaredMethod(typeof(LauncherVMPatch), nameof(UpdateAndSaveUserModsData)));
            if (res2 is null) return false;
            res2.Patch();

            var res3 = harmony.TryPatch(
                AccessTools2.Method(typeof(LauncherVM), "GetApplicationVersionOfModule"),
                prefix: AccessTools2.DeclaredMethod(typeof(LauncherVMPatch), nameof(GetApplicationVersionOfModulePrefix)));
            if (!res3) return false;

            // Preventing inlining ExecuteConfirmUnverifiedDLLStart
            harmony.TryPatch(
                AccessTools2.Constructor(typeof(LauncherVM), new[] { typeof(UserDataManager), typeof(Action), typeof(Action) }),
                transpiler: AccessTools2.DeclaredMethod(typeof(LauncherVMPatch), nameof(BlankTranspiler)));
            // Preventing inlining ExecuteConfirmUnverifiedDLLStart

            return true;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void UpdateAndSaveUserModsData(object instance, bool isMultiplayer)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static bool GetApplicationVersionOfModulePrefix(string id, ref ApplicationVersion __result)
        {
            if (FeatureIds.Features.Contains(id))
            {
                __result = ApplicationVersion.Empty;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}