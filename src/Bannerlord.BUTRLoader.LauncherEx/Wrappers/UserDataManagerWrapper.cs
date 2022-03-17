using HarmonyLib.BUTR.Extensions;

using System;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Helpers
{
    internal sealed class UserDataManagerWrapper
    {
        private delegate object GetUserDataDelegate(object instance);

        private static readonly Type? OldUserDataManagerType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.UserDatas.UserDataManager");
        private static readonly Type? NewUserDataManagerType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.Library.UserDatas.UserDataManager");
        internal static readonly Type? UserDataManagerType = OldUserDataManagerType ?? NewUserDataManagerType;

        private static readonly GetUserDataDelegate? GetUserData = AccessTools2.GetPropertyGetterDelegate<GetUserDataDelegate>(UserDataManagerType!, "UserData");

        public static UserDataManagerWrapper Create(object? @object) => new(@object);

        public UserDataWrapper? UserData { get => Object is not null ? UserDataWrapper.Create(GetUserData?.Invoke(Object)) : null; }

        public object? Object { get; }


        private UserDataManagerWrapper(object? @object)
        {
            Object = @object;
        }
    }
}