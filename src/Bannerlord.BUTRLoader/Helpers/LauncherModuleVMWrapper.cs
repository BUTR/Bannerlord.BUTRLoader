using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

namespace Bannerlord.BUTRLoader.Helpers;

internal sealed class LauncherModuleVMWrapper
{
    private static readonly Type? LauncherModuleVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.LauncherModuleVM");
    private static readonly AccessTools.FieldRef<object, object>? GetInfo = AccessTools2.FieldRefAccess<object>(LauncherModuleVMType!, "Info");

    public static LauncherModuleVMWrapper Create(object @object) => new(@object);

    public ModuleInfoWrapper Info => _info ??= ModuleInfoWrapper.Create(GetInfo?.Invoke(Object));
    private ModuleInfoWrapper? _info;

    public object Object { get; }

    private LauncherModuleVMWrapper(object @object)
    {
        Object = @object;
    }
}