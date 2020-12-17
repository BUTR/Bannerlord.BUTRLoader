using Bannerlord.BUTRLoader.Patches;

using HarmonyLib;

using System.Collections.Generic;
using System.Xml;

using TaleWorlds.GauntletUI.PrefabSystem;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class PrefabInjector
    {
        private static readonly AccessTools.FieldRef<object, IDictionary<string, string>>? GetCustomTypePaths =
            AccessTools2.FieldRefAccess<IDictionary<string, string>>(typeof(WidgetFactory), "_customTypePaths");

        public static void Register(string name)
        {
            if (GetCustomTypePaths is not null)
            {
                var dict = GetCustomTypePaths(LauncherUIPatch.WidgetFactory);
                if (!dict.ContainsKey(name))
                    dict.Add(name, "");
            }
        }

        public static WidgetPrefab Create(XmlDocument doc)
        {
            return WidgetPrefabPatch.LoadFromDocument(
                LauncherUIPatch.WidgetFactory.PrefabExtensionContext,
                LauncherUIPatch.WidgetFactory.WidgetAttributeContext,
                string.Empty,
                doc);
        }
    }
}