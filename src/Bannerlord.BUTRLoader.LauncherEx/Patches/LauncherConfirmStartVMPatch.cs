using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherConfirmStartVMPatch
    {
        private static readonly Type? LauncherConfirmStartVMType =
            AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.LauncherConfirmStartVM") ??
            AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.Library.LauncherConfirmStartVM");

        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(LauncherConfirmStartVMType!, "EnableWith"),
                prefix: AccessTools2.Method(typeof(LauncherConfirmStartVMPatch), nameof(EnableWithPrefix)));
            if (!res1) return false;

            return true;
        }

        public static bool EnableWithPrefix(Action ___onConfirm)
        {
            ___onConfirm?.Invoke();
            return false;
        }
    }
}