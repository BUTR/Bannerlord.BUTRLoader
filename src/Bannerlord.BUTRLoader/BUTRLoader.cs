using Bannerlord.BUTR.Shared.ModuleInfoExtended;
using Bannerlord.BUTRLoader.Features.Interceptor;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Patches;
using Bannerlord.BUTRLoader.ResourceManagers;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;

using TaleWorlds.Core;
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

        public static bool CompactModuleList
        {
            get => _compactModuleList;
            set => _compactModuleList = value;
        }
        private static bool _compactModuleList;

        public static bool ResetModuleList
        {
            get => _resetModuleList;
            set => _resetModuleList = value;
        }
        private static bool _resetModuleList;

        private static readonly Harmony _launcherHarmony = new("bannerlord.butrloader.launcher");
        private static readonly Harmony _featureHarmony = new("bannerlord.butrloader.features");


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
            ProgramPatch.Enable(_launcherHarmony);
            UserDataManagerPatch.Enable(_launcherHarmony);
            LauncherVMPatch.Enable(_launcherHarmony);
            LauncherModuleVMPatch.Enable(_launcherHarmony);
            LauncherModsVMPatch.Enable(_launcherHarmony);
            LauncherUIPatch.Enable(_launcherHarmony);
            ViewModelPatch.Enable(_launcherHarmony);
            WidgetPrefabPatch.Enable(_launcherHarmony);

            GraphicsContextManager.Enable(_launcherHarmony);
            GraphicsContextManager.CreateAndRegister("arrow_down", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.arrow_down.png"));
            GraphicsContextManager.CreateAndRegister("arrow_left", LoadRaw("Bannerlord.BUTRLoader.Resources.Textures.arrow_left.png"));

            SpriteDataManager.Enable(_launcherHarmony);
            SpriteDataManager.CreateAndRegister("arrow_down");
            SpriteDataManager.CreateAndRegister("arrow_left");

            BrushFactoryManager.Enable(_launcherHarmony);
            BrushFactoryManager.CreateAndRegister(Load("Bannerlord.BUTRLoader.Resources.Brushes.Launcher.xml"));

            WidgetFactoryManager.Enable(_launcherHarmony);
            WidgetFactoryManager.CreateAndRegister("Launcher.Options", Load("Bannerlord.BUTRLoader.Resources.Prefabs.Launcher.Options.xml"));


            InterceptorFeature.Enable(_featureHarmony);

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

            using var stream = typeof(BUTRLoaderAppDomainManager).Assembly.GetManifestResourceStream(embedPath);
            if (stream is null) throw new Exception($"Could not find embed resource '{embedPath}'!");
            return ReadFully(stream);
        }

        public static void UnpatchAll()
        {
            _launcherHarmony.UnpatchAll(_launcherHarmony.Id);
        }
    }
}