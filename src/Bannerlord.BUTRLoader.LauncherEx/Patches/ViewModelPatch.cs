﻿using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class ViewModelPatch
    {
        public static void Enable(Harmony harmony)
        {
            harmony.TryPatch(
                AccessTools2.DeclaredConstructor(typeof(ViewModel)),
                prefix: AccessTools2.DeclaredMethod(typeof(ViewModelPatch), nameof(ViewModelCtorPrefix)));
        }

        private static bool ViewModelCtorPrefix(ViewModel __instance, ref Type ____type, ref object ____propertiesAndMethods)
        {
            if (__instance is BUTRViewModel && ViewModelExtension.DataSourceTypeBindingPropertiesCollectionCtor is { } ctor)
            {
                ____type = __instance.GetType();
                ____propertiesAndMethods = ctor(new Dictionary<string, PropertyInfo>(), new Dictionary<string, MethodInfo>());

                return false;
            }
            return true;
        }
    }
}