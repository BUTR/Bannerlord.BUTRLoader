using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherVMPatch
    {
        private static readonly Type? UserDataManagerType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.UserDatas.UserDataManager") ??
                                                             AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.Library.UserDatas.UserDataManager");
        
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method("TaleWorlds.MountAndBlade.Launcher.LauncherVM:ExecuteConfirmUnverifiedDLLStart") ??
                AccessTools2.Method("TaleWorlds.MountAndBlade.Launcher.Library.LauncherVM:ExecuteConfirmUnverifiedDLLStart"),
                transpiler: AccessTools2.Method("Bannerlord.BUTRLoader.Patches.LauncherVMPatch:BlankTranspiler"));
            if (!res1) return false;

            var res2 = harmony.TryCreateReversePatcher(
                AccessTools2.Method("TaleWorlds.MountAndBlade.Launcher.LauncherVM:UpdateAndSaveUserModsData") ??
                AccessTools2.Method("TaleWorlds.MountAndBlade.Launcher.Library.LauncherVM:UpdateAndSaveUserModsData"),
                AccessTools2.Method("Bannerlord.BUTRLoader.Patches.LauncherVMPatch:UpdateAndSaveUserModsData"));
            if (res2 is null) return false;
            res2.Patch();

            // Preventing inlining ExecuteConfirmUnverifiedDLLStart
            harmony.TryPatch(
                AccessTools2.Constructor("TaleWorlds.MountAndBlade.Launcher.LauncherVM", new[] { UserDataManagerType!, typeof(Action), typeof(Action) }) ??
                AccessTools2.Constructor("TaleWorlds.MountAndBlade.Launcher.Library.LauncherVM", new[] { UserDataManagerType!, typeof(Action), typeof(Action) }),
                transpiler: AccessTools2.Method("Bannerlord.BUTRLoader.Patches.LauncherVMPatch:BlankTranspiler"));
            // Preventing inlining ExecuteConfirmUnverifiedDLLStart

            return true;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void UpdateAndSaveUserModsData(object instance, bool isMultiplayer)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}