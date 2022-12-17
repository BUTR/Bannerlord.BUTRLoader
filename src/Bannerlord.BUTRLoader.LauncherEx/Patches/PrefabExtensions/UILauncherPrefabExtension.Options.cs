using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    /// <summary>
    /// Adds Options button on very top
    /// </summary>
    internal sealed class UILauncherPrefabExtension3 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/Widget[1]/Children/ListPanel/Children/ButtonWidget[1]";

        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension3()
        {
            XmlDocument.LoadXml(@"
<ButtonWidget DoNotPassEventsToChildren=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""100"" ButtonType=""Radio"" IsSelected=""@IsOptions"" UpdateChildrenStates=""true"">
  <Children>
    <TextWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""Launcher.GameTypeButton.SingleplayerText"" Text=""@OptionsText"" />
  </Children>
</ButtonWidget>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    /// <summary>
    /// Replaces original Divider with a left one
    /// </summary>
    internal sealed class UILauncherPrefabExtension4 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/Widget[1]/Children/Widget[1]";

        public override string Attribute => "MarginLeft";
        public override string Value => "125";
    }

    /// <summary>
    /// Appends a second Divider to the right
    /// </summary>
    internal sealed class UILauncherPrefabExtension5 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/Widget[1]/Children/Widget[1]";

        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension5()
        {
            XmlDocument.LoadXml(@"
<Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""2"" SuggestedHeight=""30"" HorizontalAlignment=""Center"" MarginRight=""125"" Sprite=""top_header_divider"" />
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    /// <summary>
    /// Adds General Tab on lower tab screen
    /// </summary>
    internal sealed class UILauncherPrefabExtension6 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/ListPanel/Children/TabToggleWidget[2]";

        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension6()
        {
            XmlDocument.LoadXml(@"
<TabToggleWidget DoNotPassEventsToChildren=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""100"" IsSelected=""@IsOptions"" TabControlWidget=""..\..\..\..\ContentPanel"" TabName=""OptionsPage"" UpdateChildrenStates=""true"" IsHidden=""@IsNotOptions"">
  <Children>
    <TextWidget Id=""SubMenuText"" Text=""@GeneralText"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""Launcher.SubMenuButton.SingleplayerText"" />
  </Children>
</TabToggleWidget>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    /// <summary>
    /// Adds the Options Tab View
    /// </summary>
    internal sealed class UILauncherPrefabExtension7 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/TabControl/Children/Launcher.Mods";

        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension7()
        {
            XmlDocument.LoadXml(@"
<Launcher.Options Id=""OptionsPage"" DataSource=""{OptionsData}"" IsDisabled=""@IsDisabled"" />
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    /// <summary>
    /// ModsPage - uses 'HasNoNews' instead of 'IsMultiplayer'
    /// </summary>
    internal sealed class UILauncherPrefabExtension8 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/ListPanel/Children/TabToggleWidget[1]";

        public override string Attribute => "IsHidden";
        public override string Value => "@HasNoNews";
    }

    /// <summary>
    /// ModsPage - uses 'HasNoMods' instead of 'IsMultiplayer'
    /// </summary>
    internal sealed class UILauncherPrefabExtension9 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/ListPanel/Children/TabToggleWidget[2]";

        public override string Attribute => "IsHidden";
        public override string Value => "@HasNoMods";
    }

    /// <summary>
    /// Changing to Option screen will change image
    /// </summary>
    internal sealed class UILauncherPrefabExtension10 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/LauncherRandomImageWidget";

        public override string Attribute => "ChangeTrigger";
        public override string Value => "@RandomImageSwitch";
    }

    /// <summary>
    /// Mods tab can be disabled
    /// </summary>
    internal sealed class UILauncherPrefabExtension11 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/TabControl/Children/Launcher.Mods";

        public override string Attribute => "IsDisabled";
        public override string Value => "@IsDisabled2";
    }

    /// <summary>
    /// News tab can be disabled
    /// </summary>
    internal sealed class UILauncherPrefabExtension12 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/TabControl/Children/Launcher.News";

        public override string Attribute => "IsDisabled";
        public override string Value => "@IsDisabled2";
    }
}