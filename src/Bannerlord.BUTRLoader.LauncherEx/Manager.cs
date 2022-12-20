using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Patches;
using Bannerlord.BUTRLoader.ResourceManagers;

using HarmonyLib;

using System;
using System.IO;
using System.Xml;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.LauncherEx
{
    public static class Manager
    {
        internal static readonly Harmony _launcherHarmony = new("bannerlord.butrloader.launcherex");
        internal static readonly AssemblyCompatibilityChecker _compatibilityChecker = new();

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
            GraphicsContextManager.CreateAndRegister("arrow_down", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.arrow_down.png"));
            GraphicsContextManager.CreateAndRegister("arrow_left", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.arrow_left.png"));
            GraphicsContextManager.CreateAndRegister("export", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.export.png"));
            GraphicsContextManager.CreateAndRegister("import", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.import.png"));
            GraphicsContextManager.CreateAndRegister("refresh", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.refresh.png"));

            SpriteDataManager.Enable(_launcherHarmony);
            SpriteDataManager.CreateAndRegister("arrow_down");
            SpriteDataManager.CreateAndRegister("arrow_left");
            SpriteDataManager.CreateAndRegister("import");
            SpriteDataManager.CreateAndRegister("export");
            SpriteDataManager.CreateAndRegister("refresh");

            BrushFactoryManager.Enable(_launcherHarmony);
            BrushFactoryManager.CreateAndRegister(Load("Bannerlord.BUTRLoader.Resources.Brushes.Launcher.xml"));

            WidgetFactoryManager.Enable(_launcherHarmony);
            WidgetFactoryManager.CreateAndRegister("Launcher.Options", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Options.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Mods2", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Mods.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Mods.ModuleTuple2", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Mods.ModuleTuple.xml"));
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
            _compatibilityChecker.Dispose();
            _launcherHarmony.UnpatchAll(_launcherHarmony.Id);
        }
    }
}