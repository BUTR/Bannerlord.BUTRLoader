﻿using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.ModuleInfoExtended;

using HarmonyLib;

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
                AccessTools.Method("TaleWorlds.MountAndBlade.Launcher.Program:StartGame"),
                postfix: AccessTools.Method(typeof(ProgramPatch), nameof(StartGamePrefix)));
            if (!res1) return false;

            return true;
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void StartGamePrefix()
        {
            if (BUTRLoaderAppDomainManager.UnblockFiles)
            {
                if (!Directory.Exists(ModuleInfo2.PathPrefix))
                    return;

                try { NtfsUnblocker.UnblockPath(ModuleInfo2.PathPrefix, "*.dll"); }
                catch { }
            }
        }
    }
}