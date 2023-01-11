using Bannerlord.BUTRLoader.Features.AssemblyResolver;
using Bannerlord.BUTRLoader.Features.Commands;
using Bannerlord.BUTRLoader.Features.ContinueSaveFile;
using Bannerlord.BUTRLoader.Features.Interceptor;
using Bannerlord.BUTRLoader.LauncherEx;

using HarmonyLib;

using System;
using System.Runtime.InteropServices;

namespace Bannerlord.BLSE.Launcher
{
    public static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static readonly Harmony _featureHarmony = new("bannerlord.butrloader.features");

        public static void Main(string[] args)
        {
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            InterceptorFeature.Enable(_featureHarmony);
            AssemblyResolverFeature.Enable(_featureHarmony);
            ContinueSaveFileFeature.Enable(_featureHarmony);
            CommandsFeature.Enable(_featureHarmony);

            Manager.Initialize();
            Manager.Enable();

            TaleWorlds.MountAndBlade.Launcher.Library.Program.Main(args);
        }
    }
}