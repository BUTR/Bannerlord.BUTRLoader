using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherUIPatch
    {
        public static event EventHandler? OnInitialize;
        public static WidgetFactory WidgetFactory { get; private set; } = default!;

        public static void Enable(Harmony harmony)
        {
            harmony.Patch(
                typeof(LauncherUI).GetMethod("Initialize", ReflectionHelper.All)!,
                postfix: new HarmonyMethod(typeof(LauncherUIPatch).GetMethod(nameof(InitializePostfix), ReflectionHelper.All)!));
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializePostfix(GauntletMovie ____movie, LauncherVM ____viewModel, UIContext ____context, WidgetFactory ____widgetFactory)
        {
            WidgetFactory = ____widgetFactory;
            OnInitialize?.Invoke(null, EventArgs.Empty);

            // Add to the existing VM our own properties
            MixinManager.AddMixins(____viewModel);

            // Dispose old movie and create the new
            ____movie.Release();
            ____movie = GauntletMovie.Load(____context, ____widgetFactory, "UILauncher", ____viewModel);
        }
    }
}