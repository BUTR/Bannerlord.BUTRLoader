﻿using Bannerlord.BUTRLoader.Features.AssemblyResolver.Patches;

using HarmonyLib;

namespace Bannerlord.BUTRLoader.Features.AssemblyResolver
{
    public static class AssemblyResolverFeature
    {
        public static string Id = FeatureIds.AssemblyResolverId;

        public static void Enable(Harmony harmony)
        {
            AssemblyLoaderPatch.Enable(harmony);
        }
    }
}