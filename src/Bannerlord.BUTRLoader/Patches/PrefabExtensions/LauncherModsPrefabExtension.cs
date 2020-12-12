using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    internal sealed class LauncherModsPrefabExtension1 : PrefabExtensionReplacePatch
    {
        public static string Movie { get; } = "Launcher.Mods";
        public static string XPath { get; } = "/Prefab/Window/Widget/Children/ListPanel/Children/TextWidget[1]";

        public override string Id => "LauncherModsPrefabExtension1";
        private XmlDocument XmlDocument { get; } = new XmlDocument();

        public LauncherModsPrefabExtension1()
        {
            XmlDocument.LoadXml("<TextWidget Text=\"@NameCategoryText\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" MarginLeft=\"20\" PositionYOffset=\"2\" Brush=\"Launcher.Mods.ModNameText\" Brush.TextHorizontalAlignment=\"Left\"/>");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    internal sealed class LauncherModsPrefabExtension2 : PrefabExtensionReplacePatch
    {
        public static string Movie { get; } = "Launcher.Mods";
        public static string XPath { get; } = "descendant::ListPanel[@Id='InnerPanel']/ItemTemplate/ListPanel/Children/TextWidget[1]";

        public override string Id => "LauncherModsPrefabExtension2";
        private XmlDocument XmlDocument { get; } = new XmlDocument();

        public LauncherModsPrefabExtension2()
        {
            XmlDocument.LoadXml("<TextWidget Text=\"@Name\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" MarginLeft=\"20\" PositionYOffset=\"2\" Brush=\"Launcher.Mods.ModNameText\" Brush.TextHorizontalAlignment=\"Left\" IsDisabled=\"true\"/>");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
}