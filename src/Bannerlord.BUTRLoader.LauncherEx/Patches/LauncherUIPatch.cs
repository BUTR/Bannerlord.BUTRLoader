using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherUIPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.DeclaredMethod(typeof(LauncherUI), "Initialize"),
                postfix: AccessTools2.DeclaredMethod(typeof(LauncherUIPatch), nameof(InitializePostfix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                AccessTools2.DeclaredMethod(typeof(LauncherUI), "Update"),
                postfix: AccessTools2.DeclaredMethod(typeof(LauncherUIPatch), nameof(UpdatePostfix)));
            if (!res2) return false;

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

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UpdatePostfix(UIContext ____context)
        {
            if (Input.InputManager is BUTRInputManager butrInputManager)
            {
                butrInputManager.Update();

                if (____context?.EventManager?.FocusedWidget is { } focusedWidget)
                {
                    focusedWidget.HandleInput(butrInputManager.ReleasedChars.Select(x => (int) x).ToArray());
                }
            }
            else
            {
                Input.Initialize(new BUTRInputManager(Input.InputManager), null);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}