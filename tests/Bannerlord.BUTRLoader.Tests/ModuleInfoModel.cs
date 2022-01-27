using Bannerlord.ModuleManager;

using System.Collections.Generic;

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

        public List<SubModuleInfoExtended> SubModules { get; init; } = new();

        public List<DependentModule> DependedModules { get; init; } = new();

        public List<DependentModuleMetadata> DependedModuleMetadatas { get; init; } = new();
    }
}