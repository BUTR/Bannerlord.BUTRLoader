using Bannerlord.BUTRLoader.Extensions;

using HarmonyLib;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using TaleWorlds.MountAndBlade.Launcher;

using static Bannerlord.BUTRLoader.Helpers.ModuleInfoHelper;

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

        public static void Disable(Harmony harmony)
        {
            harmony.Unpatch(
                AccessTools.Constructor(typeof(LauncherModuleVM)),
                AccessTools.Method(typeof(LauncherModuleVMPatch), nameof(LauncherModuleVMConstructorPostfix)));
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LauncherModuleVMConstructorPostfix(LauncherModuleVM __instance, Delegate __3)
        {
            // Do not disable Native mods
            if (GetInfo.GetValue(__instance) is { } info && GetId.Invoke(info, Array.Empty<object>()) is string id)
            {
                // Except the Native mod
                if (id.Equals("native", StringComparison.OrdinalIgnoreCase))
                    return;

                __instance.IsDisabled = !((bool) __3.DynamicInvoke(GetInfo.GetValue(__instance)));
            }
        }
    }
}