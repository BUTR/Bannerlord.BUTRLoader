using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    /// <summary>
    /// Adds a refresh button
    /// </summary>
    internal sealed class LauncherModsPrefabExtension14 : PrefabExtensionReplacePatch
    {
        public static string Movie { get; } = "Launcher.Mods";
        public static string XPath { get; } = "/Prefab/Window/Widget/Children/ListPanel/Children/Widget[2]";

        private XmlDocument XmlDocument { get; } = new();

        public LauncherModsPrefabExtension14()
        {
            XmlDocument.LoadXml(@"
<ButtonWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed""
              SuggestedWidth=""!ModToggle.Width"" SuggestedHeight=""!ModToggle.Height"" MarginLeft=""40""
              VerticalAlignment=""Center""
              Brush=""Launcher.Refresh""
              Command.Click=""ExecuteRefresh"" />
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
}