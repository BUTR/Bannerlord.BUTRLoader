using Bannerlord.BUTRLoader.WithHarmony;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

using TaleWorlds.Library;

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
                    var modulesDir = Path.Combine(BasePath.Name, "Modules");
                    var harmonyDir = Path.Combine(modulesDir, "Bannerlord.Harmony");

                    if (Directory.Exists(harmonyDir))
                    {
                        var harmonyAssemblyPath = Path.GetFullPath(Path.Combine(harmonyDir, "bin", Common.ConfigName, "0Harmony.dll"));
                        if (File.Exists(harmonyAssemblyPath))
                        {
                            Assembly.LoadFile(harmonyAssemblyPath);
                            HarmonyFlow();
                        }
                        else
                        {
                            NoHarmonyFlow();
                        }
                    }
                    else
                    {
                        NoHarmonyFlow();
                    }
                }
            };
        }

        // We don't directly depend on Harmony to avoid runtime linking issues
        private static bool HarmonyFlow()
        {
            var harmony = new Harmony("Bannerlord.BUTRLoader");

            LauncherModsVMPatch.Enable(harmony);
            LauncherUIPatch.Enable(harmony);

            return true;
        }

        private static bool NoHarmonyFlow()
        {
            return true;
        }
    }
}