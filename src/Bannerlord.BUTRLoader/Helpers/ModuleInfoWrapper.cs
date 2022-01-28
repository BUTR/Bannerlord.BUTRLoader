using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal sealed class ModuleInfoWrapper
    {
        internal delegate string GetIdDelegate(object instance);
        internal delegate string GetAliasDelegate(object instance);
        internal delegate ApplicationVersion GetVersionDelegate(object instance);
        internal delegate bool GetIsSelectedDelegate(object instance);

        internal static readonly Type? OldModuleInfoType = AccessTools2.TypeByName("TaleWorlds.Library.ModuleInfo");
        internal static readonly Type? NewModuleInfoType = AccessTools2.TypeByName("TaleWorlds.ModuleManager.ModuleInfo");
        internal static readonly Type? ModuleInfoType = OldModuleInfoType ?? NewModuleInfoType;

        internal static readonly Type? ModuleHelperType = AccessTools2.TypeByName("TaleWorlds.ModuleManager.ModuleHelper");

        internal static readonly Type? OldSubModuleInfoType = AccessTools2.TypeByName("TaleWorlds.Library.SubModuleInfo");
        internal static readonly Type? NewSubModuleInfoType = AccessTools2.TypeByName("TaleWorlds.ModuleManager.SubModuleInfo");

        internal static readonly Type? OldDependedModuleType = AccessTools2.TypeByName("TaleWorlds.Library.DependedModule");
        internal static readonly Type? NewDependedModuleType = AccessTools2.TypeByName("TaleWorlds.ModuleManager.DependedModule");

        internal static readonly GetIdDelegate? GetId = AccessTools2.GetPropertyGetterDelegate<GetIdDelegate>(ModuleInfoType!, "Id");
        internal static readonly GetAliasDelegate? GetAlias = AccessTools2.GetPropertyGetterDelegate<GetAliasDelegate>(ModuleInfoType!, "Alias");
        internal static readonly GetVersionDelegate? GetVersion = AccessTools2.GetPropertyGetterDelegate<GetVersionDelegate>(ModuleInfoType!, "Version");
        internal static readonly GetIsSelectedDelegate? GetIsSelected = AccessTools2.GetPropertyGetterDelegate<GetIsSelectedDelegate>(ModuleInfoType!, "IsSelected");

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