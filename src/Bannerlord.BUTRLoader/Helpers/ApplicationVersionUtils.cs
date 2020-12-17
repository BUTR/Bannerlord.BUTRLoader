using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    // https://github.com/BUTR/Bannerlord.ButterLib/blob/dev/src/Bannerlord.ButterLib/Helpers/ApplicationVersionUtils.cs
    /// <summary>Helper class for handling the game version.</summary>
    internal static class ApplicationVersionUtils
    {
        public static bool TryParse(string? versionAsString, out ApplicationVersion version)
        {
            var changeSet = 0;
            version = default;
            if (versionAsString is null)
                return false;

            var array = versionAsString.Split('.');
            if (array.Length != 3 && array.Length != 4 && array[0].Length == 0)
                return false;

            var applicationVersionType = ApplicationVersion.ApplicationVersionTypeFromString(array[0][0].ToString());
            if (!int.TryParse(array[0].Substring(1), out var major))
                return false;
            if (!int.TryParse(array[1], out var minor))
                return false;
            if (!int.TryParse(array[2], out var revision))
                return false;
            if (array.Length == 4)
            {
                if (!int.TryParse(array[3], out changeSet))
                    return false;
            }

            version = new ApplicationVersion(applicationVersionType, major, minor, revision, changeSet, ApplicationVersionGameType.Singleplayer);
            return true;
        }

        public static string ToString(ApplicationVersion av)
        {
            string prefix = ApplicationVersion.GetPrefix(av.ApplicationVersionType);
            var def = ApplicationVersion.FromParametersFile(ApplicationVersionGameType.Singleplayer);
            return $"{prefix}{av.Major}.{av.Minor}.{av.Revision}{(av.ChangeSet == def.ChangeSet ? "" : $".{av.ChangeSet}")}";
        }
    }
}