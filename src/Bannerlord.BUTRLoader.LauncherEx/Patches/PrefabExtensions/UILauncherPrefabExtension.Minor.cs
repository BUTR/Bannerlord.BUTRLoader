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
            var verticalOffset = 90;

            XmlDocument.LoadXml(@$"
<TextWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" VerticalAlignment=""Bottom"" Brush=""Launcher.Version.Text"" MarginLeft=""7"" MarginBottom=""{verticalOffset}"" IsHidden=""@IsNotSingleplayer"" Text=""@BUTRLoaderVersionText""/>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
}