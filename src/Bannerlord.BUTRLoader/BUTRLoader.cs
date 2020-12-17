using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.ModuleInfoExtended;
using Bannerlord.BUTRLoader.Patches;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.Launcher.UserDatas;

namespace Bannerlord.BUTRLoader
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal sealed class BUTRLoaderAppDomainManager : AppDomainManager
    {
        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            base.InitializeNewDomain(appDomainInfo);

#if STABLE_DEBUG || BETA_DEBUG
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
            ;
        }

        private static bool Initialize()
        {
            var harmony = new Harmony("Bannerlord.BUTRLoader");

            LauncherModsVMPatch.Enable(harmony);
            LauncherUIPatch.Enable(harmony);
            WidgetPrefabPatch.Enable(harmony);

            return true;
        }
    }
}