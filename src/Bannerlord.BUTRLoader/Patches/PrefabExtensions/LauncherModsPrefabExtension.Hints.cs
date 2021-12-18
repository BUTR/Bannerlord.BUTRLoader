using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Helpers;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    internal sealed class LauncherModsPrefabExtension9 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = "Launcher.Mods.ModuleTuple";
        public static string XPath { get; } = "/Prefab/Window/ListPanel/Children/ListPanel/Children/Widget[2]/Children/LauncherHintTriggerWidget";

        public override string Attribute => "DataSource";
        public override string Value => "{DependencyHint2}";
    }

    internal sealed class LauncherModsPrefabExtension10 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = "Launcher.Mods.ModuleTuple";
        public static string XPath { get; } = "/Prefab/Window/ListPanel/Children/ListPanel/Children/Widget[2]";

        public override string Attribute => "IsVisible";
        public override string Value => "@AnyDependencyAvailable2";
    }

    internal sealed class LauncherModsPrefabExtension11 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = ApplicationVersionHelper.GameVersion() is { Major: 1, Minor: >= 7 }
            ? "Launcher.Mods.ModuleTuple"
            : "Launcher.Mods";
        public static string XPath { get; } = ApplicationVersionHelper.GameVersion() is { Major: 1, Minor: >= 7 }
            ? "/Prefab/Window/ListPanel/Children/ListPanel/Children/Widget[3]"
            : "descendant::ListPanel[@Id='InnerPanel']/ItemTemplate/ListPanel/Children/ListPanel/Children/Widget[2]";

        public override string Attribute => "IsVisible";
        public override string Value => "@IsDangerous2";
    }
}