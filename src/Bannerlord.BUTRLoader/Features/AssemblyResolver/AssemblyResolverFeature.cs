using Bannerlord.BUTRLoader.Features.AssemblyResolver.Patches;
using Bannerlord.BUTRLoader.Shared;

using HarmonyLib;

namespace Bannerlord.BUTRLoader.Features.AssemblyResolver
{
    internal static class AssemblyResolverFeature
    {
        public static string Id = FeatureIds.AssemblyResolverId;

        public static void Enable(Harmony harmony)
        {
            AssemblyLoaderPatch.Enable(harmony);
        }
    }
}