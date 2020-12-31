using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.ModuleInfoExtended
{
    /// <summary>
    /// https://github.com/BUTR/Bannerlord.ButterLib/blob/dev/src/Bannerlord.ButterLib/Helpers/ModuleInfo/DependedModule.cs
    /// </summary>
    internal readonly struct DependedModule
    {
        public string ModuleId { get; init; }
        public ApplicationVersion Version { get; init; }
    }
}