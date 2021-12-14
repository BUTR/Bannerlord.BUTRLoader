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
        public static string Movie { get; } = "Launcher.Mods.ModuleTuple";
        public static string XPath { get; } = "/Prefab/Window/ListPanel/Children/ListPanel/Children/Widget[3]";

        public override string Attribute => "IsVisible";
        public override string Value => "@IsDangerous2";
    }
}
