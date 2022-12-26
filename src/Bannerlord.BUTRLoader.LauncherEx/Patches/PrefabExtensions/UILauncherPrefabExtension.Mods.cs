using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    /// <summary>
    /// ModsPage - uses 'HasNoMods' instead of 'IsMultiplayer'
    /// </summary>
    internal sealed class UILauncherPrefabExtension9 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TabToggleWidget[2]";

        public override string Attribute => "IsHidden";
        public override string Value => "@HasNoMods";
    }

    internal sealed class UILauncherPrefabExtension27 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TabToggleWidget[2]";

        public override string Attribute => "IsSelected";
        public override string Value => "@IsModsDataSelected";
    }

    /// <summary>
    /// Replaces Launcher.Mods with our own static implementation, since we add a lot of custom stuff anyway
    /// </summary>
    internal sealed class UILauncherPrefabExtension13 : PrefabExtensionReplacePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::Launcher.Mods";

        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension13()
        {
            XmlDocument.LoadXml(@"
<Launcher.Mods2 Id=""ModsPage"" DataSource=""{ModsData}"" IsDisabled=""@IsDisabled2"" />
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
}