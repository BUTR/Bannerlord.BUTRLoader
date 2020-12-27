using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.ModuleInfoExtended
{
    /// <summary>
    /// https://github.com/BUTR/Bannerlord.ButterLib/blob/dev/src/Bannerlord.ButterLib/Helpers/ModuleInfo/DependedModuleMetadata.cs
    /// </summary>
    internal readonly struct DependedModuleMetadata
    {
        public readonly string Id;
        public readonly LoadType LoadType;
        public readonly bool IsOptional;
        public readonly bool IsIncompatible;
        public readonly ApplicationVersion Version;

        public DependedModuleMetadata(string id, LoadType loadType, bool isOptional, bool isIncompatible, ApplicationVersion version)
        {
            Id = id;
            LoadType = loadType;
            IsOptional = isOptional;
            IsIncompatible = isIncompatible;
            Version = version;
        }

        internal static string GetLoadType(LoadType loadType) => loadType switch
        {
            LoadType.NONE           => "",
            LoadType.LoadAfterThis  => "Before       ",
            LoadType.LoadBeforeThis => "After        ",
            _                       => "ERROR        "
        };
        private static string GetVersion(ApplicationVersion av) => av.IsSame(ApplicationVersion.Empty) ? "" : $" {av}";
        private static string GetOptional(bool isOptional) => isOptional ? " Optional" : "";
        private static string GetIncompatible(bool isOptional) => isOptional ? "Incompatible " : "";
        public override string ToString() => GetLoadType(LoadType) + GetIncompatible(IsIncompatible) + Id + GetVersion(Version) + GetOptional(IsOptional);
    }
}