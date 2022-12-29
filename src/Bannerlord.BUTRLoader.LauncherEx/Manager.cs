using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Patches;
using Bannerlord.BUTRLoader.ResourceManagers;
using Bannerlord.BUTRLoader.Widgets;

using HarmonyLib;

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

[assembly: InternalsVisibleTo("Bannerlord.BUTRLoader.Tests")]

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.LauncherEx
{
    public static class Manager
    {
        internal static readonly AssemblyCompatibilityChecker _compatibilityChecker = new();
        private static readonly Harmony _launcherHarmony = new("bannerlord.butrloader.launcherex");

        public static event Action? OnDisable;

        public static void Initialize()
        {
            AssemblyLoaderPatch.Enable(_launcherHarmony);
        }

        public static void Enable()
        {
            ProgramPatch.Enable(_launcherHarmony);
            UserDataManagerPatch.Enable(_launcherHarmony);
            LauncherVMPatch.Enable(_launcherHarmony);
            LauncherModsVMPatch.Enable(_launcherHarmony);
            LauncherConfirmStartVMPatch.Enable(_launcherHarmony);
            LauncherUIPatch.Enable(_launcherHarmony);
            ViewModelPatch.Enable(_launcherHarmony);
            WidgetPrefabPatch.Enable(_launcherHarmony);

            GraphicsContextManager.Enable(_launcherHarmony);
            GraphicsContextManager.CreateAndRegister("launcher_arrow_down", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.arrow_down.png"));
            GraphicsContextManager.CreateAndRegister("launcher_arrow_left", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.arrow_left.png"));
            GraphicsContextManager.CreateAndRegister("launcher_export", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.export.png"));
            GraphicsContextManager.CreateAndRegister("launcher_import", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.import.png"));
            GraphicsContextManager.CreateAndRegister("launcher_refresh", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.refresh.png"));
            GraphicsContextManager.CreateAndRegister("launcher_warm_overlay", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.warm_overlay.png"));
            GraphicsContextManager.CreateAndRegister("launcher_folder", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.folder.png"));
            GraphicsContextManager.CreateAndRegister("launcher_search", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.search.png"));

            SpriteDataManager.Enable(_launcherHarmony);
            SpriteDataManager.CreateAndRegister("launcher_arrow_down");
            SpriteDataManager.CreateAndRegister("launcher_arrow_left");
            SpriteDataManager.CreateAndRegister("launcher_import");
            SpriteDataManager.CreateAndRegister("launcher_export");
            SpriteDataManager.CreateAndRegister("launcher_refresh");
            SpriteDataManager.CreateAndRegister("launcher_warm_overlay");
            SpriteDataManager.CreateAndRegister("launcher_folder");
            SpriteDataManager.CreateAndRegister("launcher_search");

            BrushFactoryManager.Enable(_launcherHarmony);
            BrushFactoryManager.CreateAndRegister(Load("Bannerlord.BUTRLoader.Resources.Brushes.Launcher.xml"));

            WidgetFactoryManager.Enable(_launcherHarmony);
            WidgetFactoryManager.Register(typeof(LauncherToggleButtonWidget));
            WidgetFactoryManager.Register(typeof(LauncherSearchBoxWidget));

            WidgetFactoryManager.CreateAndRegister("Launcher.ToggleButton", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Widgets.Launcher.ToggleButton.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SearchBox", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Widgets.Launcher.SearchBox.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Scrollbar", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Widgets.Launcher.Scrollbar.xml"));

            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyBoolView", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Properties.Launcher.SettingsPropertyBoolView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyButtonView", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Properties.Launcher.SettingsPropertyButtonView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyFloatView", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Properties.Launcher.SettingsPropertyFloatView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyIntView", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Properties.Launcher.SettingsPropertyIntView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyStringView", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Properties.Launcher.SettingsPropertyStringView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Options", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Options.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Options.OptionTuple", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Options.OptionTuple.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Mods2", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Mods.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Mods.ModuleTuple2", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Mods.ModuleTuple.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Saves", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Saves.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Saves.SaveTuple", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Saves.SaveTuple.xml"));

        }

        private static XmlDocument Load(string embedPath)
        {
            using var stream = typeof(Manager).Assembly.GetManifestResourceStream(embedPath);
            if (stream is null) throw new Exception($"Could not find embed resource '{embedPath}'!");
            using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true });
            var doc = new XmlDocument();
            doc.Load(xmlReader);
            return doc;
        }
        private static byte[] LoadRaw(string embedPath)
        {
            static byte[] ReadFully(Stream input)
            {
                byte[] buffer = new byte[16 * 1024];
                using var ms = new MemoryStream();
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }

            using var stream = typeof(Manager).Assembly.GetManifestResourceStream(embedPath);
            if (stream is null) throw new Exception($"Could not find embed resource '{embedPath}'!");
            return ReadFully(stream);
        }

        public static void Disable()
        {
            OnDisable?.Invoke();
            _compatibilityChecker.Dispose();
            _launcherHarmony.UnpatchAll(_launcherHarmony.Id);
        }
    }
}