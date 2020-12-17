using System.Reflection;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class ReflectionHelper
    {
        public const BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    }
}
