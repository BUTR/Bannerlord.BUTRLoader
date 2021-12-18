using HarmonyLib.BUTR.Extensions;

using System;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal class LauncherHintVMWrapper
    {
        private delegate object ConstructorV1Delegate(string text);

        private static readonly Type? Type = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.LauncherHintVM");
        private static readonly ConstructorV1Delegate? ConstructorV1 = AccessTools2.GetConstructorDelegate<ConstructorV1Delegate>(Type!, new[] { typeof(string) });

        public static LauncherHintVMWrapper? Create(string text) => ConstructorV1 is null ? null : new(ConstructorV1(text));

        public object Object { get; }

        public LauncherHintVMWrapper(object obj) => Object = obj;
    }
}