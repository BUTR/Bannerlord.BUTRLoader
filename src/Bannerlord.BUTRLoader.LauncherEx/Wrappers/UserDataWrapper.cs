using HarmonyLib.BUTR.Extensions;

namespace Bannerlord.BUTRLoader.Wrappers
{
    internal sealed class UserDataWrapper
    {
        private delegate object GetSingleplayerDataDelegate(object instance);
        private static readonly GetSingleplayerDataDelegate? GetSingleplayerData =
            AccessTools2.GetPropertyGetterDelegate<GetSingleplayerDataDelegate>("TaleWorlds.MountAndBlade.Launcher.UserDatas.UserData:SingleplayerData") ??
            AccessTools2.GetPropertyGetterDelegate<GetSingleplayerDataDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.UserDatas.UserData:SingleplayerData");

        public static UserDataWrapper Create(object? @object) => new(@object);

        public UserGameTypeDataWrapper? SingleplayerData { get => Object is not null ? UserGameTypeDataWrapper.Create(GetSingleplayerData?.Invoke(Object)) : null; }

        public object? Object { get; }

        private UserDataWrapper(object? @object)
        {
            Object = @object;
        }
    }
}