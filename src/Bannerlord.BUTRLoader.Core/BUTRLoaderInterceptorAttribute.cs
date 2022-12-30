using System;

namespace Bannerlord.BUTRLoader
{
    [Obsolete("Use BLSEInterceptorAttribute")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BUTRLoaderInterceptorAttribute : Attribute { }
}