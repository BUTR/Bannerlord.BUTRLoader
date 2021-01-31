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
<Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""CoverChildren"" SuggestedWidth=""200"" MarginLeft=""-200""
        VerticalAlignment=""Center""
        IsVisible=""@HasIssues"" UpdateChildrenStates=""true"">
  <Children>
    <ButtonWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""40"" SuggestedHeight=""40""
                  HorizontalAlignment=""Left"" VerticalAlignment=""Center""
                  ButtonType=""Toggle"" Brush=""Launcher.Mods.ExpandIndicator"" IsSelected=""@IsExpanded""
                  DoNotPassEventsToChildren=""true""/>
  </Children>
</Widget>
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


    internal sealed class LauncherModsPrefabExtension4 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = "Launcher.Mods";
        public static string XPath { get; } = "descendant::ListPanel[@Id='InnerPanel']//ListPanel[@DragWidget='DragWidget']";

        public override string Attribute => "SuggestedHeight";
        public override string Value => BUTRLoaderAppDomainManager.CompactModuleList ? "24" : "26";
    }
    internal sealed class LauncherModsPrefabExtension5 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = "Launcher.Mods";
        public static string XPath { get; } = "descendant::ListPanel[@Id='InnerPanel']//ListPanel[@DragWidget='DragWidget']";

        public override string Attribute => "MarginBottom";
        public override string Value => BUTRLoaderAppDomainManager.CompactModuleList ? "2" : "10";
    }
    internal sealed class LauncherModsPrefabExtension6 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = "Launcher.Mods";
        public static string XPath { get; } = "descendant::ListPanel[@Id='InnerPanel']//TextWidget[@Text='@Name']";

        public override string Attribute => "Brush.FontSize";
        public override string Value => BUTRLoaderAppDomainManager.CompactModuleList ? "20" : "26";
    }
    internal sealed class LauncherModsPrefabExtension7 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = "Launcher.Mods";
        public static string XPath { get; } = "descendant::ListPanel[@Id='InnerPanel']//TextWidget[@Text='@VersionText']";

        public override string Attribute => "Brush.FontSize";
        public override string Value => BUTRLoaderAppDomainManager.CompactModuleList ? "20" : "26";
    }
}