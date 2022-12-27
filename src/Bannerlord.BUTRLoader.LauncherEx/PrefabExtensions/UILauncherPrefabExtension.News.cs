using Bannerlord.BUTRLoader.Helpers;

namespace Bannerlord.BUTRLoader.PrefabExtensions
{
    /// <summary>
    /// ModsPage - uses 'HasNoNews' instead of 'IsMultiplayer'
    /// </summary>
    internal sealed class UILauncherPrefabExtension8 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TabToggleWidget[1]";

        public override string Attribute => "IsHidden";
        public override string Value => "@HasNoNews";
    }

    /// <summary>
    /// News tab can be disabled
    /// </summary>
    internal sealed class UILauncherPrefabExtension12 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::Launcher.News";

        public override string Attribute => "IsDisabled";
        public override string Value => "@IsDisabled2";
    }
}