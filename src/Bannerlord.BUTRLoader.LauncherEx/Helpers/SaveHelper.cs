using Bannerlord.ModuleManager;

using System;

using TaleWorlds.SaveSystem;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class SaveHelper
    {
        public static string[] GetModules(MetaData metadata)
        {
            if (!metadata.TryGetValue("Modules", out var text))
            {
                return Array.Empty<string>();
            }
            return text.Split(';');
        }
        public static ApplicationVersion GetModuleVersion(MetaData metadata, string moduleName)
        {
            if (metadata.TryGetValue($"Module_{moduleName}", out var versionRaw))
            {
                return ApplicationVersion.TryParse(versionRaw, out var versionVar) ? versionVar : ApplicationVersion.Empty;
            }
            return ApplicationVersion.Empty;
        }

        public static int GetChangeSet(MetaData metadata) =>
            metadata.TryGetValue("ApplicationVersion", out var av) && av?.Split('.') is { Length: 4 } split && int.TryParse(split[3], out var cs) ? cs : -1;
    }
}