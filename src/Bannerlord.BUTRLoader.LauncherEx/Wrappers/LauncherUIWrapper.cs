using HarmonyLib.BUTR.Extensions;

using System;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal sealed class LauncherUIWrapper
    {
        private static readonly Type? OldLauncherUIType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.LauncherUI");
        private static readonly Type? NewLauncherUIType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.Library.LauncherUI");
        internal static readonly Type? LauncherUIType = OldLauncherUIType ?? NewLauncherUIType;

        public static LauncherUIWrapper Create(object @object) => new(@object);

        public object Object { get; }

        private LauncherUIWrapper(object @object)
        {
            Object = @object;
        }
    }
}