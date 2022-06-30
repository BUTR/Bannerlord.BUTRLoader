using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    /// <summary>
    /// Adds the issue widget expand button
    /// </summary>
    internal sealed class LauncherModsPrefabExtension1 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie { get; } = ApplicationVersionHelper.GameVersion() is { Major: 1, Minor: >= 7 }
            ? "Launcher.Mods.ModuleTuple"
            : "Launcher.Mods";
        public static string XPath { get; } = "descendant::TextWidget[@Text='@VersionText']";

        public override InsertType Type => InsertType.Prepend;
        private XmlDocument XmlDocument { get; } = new();

        public LauncherModsPrefabExtension1()
        {
            XmlDocument.LoadXml(@$"
<Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""CoverChildren"" SuggestedWidth=""40""
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

    /// <summary>
    /// Adds another ListPanel as parent of the Module ListPanel
    /// and adds a hideable module issue status widget
    /// </summary>
    internal sealed class LauncherModsPrefabExtension2 : PrefabExtensionCustomPatch<XmlNode>
    {
        public static string Movie { get; } = ApplicationVersionHelper.GameVersion() is { Major: 1, Minor: >= 7 }
            ? "Launcher.Mods.ModuleTuple"
            : "Launcher.Mods";
        public static string XPath { get; } = ApplicationVersionHelper.GameVersion() is { Major: 1, Minor: >= 7 }
            ? "/Prefab/Window/ListPanel"
            : "descendant::ListPanel[@Id='InnerPanel']/ItemTemplate/ListPanel";

        private XmlDocument XmlDocument { get; } = new();

        public LauncherModsPrefabExtension2()
        {
            XmlDocument.LoadXml(@"
<ListPanel LayoutImp.LayoutMethod=""VerticalBottomToTop"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"">
  <Children>
    <Widget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" MarginLeft=""30""
            IsDisabled=""true"" IsVisible=""@IsExpanded"">
      <Children>
        <TextWidget Text=""@IssuesText"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" SuggestedHeight=""500"" MarginBottom=""10""
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