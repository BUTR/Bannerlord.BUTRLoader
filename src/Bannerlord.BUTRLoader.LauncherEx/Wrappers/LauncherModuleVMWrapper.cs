using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal sealed class LauncherModuleVMWrapper
    {
        private delegate string GetNameDelegate(object instance);
        private delegate void SetIsSelectedDelegate(object instance, bool value);
        private delegate bool GetIsSelectedDelegate(object instance);
        private delegate bool GetIsDisabledDelegate(object instance);
        private delegate void SetIsDisabledDelegate(object instance, bool value);

        public static readonly Type? LauncherModuleVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.LauncherModuleVM") ??
                                                            AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM");

        private static readonly AccessTools.FieldRef<object, object>? GetInfo = AccessTools2.FieldRefAccess<object>(LauncherModuleVMType!, "Info");

        private static readonly GetNameDelegate? GetName = AccessTools2.GetPropertyGetterDelegate<GetNameDelegate>(LauncherModuleVMType!, "Name");

        private static readonly GetIsSelectedDelegate? GetIsSelected = AccessTools2.GetPropertyGetterDelegate<GetIsSelectedDelegate>(LauncherModuleVMType!, "IsSelected");
        private static readonly SetIsSelectedDelegate? SetIsSelected = AccessTools2.GetPropertySetterDelegate<SetIsSelectedDelegate>(LauncherModuleVMType!, "IsSelected");

        private static readonly GetIsDisabledDelegate? GetIsDisabled = AccessTools2.GetPropertyGetterDelegate<GetIsDisabledDelegate>(LauncherModuleVMType!, "IsDisabled");
        private static readonly SetIsDisabledDelegate? SetIsDisabled = AccessTools2.GetPropertySetterDelegate<SetIsDisabledDelegate>(LauncherModuleVMType!, "IsDisabled");

        public static LauncherModuleVMWrapper Create(object? @object) => new(@object);

        public string Name { get => Object is not null ? GetName?.Invoke(Object) ?? "ERROR" : "ERROR"; }

        public bool IsSelected
        {
            get => Object is not null ? GetIsSelected?.Invoke(Object) ?? false : false; 
            set { if (Object is not null && SetIsSelected is not null)  SetIsSelected(Object, value); }
        }

        public bool IsDisabled
        {
            get => Object is not null ? GetIsDisabled?.Invoke(Object) ?? false : false; 
            set { if (Object is not null && SetIsDisabled is not null) SetIsDisabled(Object, value); }
        }

        public ModuleInfoWrapper? Info { get => Object is not null ? ModuleInfoWrapper.Create(GetInfo?.Invoke(Object)) : null; }

        public ViewModel? Object { get; } = default!;

        private LauncherModuleVMWrapper(object? @object)
        {
            if (@object is ViewModel vm)
                Object = vm;
        }

        public void ExecuteCommand(string commandName, object[] parameters)
        {
            Object?.ExecuteCommand(commandName, parameters);
        }
    }
}