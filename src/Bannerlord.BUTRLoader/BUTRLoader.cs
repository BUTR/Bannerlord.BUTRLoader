using Bannerlord.BUTRLoader.Patches;

using HarmonyLib;

using System;
using System.Diagnostics.CodeAnalysis;

namespace Bannerlord.BUTRLoader
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal sealed class BUTRLoaderAppDomainManager : AppDomainManager
    {
        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            base.InitializeNewDomain(appDomainInfo);

            AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
            {
                // Wait for the Launcher assembly to load
                if (args.LoadedAssembly.GetName().Name == "TaleWorlds.MountAndBlade.Launcher")
                {
                    HarmonyFlow();
                }
            };
        }

        private static bool HarmonyFlow()
        {
            var harmony = new Harmony("Bannerlord.BUTRLoader");

            LauncherModsVMPatch.Enable(harmony);
            LauncherUIPatch.Enable(harmony);

            return true;
        }
    }
}