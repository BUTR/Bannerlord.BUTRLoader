using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal sealed class LauncherModsVMWrapper
    {
        private delegate string GetIdDelegate(object instance);
        private delegate IList GetModulesDelegate(object instance);
        private delegate MBBindingList<LauncherModuleVMWrapper> GetModulesDelegate2(object instance);
        private delegate bool GetIsDisabledOnMultiplayerDelegate(object instance);
        private delegate void SetIsDisabledOnMultiplayerDelegate(object instance, bool value);

        private static readonly Type? OldLauncherModsVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.LauncherModsVM");
        private static readonly Type? NewLauncherModsVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModsVM");
        internal static readonly Type? LauncherModsVMType = OldLauncherModsVMType ?? NewLauncherModsVMType;

        private static readonly GetIdDelegate? GetId = AccessTools2.GetPropertyGetterDelegate<GetIdDelegate>(LauncherModsVMType!, "Id");
        private static readonly GetModulesDelegate? GetModules = AccessTools2.GetPropertyGetterDelegate<GetModulesDelegate>(LauncherModsVMType!, "Modules");

        private static readonly GetIsDisabledOnMultiplayerDelegate? GetIsDisabledOnMultiplayer = AccessTools2.GetPropertyGetterDelegate<GetIsDisabledOnMultiplayerDelegate>(LauncherModsVMType!, "IsDisabledOnMultiplayer");
        private static readonly SetIsDisabledOnMultiplayerDelegate? SetIsDisabledOnMultiplayer = AccessTools2.GetPropertySetterDelegate<SetIsDisabledOnMultiplayerDelegate>(LauncherModsVMType!, "IsDisabledOnMultiplayer");

        public static LauncherModsVMWrapper Create(object? @object) => new(@object);

        public string Id => Object is not null ? GetId?.Invoke(Object) ?? string.Empty : string.Empty;

        public bool IsDisabledOnMultiplayer
        {
            get => Object is not null ? GetIsDisabledOnMultiplayer?.Invoke(Object) ?? false : false;
            set { if (Object is not null && SetIsDisabledOnMultiplayer is not null) SetIsDisabledOnMultiplayer?.Invoke(Object, value); }
        }

        public IList ModulesRaw => Object is not null ? GetModules?.Invoke(Object) ?? Array.Empty<object>() : Array.Empty<object>();
        public IEnumerable<LauncherModuleVMWrapper> Modules
        {
            get
            {
                foreach (var raw in ModulesRaw)
                {
                    yield return LauncherModuleVMWrapper.Create(raw);
                }
            }
        }

        public ViewModel? Object { get; } = default!;

        private LauncherModsVMWrapper(object? @object)
        {
            if (@object is ViewModel vm)
                Object = vm;
        }
    }
}