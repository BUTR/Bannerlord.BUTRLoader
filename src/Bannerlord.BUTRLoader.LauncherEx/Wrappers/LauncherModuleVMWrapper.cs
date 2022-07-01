using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Wrappers
{
    internal sealed class LauncherModuleVMWrapper
    {
        private static readonly AccessTools.FieldRef<object, object>? GetInfo =
            AccessTools2.FieldRefAccess<object>("TaleWorlds.MountAndBlade.Launcher.LauncherModuleVM:Info") ??
            AccessTools2.FieldRefAccess<object>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM:Info");

        private delegate string GetNameDelegate(object instance);
        private static readonly GetNameDelegate? GetName =
            AccessTools2.GetPropertyGetterDelegate<GetNameDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherModuleVM:Name") ??
            AccessTools2.GetPropertyGetterDelegate<GetNameDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM:Name");

        private delegate bool GetIsSelectedDelegate(object instance);
        private static readonly GetIsSelectedDelegate? GetIsSelected =
            AccessTools2.GetPropertyGetterDelegate<GetIsSelectedDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherModuleVM:IsSelected") ??
            AccessTools2.GetPropertyGetterDelegate<GetIsSelectedDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM:IsSelected");

        private delegate void SetIsSelectedDelegate(object instance, bool value);
        private static readonly SetIsSelectedDelegate? SetIsSelected =
            AccessTools2.GetPropertySetterDelegate<SetIsSelectedDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherModuleVM:IsSelected") ??
            AccessTools2.GetPropertySetterDelegate<SetIsSelectedDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM:IsSelected");

        private delegate bool GetIsDisabledDelegate(object instance);
        private static readonly GetIsDisabledDelegate? GetIsDisabled =
            AccessTools2.GetPropertyGetterDelegate<GetIsDisabledDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherModuleVM:IsDisabled") ??
            AccessTools2.GetPropertyGetterDelegate<GetIsDisabledDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM:IsDisabled");

        private delegate void SetIsDisabledDelegate(object instance, bool value);
        private static readonly SetIsDisabledDelegate? SetIsDisabled =
            AccessTools2.GetPropertySetterDelegate<SetIsDisabledDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherModuleVM:IsDisabled") ??
            AccessTools2.GetPropertySetterDelegate<SetIsDisabledDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM:IsDisabled");

        public static LauncherModuleVMWrapper Create(object? @object) => new(@object);

        public string Name { get => Object is not null ? GetName?.Invoke(Object) ?? "ERROR" : "ERROR"; }

        public bool IsSelected
        {
            get => Object is not null ? GetIsSelected?.Invoke(Object) ?? false : false;
            set { if (Object is not null && SetIsSelected is not null) SetIsSelected(Object, value); }
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