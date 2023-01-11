using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Localization;
using Bannerlord.BUTRLoader.Patches;
using Bannerlord.BUTRLoader.ResourceManagers;
using Bannerlord.BUTRLoader.TPac;
using Bannerlord.BUTRLoader.Widgets;

using HarmonyLib;

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

using TaleWorlds.Library;

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

            BUTRLocalizationManager.LoadLanguage(Load("Bannerlord.BUTRLoader.Resources.Localization.EN.strings.xml"));
            BUTRLocalizationManager.LoadLanguage(Load("Bannerlord.BUTRLoader.Resources.Localization.RU.strings.xml"));
            BUTRLocalizationManager.LoadLanguage(Load("Bannerlord.BUTRLoader.Resources.Localization.CNs.strings.xml"));
            BUTRLocalizationManager.ActiveLanguage = BUTRLocalizationManager.GetActiveLanguage();

            GraphicsContextManager.Enable(_launcherHarmony);
            GraphicsContextManager.CreateAndRegister("launcher_arrow_down", LoadStream("Bannerlord.BUTRLoader.Resources.Textures.arrow_down.png"));
            GraphicsContextManager.CreateAndRegister("launcher_arrow_left", LoadStream("Bannerlord.BUTRLoader.Resources.Textures.arrow_left.png"));
            GraphicsContextManager.CreateAndRegister("launcher_export", LoadStream("Bannerlord.BUTRLoader.Resources.Textures.export.png"));
            GraphicsContextManager.CreateAndRegister("launcher_import", LoadStream("Bannerlord.BUTRLoader.Resources.Textures.import.png"));
            GraphicsContextManager.CreateAndRegister("launcher_refresh", LoadStream("Bannerlord.BUTRLoader.Resources.Textures.refresh.png"));
            GraphicsContextManager.CreateAndRegister("launcher_folder", LoadStream("Bannerlord.BUTRLoader.Resources.Textures.folder.png"));
            GraphicsContextManager.CreateAndRegister("launcher_search", LoadStream("Bannerlord.BUTRLoader.Resources.Textures.search.png"));
            GraphicsContextManager.CreateAndRegister("warm_overlay", LoadStream("Bannerlord.BUTRLoader.Resources.Textures.warm_overlay.png"));

            SpriteDataManager.Enable(_launcherHarmony);
            SpriteDataManager.CreateAndRegister("launcher_arrow_down");
            SpriteDataManager.CreateAndRegister("launcher_arrow_left");
            SpriteDataManager.CreateAndRegister("launcher_import");
            SpriteDataManager.CreateAndRegister("launcher_export");
            SpriteDataManager.CreateAndRegister("launcher_refresh");
            SpriteDataManager.CreateAndRegister("launcher_folder");
            SpriteDataManager.CreateAndRegister("launcher_search");
            SpriteDataManager.CreateAndRegister("warm_overlay");

            switch (BUTRLocalizationManager.ActiveLanguage)
            {
                case BUTRLocalizationManager.ChineseTraditional or BUTRLocalizationManager.ChineseSimple:
                {
                    var asset = new AssetPackage(Path.Combine(BasePath.Name, "Modules/Native/AssetPackages/gauntlet_ui.tpac"));
                    if (asset.Items.OfType<Texture>().FirstOrDefault(x => x.Name == "ui_fonts_1") is { } chinese)
                    {
                        GraphicsContextManager.CreateAssetTextureAndRegister("simkai", chinese);
                        SpriteDataManager.CreateGenericAndRegister("simkai");
                    }
                    break;
                }
                case BUTRLocalizationManager.Japanese:
                {
                    var asset = new AssetPackage(Path.Combine(BasePath.Name, "Modules/Native/AssetPackages/gauntlet_ui.tpac"));
                    if (asset.Items.OfType<Texture>().FirstOrDefault(x => x.Name == "ui_fonts_2") is { } japanese)
                    {
                        GraphicsContextManager.CreateAssetTextureAndRegister("SourceHanSansJP", japanese);
                        SpriteDataManager.CreateGenericAndRegister("SourceHanSansJP");
                    }
                    break;
                }
                case BUTRLocalizationManager.Korean:
                {
                    var asset = new AssetPackage(Path.Combine(BasePath.Name, "Modules/Native/AssetPackages/gauntlet_ui.tpac"));
                    if (asset.Items.OfType<Texture>().FirstOrDefault(x => x.Name == "ui_fonts_1") is { } korean)
                    {
                        GraphicsContextManager.CreateAssetTextureAndRegister("NanumGothicKR", korean);
                        SpriteDataManager.CreateGenericAndRegister("NanumGothicKR");
                    }
                    break;
                }
            }


            BrushFactoryManager.Enable(_launcherHarmony);
            BrushFactoryManager.CreateAndRegister(Load("Bannerlord.BUTRLoader.Resources.Brushes.Launcher.xml"));

            WidgetFactoryManager.Enable(_launcherHarmony);
            WidgetFactoryManager.Register(typeof(LauncherToggleButtonWidget));
            WidgetFactoryManager.Register(typeof(LauncherSearchBoxWidget));

            WidgetFactoryManager.CreateAndRegister("Launcher.ToggleButton", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Widgets.Launcher.ToggleButton.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SearchBox", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Widgets.Launcher.SearchBox.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Scrollbar", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Widgets.Launcher.Scrollbar.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Dropdown", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Widgets.Launcher.Dropdown.xml"));

            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyBoolView", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Properties.Launcher.SettingsPropertyBoolView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyButtonView", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Properties.Launcher.SettingsPropertyButtonView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyFloatView", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Properties.Launcher.SettingsPropertyFloatView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyIntView", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Properties.Launcher.SettingsPropertyIntView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyStringView", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Properties.Launcher.SettingsPropertyStringView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyDropdownView", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Properties.Launcher.SettingsPropertyDropdownView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Options", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Options.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Options.OptionTuple", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Options.OptionTuple.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Mods2", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Mods.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Mods.ModuleTuple2", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Mods.ModuleTuple.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Saves", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Saves.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Saves.SaveTuple", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Saves.SaveTuple.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.MessageBox", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.MessageBox.xml"));

            FontFactoryManager.Enable(_launcherHarmony);
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
        private static Stream LoadStream(string embedPath)
        {
            return typeof(Manager).Assembly.GetManifestResourceStream(embedPath) ?? Stream.Null;
        }
        private static byte[] LoadRaw(string embedPath)
        {
            static byte[] ReadFully(Stream input)
            {
                var buffer = new byte[16 * 1024];
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
            GraphicsContextManager.Clear();
            SpriteDataManager.Clear();
            BrushFactoryManager.Clear();
            WidgetFactoryManager.Clear();
            BUTRLocalizationManager.Clear();
            _launcherHarmony.UnpatchAll(_launcherHarmony.Id);
        }
    }
}