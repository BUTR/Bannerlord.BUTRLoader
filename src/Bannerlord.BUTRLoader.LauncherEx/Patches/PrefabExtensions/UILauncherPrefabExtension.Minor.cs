using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    /// <summary>
    /// BUTRLoader text up the Version. Singleplayer
    /// </summary>
    internal sealed class UILauncherPrefabExtension1 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TextWidget[@Text='@VersionText']";

        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension1()
        {
            var verticalOffset = 90;

            XmlDocument.LoadXml(@$"
<TextWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" VerticalAlignment=""Bottom"" Brush=""Launcher.Version.Text"" MarginLeft=""7"" MarginBottom=""{verticalOffset}"" IsHidden=""@HideBUTRLoaderVersionText"" Text=""@BUTRLoaderVersionText""/>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    /// <summary>
    /// Reset MarginRight of TopMenu widget
    /// </summary>
    internal sealed class UILauncherPrefabExtension15 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::Widget[@Id='TopMenu']";

        public override string Attribute => "MarginRight";
        public override string Value => "10";
    }

    /// <summary>
    /// Move the random image to the left by 100 pixels
    /// </summary>
    internal sealed class UILauncherPrefabExtension16 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::LauncherRandomImageWidget";

        public override string Attribute => "MarginRight";
        public override string Value => "-100";
    }

    /// <summary>
    /// Make the random image hideable
    /// </summary>
    internal sealed class UILauncherPrefabExtension17 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::LauncherRandomImageWidget";

        public override string Attribute => "IsHidden";
        public override string Value => "@HideRandomImage";
    }
}