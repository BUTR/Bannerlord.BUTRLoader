using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    internal sealed class UILauncherPrefabExtension1 : PrefabExtensionReplacePatch
    {
        public static string Movie { get; } = "UILauncherPrefabExtension1";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/TextWidget";

        public override string Id => "UILauncherPrefabExtension1";
        private XmlDocument XmlDocument { get; } = new XmlDocument();

        public UILauncherPrefabExtension1()
        {
            XmlDocument.LoadXml("<TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" VerticalAlignment=\"Bottom\" Brush=\"Launcher.Version.Text\" MarginLeft=\"7\" MarginBottom=\"5\" IsHidden=\"@IsMultiplayer\" Text=\"@VersionTextSingleplayer\"/>");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    internal sealed class UILauncherPrefabExtension2 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/TextWidget";

        public override string Id => "UILauncherPrefabExtension2";
        public override InsertType Type { get; } = InsertType.Append;
        private XmlDocument XmlDocument { get; } = new XmlDocument();

        public UILauncherPrefabExtension2()
        {
            XmlDocument.LoadXml("<TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" VerticalAlignment=\"Bottom\" Brush=\"Launcher.Version.Text\" MarginLeft=\"7\" MarginBottom=\"5\" IsHidden=\"@IsSingleplayer\" Text=\"@VersionTextMultiplayer\"/>");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
}