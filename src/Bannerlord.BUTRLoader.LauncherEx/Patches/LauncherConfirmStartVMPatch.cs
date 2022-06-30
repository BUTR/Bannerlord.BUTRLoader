using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherConfirmStartVMPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.LauncherConfirmStartVM:EnableWith") ??
                AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.Library.LauncherConfirmStartVM:EnableWith"),
                prefix: AccessTools2.DeclaredMethod("Bannerlord.BUTRLoader.Patches.LauncherConfirmStartVMPatch:EnableWithPrefix"));
            if (!res1)
            {
                var res2 = harmony.TryPatch(
                    AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.LauncherConfirmStartVM:EnableWith") ??
                    AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.Library.LauncherConfirmStartVM:EnableWith"),
                    prefix: AccessTools2.DeclaredMethod("Bannerlord.BUTRLoader.Patches.LauncherConfirmStartVMPatch:EnableWithPrefix2"));
                if (!res2)
                    return false;
            }

            return true;
        }

        public static bool EnableWithPrefix(Action? ___onConfirm)
        {
            ___onConfirm?.Invoke();
            return false;
        }
        public static bool EnableWithPrefix2(Action? ____onConfirm)
        {
            ____onConfirm?.Invoke();
            return false;
        }
    }
}