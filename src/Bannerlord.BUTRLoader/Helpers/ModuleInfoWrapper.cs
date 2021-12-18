using HarmonyLib.BUTR.Extensions;

using System;

namespace Bannerlord.BUTRLoader.Helpers;

internal sealed class ModuleInfoWrapper
{
    private delegate string GetIdDelegate(object instance);
    private delegate string GetAliasDelegate(object instance);
    private delegate bool GetIsSelectedDelegate(object instance);

    private static readonly Type? OldModuleInfoType = AccessTools2.TypeByName("TaleWorlds.Library.ModuleInfo");
    private static readonly Type? NewModuleInfoType = AccessTools2.TypeByName("TaleWorlds.ModuleManager.ModuleInfo");
    public static readonly Type? ModuleInfoType = OldModuleInfoType ?? NewModuleInfoType;

    private static readonly GetIdDelegate? GetId = AccessTools2.GetPropertyGetterDelegate<GetIdDelegate>(ModuleInfoType!, "Id");
    private static readonly GetAliasDelegate? GetAlias = AccessTools2.GetPropertyGetterDelegate<GetAliasDelegate>(ModuleInfoType!, "Alias");
    private static readonly GetIsSelectedDelegate? GetIsSelected = AccessTools2.GetPropertyGetterDelegate<GetIsSelectedDelegate>(ModuleInfoType!, "IsSelected");

    public static ModuleInfoWrapper Create(object? @object) => new(@object);

    public string Id => _id ??= Object is null ? string.Empty : GetId?.Invoke(Object) ?? string.Empty;
    private string? _id;
    public string Alias => _alias ??= Object is null ? string.Empty : GetAlias?.Invoke(Object) ?? string.Empty;
    private string? _alias;
    public bool IsSelected => Object is null ? false : GetIsSelected?.Invoke(Object) ?? false;

    public object? Object { get; }

    private ModuleInfoWrapper(object? @object)
    {
        Object = @object;
    }
}