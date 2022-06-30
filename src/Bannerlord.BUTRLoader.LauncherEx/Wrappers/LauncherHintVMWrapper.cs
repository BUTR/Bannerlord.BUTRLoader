using HarmonyLib.BUTR.Extensions;

namespace Bannerlord.BUTRLoader.Wrappers
{
    internal sealed class LauncherHintVMWrapper
    {
        private delegate object ConstructorV1Delegate(string text);
        private static readonly ConstructorV1Delegate? ConstructorV1 =
            AccessTools2.GetConstructorDelegate<ConstructorV1Delegate>("TaleWorlds.MountAndBlade.Launcher.LauncherHintVM", new[] { typeof(string) }) ??
            AccessTools2.GetConstructorDelegate<ConstructorV1Delegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherHintVM", new[] { typeof(string) });

        public static LauncherHintVMWrapper? Create(string text) => ConstructorV1 is null ? null : new(ConstructorV1(text));

        public object Object { get; }

        public LauncherHintVMWrapper(object obj) => Object = obj;
    }
}