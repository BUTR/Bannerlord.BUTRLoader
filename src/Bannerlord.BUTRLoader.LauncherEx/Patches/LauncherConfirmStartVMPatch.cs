using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherConfirmStartVMPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(typeof(LauncherConfirmStartVM), "EnableWith"),
                prefix: AccessTools2.Method(typeof(LauncherConfirmStartVMPatch), nameof(EnableWithPrefix)));
            if (!res1) return false;

            return true;
        }

        public static bool EnableWithPrefix(LauncherConfirmStartVM instance, Action ___onConfirm)
        {
            ___onConfirm?.Invoke();
            return false;
        }
    }
}