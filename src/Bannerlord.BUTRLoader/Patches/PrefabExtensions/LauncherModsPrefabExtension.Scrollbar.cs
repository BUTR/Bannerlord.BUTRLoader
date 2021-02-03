using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    /// <summary>
    /// Adds a scrollbar
    /// </summary>
    internal sealed class LauncherModsPrefabExtension3 : PrefabExtensionReplacePatch
    {
        public static string Movie { get; } = "Launcher.Mods";
        public static string XPath { get; } = "descendant::ScrollbarWidget[@Id='Scrollbar']";

        private XmlDocument XmlDocument { get; } = new();

        public LauncherModsPrefabExtension3()
        {
            XmlDocument.LoadXml(@"
<ScrollbarWidget Id=""Scrollbar"" Handle=""ScrollbarHandle"" AlignmentAxis=""Vertical"" MinValue=""0"" MaxValue=""100""
                 HorizontalAlignment=""Right"" VerticalAlignment=""Center""
                 WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent""
                 SuggestedWidth=""20"" MarginTop=""46"" MarginBottom=""23"" MarginRight=""120""
                 UpdateChildrenStates=""true"" >
  <Children>
    <Widget
            HorizontalAlignment=""Center""
            WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent""
            SuggestedWidth=""4""
            Sprite=""BlankWhiteSquare_9"" Color=""#5A4033FF"" />
    <Widget Id=""ScrollbarHandle""
            SuggestedWidth=""12""
            Sprite=""BlankWhiteSquare_9"" Color=""#E6C8A6FF"" />
  </Children>
</ScrollbarWidget>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
}