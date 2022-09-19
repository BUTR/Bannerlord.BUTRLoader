using Bannerlord.BUTRLoader.Features.AssemblyResolver;
using Bannerlord.BUTRLoader.Features.Interceptor;

using HarmonyLib;

namespace Bannerlord.MBSE
{
    public static class Program
    {
        private static readonly Harmony _featureHarmony = new("bannerlord.butrloader.features");

        public static void Main(string[] args)
        {
            InterceptorFeature.Enable(_featureHarmony);
            AssemblyResolverFeature.Enable(_featureHarmony);

            TaleWorlds.Starter.Library.Program.Main(args);
        }
    }
}