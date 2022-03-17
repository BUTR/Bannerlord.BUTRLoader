using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Helpers
{
    internal sealed class UserGameTypeDataWrapper
    {
        private delegate IList GetModDatasDelegate(object instance);

        private static readonly Type? UserGameTypeDataType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.UserDatas.UserGameTypeData");
        private static readonly GetModDatasDelegate? GetModDatas = AccessTools2.GetPropertyGetterDelegate<GetModDatasDelegate>(UserGameTypeDataType!, "ModDatas");

        public static UserGameTypeDataWrapper Create(object? @object) => new(@object);

        public IList ModDatasRaw => Object is not null ? GetModDatas?.Invoke(Object) ?? Array.Empty<object>() : Array.Empty<object>();
        public IEnumerable<UserModDataWrapper> ModDatas
        {
            get
            {
                foreach (var raw in ModDatasRaw)
                {
                    yield return UserModDataWrapper.Create(raw);
                }
            }
        }

        public object? Object { get; }

        private UserGameTypeDataWrapper(object? @object)
        {
            Object = @object;
        }
    }
}