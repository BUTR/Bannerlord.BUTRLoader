using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Helpers;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    /// <summary>
    /// Replaces IsDisabled with our implementation
    /// </summary>
    internal sealed class LauncherModsPrefabExtension12 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = ApplicationVersionHelper.GameVersion() is { Major: 1, Minor: >= 7 }
            ? "Launcher.Mods.ModuleTuple"
            : "Launcher.Mods";

        public static string XPath { get; } = ApplicationVersionHelper.GameVersion() is { Major: 1, Minor: >= 7 }
            ? "/Prefab/Window/ListPanel/Children/ListPanel/Children/ButtonWidget[1]"
            : "descendant::ListPanel[@Id='InnerPanel']/ItemTemplate/ListPanel/Children/ListPanel/Children/ButtonWidget[1]";

        public override string Attribute => "IsDisabled";
        public override string Value => "@IsDisabled2";
    }

    /// <summary>
    /// Replaces IsDisabled with our implementation
    /// </summary>
    internal sealed class LauncherModsPrefabExtension13 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = ApplicationVersionHelper.GameVersion() is { Major: 1, Minor: >= 7 }
            ? "Launcher.Mods.ModuleTuple"
            : "Launcher.Mods";

        public static string XPath { get; } = ApplicationVersionHelper.GameVersion() is { Major: 1, Minor: >= 7 }
            ? "/Prefab/Window/ListPanel/Children/ListPanel/Children/ButtonWidget[1]/Children/ImageWidget"
            : "descendant::ListPanel[@Id='InnerPanel']/ItemTemplate/ListPanel/Children/ListPanel/Children/ButtonWidget[1]/Children/ImageWidget";

        public override string Attribute => "IsDisabled";
        public override string Value => "@IsDisabled2";
    }
}