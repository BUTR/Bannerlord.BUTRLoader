#if CONTINUE
using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    internal sealed class UILauncherPrefabExtension21 : PrefabExtensionSetAttributePatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "descendant::ButtonWidget[@Id='PlayButton']";

        public override string Attribute => "HorizontalAlignment";
        public override string Value => "@PlayButtonAlignment";
    }

    internal sealed class UILauncherPrefabExtension22 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "descendant::ButtonWidget[@Id='PlayButton']";

        public override InsertType Type { get; } = InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension22()
        {
            XmlDocument.LoadXml(@"
<ButtonWidget Id=""ContinueButton""
              WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed""
              SuggestedWidth=""!PlayButton.Frame.Width"" SuggestedHeight=""!PlayButton.Frame.Height""
              HorizontalAlignment=""Left"" VerticalAlignment=""Bottom"" MarginBottom=""18""
              Command.Click=""ExecuteStartGame"" UpdateChildrenStates=""true"" DoNotPassEventsToChildren=""true""
              IsHidden=""@IsNotSingleplayer"" >
  <Children>
    <BrushWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed""
                 SuggestedWidth=""!PlayButton.Inner.Width"" SuggestedHeight=""!PlayButton.Inner.Height""
                 HorizontalAlignment=""Center"" VerticalAlignment=""Center""
                 Brush=""Launcher.PlayButton.Inner.Singleplayer"" />
    <ListPanel LayoutImp.LayoutMethod=""VerticalBottomToTop""
               WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed""
               SuggestedWidth=""!PlayButton.Inner.Width"" SuggestedHeight=""!PlayButton.Inner.Height""
               HorizontalAlignment=""Center"" VerticalAlignment=""Center"" >
      <Children>
        <TextWidget Text=""Continue:"" PositionYOffset=""2""
                    WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent""
                    HorizontalAlignment=""Center"" VerticalAlignment=""Center""
                    Brush=""Launcher.PlayButton.Text"" Brush.FontSize=""24"" />
        <TextWidget Text=""@Continue"" PositionYOffset=""2""
                    WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent""
                    HorizontalAlignment=""Center"" VerticalAlignment=""Center""
                    Brush=""Launcher.PlayButton.Text"" Brush.FontSize=""18"" />
      </Children>
    </ListPanel>
    <BrushWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed""
                 SuggestedWidth=""!PlayButton.Frame.Width"" SuggestedHeight=""!PlayButton.Frame.Height""
                 HorizontalAlignment=""Center"" VerticalAlignment=""Center""
                 Brush=""Launcher.PlayButton.Frame"" />
  </Children>
</ButtonWidget>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    internal sealed class UILauncherPrefabExtension23 : PrefabExtensionCustomPatch<XmlNode>
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "descendant::ButtonWidget[@Id='PlayButton']";

        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension23()
        {
            XmlDocument.LoadXml(@"
<ListPanel LayoutImp.LayoutMethod=""HorizontalCentered""
           WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren""
           HorizontalAlignment=""Center"" VerticalAlignment=""Bottom"" MarginBottom=""18"" >
  <Children>
    <ButtonWidget Id=""ContinueButton""
                  WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed""
                  SuggestedWidth=""!PlayButton.Frame.Width"" SuggestedHeight=""!PlayButton.Frame.Height""
                  HorizontalAlignment=""Left"" VerticalAlignment=""Bottom"" MarginBottom=""18""
                  Command.Click=""ExecuteStartGame"" UpdateChildrenStates=""true"" DoNotPassEventsToChildren=""true""
                  IsHidden=""@IsNotSingleplayer"" >
      <Children>
        <BrushWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed""
                     SuggestedWidth=""!PlayButton.Inner.Width"" SuggestedHeight=""!PlayButton.Inner.Height""
                     HorizontalAlignment=""Center"" VerticalAlignment=""Center""
                     Brush=""Launcher.PlayButton.Inner.Singleplayer"" />
        <ListPanel LayoutImp.LayoutMethod=""VerticalBottomToTop""
                   WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed""
                   SuggestedWidth=""!PlayButton.Inner.Width"" SuggestedHeight=""!PlayButton.Inner.Height""
                   HorizontalAlignment=""Center"" VerticalAlignment=""Center"" >
          <Children>
            <TextWidget Text=""Continue:"" PositionYOffset=""2""
                        WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent""
                        HorizontalAlignment=""Center"" VerticalAlignment=""Center""
                        Brush=""Launcher.PlayButton.Text"" Brush.FontSize=""24"" />
            <TextWidget Text=""@Continue"" PositionYOffset=""2""
                        WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent""
                        HorizontalAlignment=""Center"" VerticalAlignment=""Center""
                        Brush=""Launcher.PlayButton.Text"" Brush.FontSize=""18"" />
          </Children>
        </ListPanel>
        <BrushWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed""
                     SuggestedWidth=""!PlayButton.Frame.Width"" SuggestedHeight=""!PlayButton.Frame.Height""
                     HorizontalAlignment=""Center"" VerticalAlignment=""Center""
                     Brush=""Launcher.PlayButton.Frame"" />
      </Children>
    </ButtonWidget>
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
#endif