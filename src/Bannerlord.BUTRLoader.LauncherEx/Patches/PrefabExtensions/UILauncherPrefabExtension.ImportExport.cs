using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    internal sealed class UILauncherPrefabExtension14 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TextWidget[@Text='@VersionText']";

        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension14()
        {
            var verticalOffset = 35;

            XmlDocument.LoadXml(@$"
<ListPanel WidthSizePolicy=""CoverChildren"" HeightSizePolicy=""CoverChildren""
           VerticalAlignment=""Bottom""
           MarginLeft =""5"" MarginBottom=""{verticalOffset}""
           LayoutImp.LayoutMethod=""HorizontalLeftToRight"" IsHidden=""@IsNotSingleplayer"">
  <Children>
    <ButtonWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""40"" SuggestedHeight=""40""
                  VerticalAlignment=""Bottom""
                  MarginLeft =""3"" MarginRight=""3"" MarginTop=""3"" MarginBottom=""3""
                  Brush=""Launcher.Import"" IsHidden=""@IsNotSingleplayer""
                  Command.Click=""ExecuteImport"" Command.HoverBegin=""ExecuteBeginHintImport"" Command.HoverEnd=""ExecuteEndHint""/>

    <ButtonWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""40"" SuggestedHeight=""40""
                  VerticalAlignment=""Bottom""
                  MarginLeft =""3"" MarginRight=""3"" MarginTop=""3"" MarginBottom=""3""
                  Brush=""Launcher.Export"" IsHidden=""@IsNotSingleplayer""
                  Command.Click=""ExecuteExport"" Command.HoverBegin=""ExecuteBeginHintExport"" Command.HoverEnd=""ExecuteEndHint""/>
  </Children>
</ListPanel>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
}