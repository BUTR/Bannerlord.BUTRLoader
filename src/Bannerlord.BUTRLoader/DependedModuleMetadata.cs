using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader
{
    internal readonly struct DependedModuleMetadata
    {
        public readonly string Id;
        public readonly LoadType LoadType;
        public readonly bool IsOptional;
        public readonly ApplicationVersion Version;

        public DependedModuleMetadata(string id, LoadType loadType, bool isOptional, ApplicationVersion version)
        {
            Id = id;
            LoadType = loadType;
            IsOptional = isOptional;
            Version = version;
        }

        internal static string GetLoadType(LoadType loadType) => loadType switch
        {
            LoadType.LoadAfterThis  => "Before ",
            LoadType.LoadBeforeThis => "After  ",
            _                       => "ERROR  "
        };
        private static string GetVersion(ApplicationVersion av) => av.IsSame(ApplicationVersion.Empty) ? "" : $" {av}";
        private static string GetOptional(bool isOptional) => isOptional ? " Optional" : "";
        public override string ToString() => GetLoadType(LoadType) + Id + GetVersion(Version) + GetOptional(IsOptional);
    }
}