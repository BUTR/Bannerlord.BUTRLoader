using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    internal sealed class UILauncherPrefabExtension24 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TabToggleWidget[5]";

        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension24()
        {
            XmlDocument.LoadXml(@"
<TabToggleWidget DoNotPassEventsToChildren=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""100"" TabControlWidget=""..\..\..\..\ContentPanel"" TabName=""SavesPage"" UpdateChildrenStates=""true"" IsSelected=""@IsSavesDataSelected"" IsHidden=""@IsNotSingleplayer"">
  <Children>
    <TextWidget Id=""SubMenuText"" Text=""@SavesText"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""Launcher.SubMenuButton.SingleplayerText"" />
  </Children>
</TabToggleWidget>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
    internal sealed class UILauncherPrefabExtension25 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::Launcher.Options[@Id='OptionsGamePage']";

        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension25()
        {
            XmlDocument.LoadXml(@"
<Launcher.Saves Id=""SavesPage"" DataSource=""{SavesData}"" IsDisabled=""@IsDisabled"" />
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    internal sealed class UILauncherPrefabExtension26 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::ButtonWidget[@Id='PlaySingleplayerButton']";

        public override string Attribute => "IsVisible";
        public override string Value => "@ShowPlaySingleplayerButton";
    }
}