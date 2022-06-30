using Bannerlord.BUTRLoader.Wrappers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherModuleVMPatch
    {
        public static readonly ConditionalWeakTable<object, Delegate> AreAllDepenenciesPresentReferences = new();

        private static readonly Type? LauncherModuleVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.LauncherModuleVM") ??
                                                            AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM");
        
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools.FirstConstructor(LauncherModuleVMType, ci => ci.GetParameters().Length > 0),
                postfix: AccessTools2.Method("Bannerlord.BUTRLoader.Patches.LauncherModuleVMPatch:LauncherModuleVMConstructorPostfix"));
            if (!res1) return false;

            return true;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LauncherModuleVMConstructorPostfix(object __instance, Delegate __3)
        {
            var wrapper = LauncherModuleVMWrapper.Create(__instance);
            // Except the Native module
            if (wrapper.Info?.Id.Equals("native", StringComparison.OrdinalIgnoreCase) == true)
                return;

            // We need to save isAllDepenenciesPresent delegate for Mixin's usage
            AreAllDepenenciesPresentReferences.Add(__instance, __3);
        }
    }
}