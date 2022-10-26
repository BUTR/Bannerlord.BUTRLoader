using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Helpers;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    /// <summary>
    /// Replaces IsDisabled with our implementation
    /// </summary>
    internal sealed class LauncherModsPrefabExtension12 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = "Launcher.Mods.ModuleTuple";

        public static string XPath { get; } = "/Prefab/Window/ListPanel/Children/ListPanel/Children/ButtonWidget[1]";

        public override string Attribute => "IsDisabled";
        public override string Value => "@IsDisabled2";
    }

    /// <summary>
    /// Replaces IsDisabled with our implementation
    /// </summary>
    internal sealed class LauncherModsPrefabExtension13 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = "Launcher.Mods.ModuleTuple";

        public static string XPath { get; } = "/Prefab/Window/ListPanel/Children/ListPanel/Children/ButtonWidget[1]/Children/ImageWidget";

        public override string Attribute => "IsDisabled";
        public override string Value => "@IsDisabled2";
    }
}