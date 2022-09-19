using System.Collections.Generic;

namespace Bannerlord.BUTRLoader
{
    public static class FeatureIds
    {
        public static string InterceptorId => "BUTRLoader.BUTRLoadingInterceptor";
        public static string AssemblyResolverId => "BUTRLoader.BUTRAssemblyResolver";

        public static HashSet<string> Features = new()
        {
            InterceptorId,
            AssemblyResolverId
        };
    }
}