using HarmonyLib.BUTR.Extensions;

using System;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Helpers
{
    internal sealed class UserModDataWrapper
    {
        private delegate string GetIdDelegate(object instance);
        private delegate bool GetIsSelectedDelegate(object instance);

        private static readonly Type? UserModDataType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.UserDatas.UserModData");
        private static readonly GetIdDelegate? GetId = AccessTools2.GetPropertyGetterDelegate<GetIdDelegate>(UserModDataType!, "Id");
        private static readonly GetIsSelectedDelegate? GetIsSelected = AccessTools2.GetPropertyGetterDelegate<GetIsSelectedDelegate>(UserModDataType!, "IsSelected");

        public static UserModDataWrapper Create(object? @object) => new(@object);

        public object Id => Object is not null ? GetId?.Invoke(Object) ?? string.Empty : string.Empty;
        public bool IsSelected => Object is not null ? GetIsSelected?.Invoke(Object) ?? false : false;

        public object? Object { get; }

        private UserModDataWrapper(object? @object)
        {
            Object = @object;
        }
    }
}