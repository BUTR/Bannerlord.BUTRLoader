using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Wrappers
{
    internal sealed class LauncherModsVMWrapper
    {
        private delegate IList GetModulesDelegate(object instance);
        private static readonly GetModulesDelegate? GetModules =
            AccessTools2.GetPropertyGetterDelegate<GetModulesDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherModsVM:Modules") ??
            AccessTools2.GetPropertyGetterDelegate<GetModulesDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModsVM:Modules");

        private delegate bool GetIsDisabledOnMultiplayerDelegate(object instance);
        private static readonly GetIsDisabledOnMultiplayerDelegate? GetIsDisabledOnMultiplayer =
            AccessTools2.GetPropertyGetterDelegate<GetIsDisabledOnMultiplayerDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherModsVM:IsDisabledOnMultiplayer") ??
            AccessTools2.GetPropertyGetterDelegate<GetIsDisabledOnMultiplayerDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModsVM:IsDisabledOnMultiplayer");
        
        private delegate void SetIsDisabledOnMultiplayerDelegate(object instance, bool value);
        private static readonly SetIsDisabledOnMultiplayerDelegate? SetIsDisabledOnMultiplayer =
            AccessTools2.GetPropertySetterDelegate<SetIsDisabledOnMultiplayerDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherModsVM:IsDisabledOnMultiplayer") ??
            AccessTools2.GetPropertySetterDelegate<SetIsDisabledOnMultiplayerDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModsVM:IsDisabledOnMultiplayer");

        public static LauncherModsVMWrapper Create(object? @object) => new(@object);

        public bool IsDisabledOnMultiplayer
        {
            get => Object is not null ? GetIsDisabledOnMultiplayer?.Invoke(Object) ?? false : false;
            set { if (Object is not null && SetIsDisabledOnMultiplayer is not null) SetIsDisabledOnMultiplayer(Object, value); }
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