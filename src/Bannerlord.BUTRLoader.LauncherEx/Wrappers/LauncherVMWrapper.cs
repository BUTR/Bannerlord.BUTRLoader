using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal sealed class LauncherVMWrapper
    {
        private delegate bool GetIsSingleplayerDelegate(object instance);
        private delegate void SetIsSingleplayerDelegate(object instance, bool value);
        private delegate bool GetIsMultiplayerDelegate(object instance);
        private delegate void SetIsMultiplayerDelegate(object instance, bool value);
        private delegate object GetModsDataDelegate(object instance);
        private delegate object GetNewsDelegate(object instance);

        private static readonly Type? OldLauncherVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.LauncherVM");
        private static readonly Type? NewLauncherVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.Library.LauncherVM");
        internal static readonly Type? LauncherVMType = OldLauncherVMType ?? NewLauncherVMType;

        private static readonly GetIsSingleplayerDelegate? GetIsSingleplayer = AccessTools2.GetPropertyGetterDelegate<GetIsSingleplayerDelegate>(LauncherVMType!, "IsSingleplayer");
        private static readonly SetIsSingleplayerDelegate? SetIsSingleplayer = AccessTools2.GetPropertySetterDelegate<SetIsSingleplayerDelegate>(LauncherVMType!, "IsSingleplayer");

        private static readonly GetIsMultiplayerDelegate? GetIsMultiplayer = AccessTools2.GetPropertyGetterDelegate<GetIsMultiplayerDelegate>(LauncherVMType!, "IsMultiplayer");
        private static readonly SetIsMultiplayerDelegate? SetIsMultiplayer = AccessTools2.GetPropertySetterDelegate<SetIsMultiplayerDelegate>(LauncherVMType!, "IsMultiplayer");

        private static readonly GetModsDataDelegate? GetModsData = AccessTools2.GetPropertyGetterDelegate<GetModsDataDelegate>(LauncherVMType!, "ModsData");

        private static readonly GetNewsDelegate? GetNews = AccessTools2.GetPropertyGetterDelegate<GetNewsDelegate>(LauncherVMType!, "News");

        public static LauncherVMWrapper Create(object @object) => new(@object);

        public bool IsSingleplayer { get => GetIsSingleplayer?.Invoke(Object) ?? false; set => SetIsSingleplayer?.Invoke(Object, value); }
        public bool IsMultiplayer { get => GetIsMultiplayer?.Invoke(Object) ?? false; set => SetIsMultiplayer?.Invoke(Object, value); }
        public LauncherModsVMWrapper ModsData { get => LauncherModsVMWrapper.Create(GetModsData?.Invoke(Object)); }
        public LauncherNewsVMWrapper News { get => LauncherNewsVMWrapper.Create(GetNews?.Invoke(Object)); }

        public ViewModel Object { get; } = default!;

        private LauncherVMWrapper(object @object)
        {
            if (@object is ViewModel vm)
                Object = vm;
        }
    }
}