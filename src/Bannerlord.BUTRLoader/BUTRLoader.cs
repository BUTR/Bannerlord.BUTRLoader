using Bannerlord.BUTRLoader.Features.Interceptor;
using Bannerlord.BUTRLoader.Features.Interceptor.Patches;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Bannerlord.BUTRLoader.Tests")]

namespace Bannerlord.BUTRLoader
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal sealed class BUTRLoaderAppDomainManager : AppDomainManager
    {
        private static readonly Harmony _featureHarmony = new("bannerlord.butrloader.features");

        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            base.InitializeNewDomain(appDomainInfo);

            Initialize();

            if (appDomainInfo.ApplicationName == "TaleWorlds.MountAndBlade.Launcher.exe")
            {
                Assembly.LoadFrom("Bannerlord.BUTRLoader.LauncherEx.dll");
            }
            AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
            {
                // Pre e1.7.2
                // Wait for the Launcher assembly to load
                if (args.LoadedAssembly.GetName().Name == "TaleWorlds.MountAndBlade.Launcher")
                {
                    if (args.LoadedAssembly.GetType("TaleWorlds.MountAndBlade.Launcher.LauncherVM") is null)
                    {
                        return;
                    }

                    var init = AccessTools2.Method("Bannerlord.BUTRLoader.LauncherEx.Manager:Enable");
                    init?.Invoke(null, Array.Empty<object>());
                }

                // Post e1.7.2
                // Wait for the Launcher assembly to load
                if (args.LoadedAssembly.GetName().Name == "TaleWorlds.MountAndBlade.Launcher.Library")
                {
                    var init = AccessTools2.Method("Bannerlord.BUTRLoader.LauncherEx.Manager:Enable");
                    init?.Invoke(null, Array.Empty<object>());
                }
            };
        }


        private static bool Initialize()
        {
            InterceptorFeature.Enable(_featureHarmony);
            AssemblyLoaderPatch.Enable(_featureHarmony);

            return true;
        }
    }
}