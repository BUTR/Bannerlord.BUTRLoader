using HarmonyLib.BUTR.Extensions;

namespace Bannerlord.BUTRLoader.Wrappers
{
    internal sealed class UserModDataWrapper
    {
        private delegate string GetIdDelegate(object instance);
        private static readonly GetIdDelegate? GetId =
            AccessTools2.GetPropertyGetterDelegate<GetIdDelegate>("TaleWorlds.MountAndBlade.Launcher.UserDatas.UserModData:Id") ??
            AccessTools2.GetPropertyGetterDelegate<GetIdDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.UserDatas.UserModData:Id");
        
        private delegate bool GetIsSelectedDelegate(object instance);
        private static readonly GetIsSelectedDelegate? GetIsSelected =
            AccessTools2.GetPropertyGetterDelegate<GetIsSelectedDelegate>("TaleWorlds.MountAndBlade.Launcher.UserDatas.UserModData:IsSelected") ??
            AccessTools2.GetPropertyGetterDelegate<GetIsSelectedDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.UserDatas.UserModData:IsSelected");

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