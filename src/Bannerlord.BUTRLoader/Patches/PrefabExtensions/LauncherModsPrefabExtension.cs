using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    internal sealed class LauncherModsPrefabExtension1 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie { get; } = "Launcher.Mods";
        public static string XPath { get; } = "descendant::Widget[@Id='DragWidget']";

        public override InsertType Type => InsertType.Prepend;
        private XmlDocument XmlDocument { get; } = new();

        public LauncherModsPrefabExtension1()
        {
            XmlDocument.LoadXml(@"
<ButtonWidget HeightSizePolicy=""CoverChildren"" WidthSizePolicy=""Fixed"" VerticalAlignment=""Center""
              SuggestedWidth=""200"" MarginLeft=""-200""
              IsDisabled=""@HasNoIssues"" IsVisible=""@HasIssues"" Command.Click=""ExecuteExpander"" UpdateChildrenStates=""true"" DoNotPassEventsToChildren=""true"">
  <Children>
    <TextWidget Text=""@Expander"" HeightSizePolicy=""CoverChildren"" WidthSizePolicy=""CoverChildren"" HorizontalAlignment=""Left""
                PositionYOffset=""2"" Brush=""Launcher.Mods.ModNameText"" VerticalAlignment=""Center""/>
  </Children>
</ButtonWidget>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    internal sealed class LauncherModsPrefabExtension2 : PrefabExtensionCustomPatch<XmlNode>
    {
        public static string Movie { get; } = "Launcher.Mods";
        public static string XPath { get; } = "/Prefab/Window/Widget/Children/ScrollablePanel/Children/Widget/Children/ListPanel/ItemTemplate/ListPanel";

        private XmlDocument XmlDocument { get; } = new();

        public LauncherModsPrefabExtension2()
        {
            XmlDocument.LoadXml(@"
<ListPanel LayoutImp.LayoutMethod=""VerticalBottomToTop"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"">
  <Children>
    <Widget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" MarginLeft=""30""
            IsDisabled=""true"" IsVisible=""@IsExpanded"">
      <Children>
        <RichTextWidget Text=""@IssuesText"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" SuggestedHeight=""500"" MarginBottom=""10""
                        Brush=""Launcher.Mods.IssueText"" Brush.TextHorizontalAlignment=""Left"" />
      </Children>
    </Widget>
  </Children>
</ListPanel>
");
        }

        public override void Apply(XmlNode node)
        {
            var ownerDocument = node is XmlDocument xmlDocument ? xmlDocument : node.OwnerDocument;
            if (ownerDocument is null)
            {
                return;
            }

            if (node.ParentNode is null)
            {
                return;
            }

            var extensionNode = XmlDocument.DocumentElement;
            if (extensionNode is null)
            {
                return;
            }


            var importedExtensionNode = ownerDocument.ImportNode(extensionNode, true);
            var clone = node.CloneNode(true);
            importedExtensionNode.FirstChild.PrependChild(clone);
            node.ParentNode.ReplaceChild(importedExtensionNode, node);
        }
    }
}