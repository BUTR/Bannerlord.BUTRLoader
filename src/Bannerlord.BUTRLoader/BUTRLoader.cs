using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.ModuleInfoExtended;
using Bannerlord.BUTRLoader.Patches;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.Launcher.UserDatas;

namespace Bannerlord.BUTRLoader
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal sealed class BUTRLoaderAppDomainManager : AppDomainManager
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Mount and Blade II Bannerlord",
            "Configs"
        );

        private static readonly string OptionsPath = Path.Combine(
            ConfigPath,
            "ModSettings",
            "BUTRLoader",
            "Options.json"
        );

        public static bool ExtendedSorting
        {
            get => _extendedSorting;
            set => _extendedSorting = value;
        }
        private static bool _extendedSorting = true;

        public static bool AutomaticallyCheckForUpdates
        {
            get => _automaticallyCheckForUpdates;
            set => _automaticallyCheckForUpdates = value;
        }
        private static bool _automaticallyCheckForUpdates;


        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            base.InitializeNewDomain(appDomainInfo);

            if (Directory.Exists(ConfigPath))
            {
                if (File.Exists(OptionsPath))
                {

                }
            }

#if STABLE_DEBUG || BETA_DEBUG
            if (AutomaticallyCheckForUpdates)
                Task.Run(CheckForUpdates);
#endif

            AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
            {
                // Wait for the Launcher assembly to load
                if (args.LoadedAssembly.GetName().Name == "TaleWorlds.MountAndBlade.Launcher")
                {
                    Initialize();
                }
            };
        }

        private static Task CheckForUpdates()
        {
            var userDataManager = new UserDataManager();
            if (userDataManager.HasUserData())
                userDataManager.LoadUserData();

            var moduleInfos = new List<ModuleInfo2>();
            foreach (var modData in userDataManager.UserData.SingleplayerData.ModDatas.DistinctBy(m => m.Id))
            {
                var moduleInfo2 = new ModuleInfo2();
                moduleInfo2.Load(modData.Id);
                moduleInfos.Add(moduleInfo2);
            }

            var newVersions = UpdateChecker.GetAsync(moduleInfos).ConfigureAwait(false).GetAwaiter().GetResult();

            return Task.CompletedTask;
        }

        private static bool Initialize()
        {
            var harmony = new Harmony("Bannerlord.BUTRLoader");

            UserDataManagerPatch.Enable(harmony);
            LauncherModsVMPatch.Enable(harmony);
            LauncherUIPatch.Enable(harmony);
            WidgetPrefabPatch.Enable(harmony);
            WidgetFactoryManager.Enable(harmony);
            WidgetFactoryManager.CreateAndRegister("Launcher.Options", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Options.xml"));

            return true;
        }
        private static XmlDocument Load(string embedPath)
        {
            using var stream = typeof(BUTRLoaderAppDomainManager).Assembly.GetManifestResourceStream(embedPath);
            if (stream is null) throw new Exception($"Could not find embed resource '{embedPath}'!");
            using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true });
            var doc = new XmlDocument();
            doc.Load(xmlReader);
            return doc;
        }
    }
}