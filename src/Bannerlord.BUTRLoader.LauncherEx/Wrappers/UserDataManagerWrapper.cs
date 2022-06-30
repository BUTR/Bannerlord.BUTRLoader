using HarmonyLib.BUTR.Extensions;

namespace Bannerlord.BUTRLoader.Wrappers
{
    internal sealed class UserDataManagerWrapper
    {
        private delegate object GetUserDataDelegate(object instance);
        private static readonly GetUserDataDelegate? GetUserData =
            AccessTools2.GetPropertyGetterDelegate<GetUserDataDelegate>("TaleWorlds.MountAndBlade.Launcher.UserDatas.UserDataManager:UserData") ??
            AccessTools2.GetPropertyGetterDelegate<GetUserDataDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.UserDatas.UserDataManager:UserData");

        public static UserDataManagerWrapper Create(object? @object) => new(@object);

        public UserDataWrapper? UserData { get => Object is not null ? UserDataWrapper.Create(GetUserData?.Invoke(Object)) : null; }

        public object? Object { get; }


        private UserDataManagerWrapper(object? @object)
        {
            Object = @object;
        }
    }
}