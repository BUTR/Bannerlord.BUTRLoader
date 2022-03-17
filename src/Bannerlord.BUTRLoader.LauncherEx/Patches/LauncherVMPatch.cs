﻿using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherVMPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(LauncherVMWrapper.LauncherVMType!, "ExecuteConfirmUnverifiedDLLStart"),
                transpiler: AccessTools2.Method(typeof(LauncherVMPatch), nameof(BlankTranspiler)));
            if (!res1) return false;

            var res2 = harmony.TryCreateReversePatcher(
                AccessTools2.Method(LauncherVMWrapper.LauncherVMType!, "UpdateAndSaveUserModsData"),
                AccessTools2.Method(typeof(LauncherVMPatch), "UpdateAndSaveUserModsData"));
            if (res2 is null) return false;
            res2.Patch();

            // Preventing inlining ExecuteConfirmUnverifiedDLLStart
            harmony.TryPatch(
                AccessTools2.Constructor(LauncherVMWrapper.LauncherVMType!, new[] { UserDataManagerWrapper.UserDataManagerType!, typeof(Action), typeof(Action) }),
                transpiler: AccessTools2.Method(typeof(LauncherVMPatch), nameof(BlankTranspiler)));
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