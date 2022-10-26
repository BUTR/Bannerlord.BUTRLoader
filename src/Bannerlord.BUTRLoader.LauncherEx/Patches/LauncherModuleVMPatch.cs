using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherModuleVMPatch
    {
        public static readonly ConditionalWeakTable<object, Delegate> AreAllDepenenciesPresentReferences = new();

        private static readonly Type? LauncherModuleVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM");

        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools.FirstConstructor(LauncherModuleVMType, ci => ci.GetParameters().Length > 0),
                postfix: AccessTools2.DeclaredMethod(typeof(LauncherModuleVMPatch), nameof(LauncherModuleVMConstructorPostfix)));
            if (!res1) return false;

            return true;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LauncherModuleVMConstructorPostfix(LauncherModuleVM __instance, Delegate __3)
        {
            // Except the Native module
            if (__instance.Info.Id.Equals("native", StringComparison.OrdinalIgnoreCase))
                return;

            // We need to save isAllDepenenciesPresent delegate for Mixin's usage
            AreAllDepenenciesPresentReferences.Add(__instance, __3);
        }
    }
}