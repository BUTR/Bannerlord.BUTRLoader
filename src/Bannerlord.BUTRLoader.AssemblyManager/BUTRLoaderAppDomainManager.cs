using Bannerlord.BUTRLoader.Features.AssemblyResolver;
using Bannerlord.BUTRLoader.Features.Interceptor;
using Bannerlord.BUTRLoader.LauncherEx;

using HarmonyLib;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Bannerlord.BUTRLoader.Tests")]

namespace Bannerlord.BUTRLoader.AssemblyManager
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal sealed class BUTRLoaderAppDomainManager : AppDomainManager
    {
        private static readonly Harmony _featureHarmony = new("bannerlord.butrloader.features");

        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            base.InitializeNewDomain(appDomainInfo);

            // delete old files
            var files = new[]
            {
                "Bannerlord.BUTRLoader.LauncherEx.dll",
                "Bannerlord.BUTRLoader.LauncherEx.pdb",
                "Bannerlord.BUTRLoader.Shared.dll",
                "Bannerlord.BUTRLoader.Shared.pdb",
            };
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception) { }
            }

            Initialize();

            AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
            {
                // Pre e1.7.2
                // Wait for the Launcher assembly to load
                if (args.LoadedAssembly.GetName().Name == "TaleWorlds.MountAndBlade.Launcher")
                {
                    if (args.LoadedAssembly.GetType("TaleWorlds.MountAndBlade.Launcher.LauncherVM") is null) return;

                    Manager.Enable();
                }

                // Post e1.7.2
                // Wait for the Launcher assembly to load
                if (args.LoadedAssembly.GetName().Name == "TaleWorlds.MountAndBlade.Launcher.Library")
                {
                    Manager.Enable();
                }
            };
        }


        private static void Initialize()
        {
            Manager.Initialize();

            InterceptorFeature.Enable(_featureHarmony);
            AssemblyResolverFeature.Enable(_featureHarmony);
        }
    }
}