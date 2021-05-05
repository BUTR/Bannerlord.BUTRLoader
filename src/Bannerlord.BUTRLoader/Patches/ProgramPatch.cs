using Bannerlord.BUTR.Shared.ModuleInfoExtended;
using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class ProgramPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools.Method("TaleWorlds.MountAndBlade.Launcher.LauncherVM:ExecuteStartGame"),
                prefix: AccessTools.Method(typeof(ProgramPatch), nameof(StartGamePrefix)));
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
            if (BUTRLoaderAppDomainManager.UnblockFiles)
            {
                if (Directory.Exists(ModuleInfo2.PathPrefix))
                {
                    try
                    {
                        NtfsUnblocker.UnblockPath(ModuleInfo2.PathPrefix, "*.dll");
                    }
                    catch { }
                }
            }

            if (BUTRLoaderAppDomainManager.FixCommonIssues)
            {
                IssuesChecker.CheckForRootHarmony();
            }

            return true;
        }
    }
}