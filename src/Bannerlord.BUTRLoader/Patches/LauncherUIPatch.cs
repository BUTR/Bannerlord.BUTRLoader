using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;

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
            // Add out Prefabs content folder
            ____context.ResourceDepot.AddLocation("GUI/Bannerlord.BUTRLoader/");
            ____context.ResourceDepot.CollectResources();

            // Make sure WidgetFactory will register the new files
            ____widgetFactory.CheckForUpdates();

            // Replace the original VM with our own
            MixinManager.AddMixins(____viewModel);

            // Dispose old movie and create the new
            ____movie.Release();
            ____movie = GauntletMovie.Load(____context, ____widgetFactory, "Bannerlord.BUTRLoader.UILauncher", ____viewModel);
        }

        /*
        private static void InitializePostfix(
            GauntletMovie ____movie,
            LauncherVM ____viewModel,
            //UserDataManager ____userDataManager,
            //Action ____onClose,
            //Action ____onMinimize,
            UIContext ____context,
            WidgetFactory ____widgetFactory)
        {
            // Add out Prefabs content folder
            ____context.ResourceDepot.AddLocation("GUI/Bannerlord.BUTRLoader/");
            ____context.ResourceDepot.CollectResources();
            // Make sure WidgetFactory will register the new files
            ____widgetFactory.CheckForUpdates();
            // Replace the original VM with our own
            //____viewModel = new LauncherVM2(____userDataManager, ____onClose, ____onMinimize);
            //____viewModel = new LauncherVM2(____viewModel);
            MixinManager.AddMixins(____viewModel);
            ____movie.Release();
            ____movie = GauntletMovie.Load(____context, ____widgetFactory, "Bannerlord.BUTRLoader.UILauncher", ____viewModel);
        }
        */
    }
}