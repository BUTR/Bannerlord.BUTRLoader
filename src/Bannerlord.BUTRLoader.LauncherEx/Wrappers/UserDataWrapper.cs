using HarmonyLib.BUTR.Extensions;

using System;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Helpers
{
    internal sealed class UserDataWrapper
    {
        private delegate object GetSingleplayerDataDelegate(object instance);

        private static readonly Type? UserDataType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.UserDatas.UserData");
        private static readonly GetSingleplayerDataDelegate? GetSingleplayerData = AccessTools2.GetPropertyGetterDelegate<GetSingleplayerDataDelegate>(UserDataType!, "SingleplayerData");

        public static UserDataWrapper Create(object? @object) => new(@object);

        public UserGameTypeDataWrapper? SingleplayerData { get => Object is not null? UserGameTypeDataWrapper.Create(GetSingleplayerData?.Invoke(Object)) : null; }

        public object? Object { get; }

        private UserDataWrapper(object? @object)
        {
            Object = @object;
        }
    }
}