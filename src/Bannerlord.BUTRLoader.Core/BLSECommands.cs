using System.Collections.Generic;

namespace Bannerlord.BUTRLoader
{
    public static class BLSECommands
    {
        public static string GetVersion(List<string> _) => typeof(BLSECommands).Assembly.GetName().Version.ToString();
    }
}