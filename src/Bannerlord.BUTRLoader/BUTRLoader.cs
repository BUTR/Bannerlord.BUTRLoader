using Bannerlord.BUTR.Shared.ModuleInfoExtended;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Patches;
using Bannerlord.BUTRLoader.Patches.Widgets;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;

using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.MountAndBlade.Launcher.UserDatas;

[assembly: InternalsVisibleTo("Bannerlord.BUTRLoader.Tests")]

namespace Bannerlord.BUTRLoader
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal sealed class BUTRLoaderAppDomainManager : AppDomainManager
    {
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

        public static bool UnblockFiles
        {
            get => _unblockFiles;
            set => _unblockFiles = value;
        }
        private static bool _unblockFiles;

        public static bool FixCommonIssues
        {
            get => _fixCommonIssues;
            set => _fixCommonIssues = value;
        }
        private static bool _fixCommonIssues;


        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            base.InitializeNewDomain(appDomainInfo);

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

            ProgramPatch.Enable(harmony);
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