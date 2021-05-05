using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherModuleVMPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools.FirstConstructor(typeof(LauncherModuleVM), ci => ci.GetParameters().Length > 0),
                postfix: AccessTools.Method(typeof(LauncherModuleVMPatch), nameof(LauncherModuleVMConstructorPostfix)));
            if (!res1) return false;

            return true;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LauncherModuleVMConstructorPostfix(LauncherModuleVM __instance, Delegate __3)
        {
            var wrapper = LauncherModuleVMWrapper.Create(__instance);

            // Do not disable Native mods

            // Except the Native mod
            if (wrapper.Info.Id.Equals("native", StringComparison.OrdinalIgnoreCase))
                return;

            // Recalculate IsDisabled since it checked for IsOfficial
            __instance.IsDisabled = !((bool) __3.DynamicInvoke(wrapper.Info.Object));
        }
    }
}