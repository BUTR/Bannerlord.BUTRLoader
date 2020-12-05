using System.Reflection;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class ReflectionHelper
    {
        public static readonly BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    }
}
