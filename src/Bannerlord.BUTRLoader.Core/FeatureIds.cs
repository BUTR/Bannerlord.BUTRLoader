﻿using System.Collections.Generic;

namespace Bannerlord.BUTRLoader
{
    public static class FeatureIds
    {
        public static string InterceptorId => "BUTRLoader.BUTRLoadingInterceptor";
        public static string AssemblyResolverId => "BUTRLoader.BUTRAssemblyResolver";
        public static string ContinueSaveFileId => "BUTRLoader.BUTRContinueSaveFile";

        public static readonly HashSet<string> Features = new()
        {
            InterceptorId,
            AssemblyResolverId,
            ContinueSaveFileId,
        };
        public static readonly HashSet<string> LauncherFeatures = new()
        {
            InterceptorId,
            AssemblyResolverId,
        };
    }
}