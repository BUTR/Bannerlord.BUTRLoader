using HarmonyLib.BUTR.Extensions;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Wrappers
{
    internal sealed class LauncherNewsVMWrapper
    {
        private delegate bool GetIsDisabledOnMultiplayerDelegate(object instance);
        private static readonly GetIsDisabledOnMultiplayerDelegate? GetIsDisabledOnMultiplayer =
            AccessTools2.GetPropertyGetterDelegate<GetIsDisabledOnMultiplayerDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherNewsVM:IsDisabledOnMultiplayer") ??
            AccessTools2.GetPropertyGetterDelegate<GetIsDisabledOnMultiplayerDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherNewsVM:IsDisabledOnMultiplayer");

        private delegate void SetIsDisabledOnMultiplayerDelegate(object instance, bool value);
        private static readonly SetIsDisabledOnMultiplayerDelegate? SetIsDisabledOnMultiplayer =
            AccessTools2.GetPropertySetterDelegate<SetIsDisabledOnMultiplayerDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherNewsVM:IsDisabledOnMultiplayer") ??
            AccessTools2.GetPropertySetterDelegate<SetIsDisabledOnMultiplayerDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherNewsVM:IsDisabledOnMultiplayer");

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