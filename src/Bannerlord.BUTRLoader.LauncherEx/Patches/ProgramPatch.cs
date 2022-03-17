using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Patches
{
    internal static class ProgramPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method("TaleWorlds.MountAndBlade.Launcher.LauncherVM:ExecuteStartGame"),
                prefix: AccessTools2.Method(typeof(ProgramPatch), nameof(StartGamePrefix)));
            if (!res1) return false;

            return true;
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool StartGamePrefix()
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

            return true;
        }

        private static string PathPrefix() => Path.Combine(TaleWorlds.Library.BasePath.Name, "Modules");
    }
}