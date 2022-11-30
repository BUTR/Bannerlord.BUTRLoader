using Bannerlord.BUTRLoader.Features.AssemblyResolver;
using Bannerlord.BUTRLoader.Features.Interceptor;

using HarmonyLib;

using System;

namespace Bannerlord.BLSE
{
    public static class Program
    {
        private static readonly Harmony _featureHarmony = new("bannerlord.butrloader.features");

        [STAThread]
        public static void Main(string[] args)
        {
            InterceptorFeature.Enable(_featureHarmony);
            AssemblyResolverFeature.Enable(_featureHarmony);

            TaleWorlds.Starter.Library.Program.Main(args);
        }
    }
}