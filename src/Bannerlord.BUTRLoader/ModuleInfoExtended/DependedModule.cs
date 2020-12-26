using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.ModuleInfoExtended
{
    /// <summary>
    /// https://github.com/BUTR/Bannerlord.ButterLib/blob/dev/src/Bannerlord.ButterLib/Helpers/ModuleInfo/DependedModule.cs
    /// </summary>
    internal readonly struct DependedModule
    {
        public string ModuleId { get; }
        public ApplicationVersion Version { get; }

        public DependedModule(string moduleId, ApplicationVersion version)
        {
            ModuleId = moduleId;
            Version = version;
        }
    }
}