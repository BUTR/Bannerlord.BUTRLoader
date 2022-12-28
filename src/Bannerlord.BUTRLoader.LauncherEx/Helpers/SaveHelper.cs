using Bannerlord.BUTR.Shared.Helpers;

using System;
using System.IO;

using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

using ApplicationVersion = Bannerlord.ModuleManager.ApplicationVersion;

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

        public static string? GetSaveFilePath(SaveGameFileInfo saveGameFileInfo)
        {
            var savesDirectory = new PlatformDirectoryPath(PlatformFileType.User, "Game Saves\\");
            if (PlatformFileHelperPCExtended.GetDirectoryFullPath(savesDirectory) is not { } savesDirectoryPath) return null;
            return Path.Combine(savesDirectoryPath, $"{saveGameFileInfo.Name}.sav");
        }
    }
}