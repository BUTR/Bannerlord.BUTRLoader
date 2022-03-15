using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    /// <summary>
    /// BUTRLoader text up the Version. Singleplayer
    /// </summary>
    internal sealed class UILauncherPrefabExtension1 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "descendant::TextWidget[@Text='@VersionText']";

        public override InsertType Type { get; } = InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension1()
        {
            var verticalOffset = 25;
            if (ApplicationVersionHelper.GameVersion() is { Major: 1, Minor: >= 7 })
            {
                verticalOffset = 90;
            }

            XmlDocument.LoadXml(@$"
<TextWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" VerticalAlignment=""Bottom"" Brush=""Launcher.Version.Text"" MarginLeft=""7"" MarginBottom=""{verticalOffset}"" IsHidden=""@IsMultiplayer"" Text=""@VersionTextSingleplayer""/>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    /// <summary>
    /// BUTRLoader text up the Version. Multiplayer
    /// </summary>
    internal sealed class UILauncherPrefabExtension2 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "descendant::TextWidget[@Text='@VersionText']";

        public override InsertType Type { get; } = InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension2()
        {
            XmlDocument.LoadXml(@"
<TextWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" VerticalAlignment=""Bottom"" Brush=""Launcher.Version.Text"" MarginLeft=""7"" MarginBottom=""5"" IsHidden=""@IsSingleplayer"" Text=""@VersionTextMultiplayer""/>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }


    /// <summary>
    /// Hides Caution window. Legacy
    /// </summary>
    internal sealed class UILauncherPrefabExtension13 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "descendant::ButtonWidget[@Id='PlayButton']";

        public override string Attribute => "Command.Click";
        public override string Value => "ExecuteConfirmUnverifiedDLLStart";
    }
}