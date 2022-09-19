using Bannerlord.BUTRLoader.Features.AssemblyResolver;
using Bannerlord.BUTRLoader.Features.Interceptor;
using Bannerlord.BUTRLoader.LauncherEx;

using HarmonyLib;

namespace Bannerlord.MBSE.Launcher
{
    public static class Program
    {
        private static readonly Harmony _featureHarmony = new("bannerlord.butrloader.features");

        public static void Main(string[] args)
        {
            InterceptorFeature.Enable(_featureHarmony);
            AssemblyResolverFeature.Enable(_featureHarmony);

            Manager.Initialize();
            Manager.Enable();

            TaleWorlds.MountAndBlade.Launcher.Library.Program.Main(args);
        }
    }
}