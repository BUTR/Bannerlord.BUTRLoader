using Bannerlord.BUTRLoader.ModuleInfoExtended;

using System.Collections.Generic;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Tests
{
    internal record ModuleInfoModel
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public bool IsOfficial { get; init; }

        public ApplicationVersion Version { get; init; } = ApplicationVersion.Empty;

        public string Alias { get; init; } = string.Empty;

        public bool IsSingleplayerModule { get; init; }

        public bool IsMultiplayerModule { get; init; }

        public bool IsSelected { get; init; }

        public List<SubModuleInfo> SubModules { get; init; } = new();

        public List<ModuleInfoExtended.DependedModule> DependedModules { get; init; } = new();

        public List<DependedModuleMetadata> DependedModuleMetadatas { get; init; }  = new();
    }
}