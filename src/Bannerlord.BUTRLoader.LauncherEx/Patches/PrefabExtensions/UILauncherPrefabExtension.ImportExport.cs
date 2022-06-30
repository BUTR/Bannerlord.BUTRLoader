using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    internal sealed class UILauncherPrefabExtension14 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "descendant::TextWidget[@Text='@VersionText']";

        public override InsertType Type { get; } = InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension14()
        {
            var verticalOffset = 35;

            XmlDocument.LoadXml(@$"
<ListPanel WidthSizePolicy=""CoverChildren"" HeightSizePolicy=""CoverChildren""
           VerticalAlignment=""Bottom""
           MarginLeft =""5"" MarginBottom=""{verticalOffset}""
           LayoutImp.LayoutMethod=""HorizontalLeftToRight"" IsHidden=""@IsMultiplayer"">
  <Children>
    <ButtonWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""40"" SuggestedHeight=""40""
                  VerticalAlignment=""Bottom""
                  MarginLeft =""3"" MarginRight=""3"" MarginTop=""3"" MarginBottom=""3""
                  Brush=""Launcher.Import"" IsHidden=""@IsMultiplayer"" DoNotPassEventsToChildren=""true""
                  Command.Click=""ExecuteImport"" Command.HoverBegin=""ExecuteBeginHintImport"" Command.HoverEnd=""ExecuteEndHint""/>

    <ButtonWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""40"" SuggestedHeight=""40""
                  VerticalAlignment=""Bottom""
                  MarginLeft =""3"" MarginRight=""3"" MarginTop=""3"" MarginBottom=""3""
                  Brush=""Launcher.Export"" IsHidden=""@IsMultiplayer""
                  Command.Click=""ExecuteExport"" Command.HoverBegin=""ExecuteBeginHintExport"" Command.HoverEnd=""ExecuteEndHint""/>
  </Children>
</ListPanel>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
}