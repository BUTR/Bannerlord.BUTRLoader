using HarmonyLib.BUTR.Extensions;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Wrappers
{
    internal sealed class ModuleInfoWrapper
    {
        internal delegate string GetIdDelegate(object instance);
        internal static readonly GetIdDelegate? GetId =
            AccessTools2.GetPropertyGetterDelegate<GetIdDelegate>("TaleWorlds.Library.ModuleInfo:Id") ??
            AccessTools2.GetPropertyGetterDelegate<GetIdDelegate>("TaleWorlds.ModuleManager.ModuleInfo:Id");

        internal delegate string GetAliasDelegate(object instance);
        internal static readonly GetAliasDelegate? GetAlias =
            AccessTools2.GetPropertyGetterDelegate<GetAliasDelegate>("TaleWorlds.Library.ModuleInfo:Alias");

        internal delegate ApplicationVersion GetVersionDelegate(object instance);
        internal static readonly GetVersionDelegate? GetVersion =
            AccessTools2.GetPropertyGetterDelegate<GetVersionDelegate>("TaleWorlds.Library.ModuleInfo:Version") ??
            AccessTools2.GetPropertyGetterDelegate<GetVersionDelegate>("TaleWorlds.ModuleManager.ModuleInfo:Version");

        internal delegate bool GetIsSelectedDelegate(object instance);
        internal static readonly GetIsSelectedDelegate? GetIsSelected =
            AccessTools2.GetPropertyGetterDelegate<GetIsSelectedDelegate>("TaleWorlds.Library.ModuleInfo:IsSelected") ??
            AccessTools2.GetPropertyGetterDelegate<GetIsSelectedDelegate>("TaleWorlds.ModuleManager.ModuleInfo:IsSelected");

        public static ModuleInfoWrapper Create(object? @object) => new(@object);

        public string Id => _id ??= Object is null ? string.Empty : GetId?.Invoke(Object) ?? string.Empty;
        private string? _id;
        public string Alias => _alias ??= Object is null ? string.Empty : GetAlias?.Invoke(Object) ?? string.Empty;
        private string? _alias;
        public bool IsSelected => Object is null || GetIsSelected is null ? false : GetIsSelected(Object);

        public ApplicationVersion Version => _version ??= Object is null ? ApplicationVersion.Empty : GetVersion?.Invoke(Object) ?? ApplicationVersion.Empty;
        private ApplicationVersion? _version;

        public object? Object { get; }

        private ModuleInfoWrapper(object? @object)
        {
            Object = @object;
        }
    }
}