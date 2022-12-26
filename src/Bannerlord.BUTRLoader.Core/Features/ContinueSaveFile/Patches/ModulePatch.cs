using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.BUTRLoader.Features.ContinueSaveFile.Patches
{
    internal static class ModulePatch
    {
        public static event Action<GameStartupInfo, string>? OnSaveGameArgParsed;

        public static bool Enable(Harmony harmony)
        {
            return true &
                   harmony.TryPatch(
                       AccessTools2.DeclaredMethod(typeof(Module), "ProcessApplicationArguments"),
                       postfix: AccessTools2.DeclaredMethod(typeof(ModulePatch), nameof(ProcessApplicationArgumentsPostfix)));
        }

        private static void ProcessApplicationArgumentsPostfix(Module __instance)
        {
            var array = Utilities.GetFullCommandLineString().Split(' ');
            for (var i = 0; i < array.Length; i++)
            {
                if (!string.Equals(array[i], "/continuesave", StringComparison.OrdinalIgnoreCase)) continue;
                if (array.Length <= i + 1) continue;
                var saveGame = array[i + 1];
                OnSaveGameArgParsed?.Invoke(__instance.StartupInfo, saveGame);
            }
        }
    }
}