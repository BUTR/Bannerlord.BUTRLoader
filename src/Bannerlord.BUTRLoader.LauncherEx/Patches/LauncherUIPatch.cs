using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherUIPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.Library.LauncherUI:Initialize"),
                postfix: AccessTools2.DeclaredMethod(typeof(LauncherUIPatch), nameof(InitializePostfix)));
            if (!res1) return false;

            // Preventing inlining Initialize
            harmony.TryPatch(
                AccessTools2.Method("TaleWorlds.MountAndBlade.Launcher.Library.LauncherUI:Update"),
                transpiler: AccessTools2.DeclaredMethod(typeof(LauncherUIPatch), nameof(BlankTranspiler)));
            // Preventing inlining Initialize

            return true;
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializePostfix(GauntletMovie ____movie, LauncherVM ____viewModel)
        {
            // Add to the existing VM our own properties
            MixinManager.AddMixins(____viewModel);
            ____movie.RefreshDataSource(____viewModel);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}