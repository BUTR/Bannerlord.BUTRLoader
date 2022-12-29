using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.LauncherEx;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.IO;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class ProgramPatch
    {
        private static string PathPrefix() => Path.Combine(BasePath.Name, "Modules");

        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(typeof(Program), "AuxFinalize"),
                prefix: AccessTools2.DeclaredMethod(typeof(ProgramPatch), nameof(AuxFinalizePrefix)));
            if (!res1) return false;

            return true;
        }

        private static void AuxFinalizePrefix()
        {
            var pathPrefix = PathPrefix();
            if (LauncherSettings.UnblockFiles)
            {
                if (Directory.Exists(pathPrefix))
                {
                    try
                    {
                        NtfsUnblocker.UnblockPath(pathPrefix, "*.dll");
                    }
                    catch { }
                }
            }

            if (LauncherSettings.FixCommonIssues)
            {
                IssuesChecker.CheckForRootHarmony();
            }

            Manager.Disable();
        }

    }
}