using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal sealed class LauncherNewsVMWrapper
    {
        private delegate bool GetIsDisabledOnMultiplayerDelegate(object instance);
        private delegate void SetIsDisabledOnMultiplayerDelegate(object instance, bool value);

        private static readonly Type? OldLauncherNewsVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.LauncherNewsVM");
        private static readonly Type? NewLauncherNewsVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.Library.LauncherNewsVM");
        internal static readonly Type? LauncherNewsVMType = OldLauncherNewsVMType ?? NewLauncherNewsVMType;

        private static readonly GetIsDisabledOnMultiplayerDelegate? GetIsDisabledOnMultiplayer = AccessTools2.GetPropertyGetterDelegate<GetIsDisabledOnMultiplayerDelegate>(LauncherNewsVMType!, "IsDisabledOnMultiplayer");
        private static readonly SetIsDisabledOnMultiplayerDelegate? SetIsDisabledOnMultiplayer = AccessTools2.GetPropertySetterDelegate<SetIsDisabledOnMultiplayerDelegate>(LauncherNewsVMType!, "IsDisabledOnMultiplayer");

        public static LauncherNewsVMWrapper Create(object? @object) => new(@object);

        public bool IsDisabledOnMultiplayer
        {
            get => Object is not null ? GetIsDisabledOnMultiplayer?.Invoke(Object) ?? false : false;
            set { if (Object is not null && SetIsDisabledOnMultiplayer is not null) SetIsDisabledOnMultiplayer(Object, value); }
        }

        public ViewModel? Object { get; } = default!;

        private LauncherNewsVMWrapper(object? @object)
        {
            if (@object is ViewModel vm)
                Object = vm;
        }
    }
}