using Bannerlord.BUTRLoader.Helpers;

using System;
using System.Xml;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    /// <summary>
    /// BUTRLoader text up the Version. Singleplayer
    /// </summary>
    internal sealed class UILauncherPrefabExtension1 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/TextWidget[1]";

        public override string Id => "UILauncherPrefabExtension1";
        public override InsertType Type { get; } = InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension1()
        {
            XmlDocument.LoadXml(@"
<TextWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" VerticalAlignment=""Bottom"" Brush=""Launcher.Version.Text"" MarginLeft=""7"" MarginBottom=""25"" IsHidden=""@IsMultiplayer"" Text=""@VersionTextSingleplayer""/>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    /// <summary>
    /// BUTRLoader text up the Version. Multiplayer
    /// </summary>
    internal sealed class UILauncherPrefabExtension2 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/TextWidget[1]";

        public override string Id => "UILauncherPrefabExtension2";
        public override InsertType Type { get; } = InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension2()
        {
            XmlDocument.LoadXml(@"
<TextWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" VerticalAlignment=""Bottom"" Brush=""Launcher.Version.Text"" MarginLeft=""7"" MarginBottom=""5"" IsHidden=""@IsSingleplayer"" Text=""@VersionTextMultiplayer""/>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }


    /// <summary>
    /// Adds Options button on very top
    /// </summary>
    internal sealed class UILauncherPrefabExtension3 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/Widget[1]/Children/ListPanel/Children/ButtonWidget[1]";

        public override string Id => "UILauncherPrefabExtension3";
        public override InsertType Type { get; } = InsertType.Append;
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
    internal sealed class UILauncherPrefabExtension4 : RawPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/Widget[1]/Children/Widget[1]";

        public override string Id => "UILauncherPrefabExtension4";
        public override Action<XmlNode> Patcher { get; } = node =>
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                return;
            }

            var ownerDocument = node is XmlDocument xmlDocument ? xmlDocument : node.OwnerDocument!;

            if (node.Attributes!["MarginLeft"] is null)
            {
                var attribute = ownerDocument.CreateAttribute("MarginLeft");
                node.Attributes.Append(attribute);
            }

            node.Attributes!["MarginLeft"].Value = "125";
        };
    }

    /// <summary>
    /// Appends a second Divider to the right
    /// </summary>
    internal sealed class UILauncherPrefabExtension5 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/Widget[1]/Children/Widget[1]";

        public override string Id => "UILauncherPrefabExtension5";
        public override InsertType Type { get; } = InsertType.Append;
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
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/ListPanel/Children/TabToggleWidget[2]";

        public override string Id => "UILauncherPrefabExtension6";
        public override InsertType Type { get; } = InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension6()
        {
            XmlDocument.LoadXml(@"
<TabToggleWidget DoNotPassEventsToChildren=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""100"" IsSelected=""@IsOptions"" TabControlWidget=""..\..\..\..\ContentPanel"" TabName=""OptionsPage"" UpdateChildrenStates=""true"" IsHidden=""@IsNotOptions"">
  <Children>
    <TextWidget Text=""@GeneralText"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""Launcher.SubMenuButton.SingleplayerText"" />
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
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/TabControl/Children/Launcher.Mods";

        public override string Id => "UILauncherPrefabExtension7";
        public override InsertType Type { get; } = InsertType.Append;
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
    /// Replaces ModsPage - uses 'SkipMods' instead of 'IsMultiplayer'
    /// </summary>
    internal sealed class UILauncherPrefabExtension8 : RawPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/ListPanel/Children/TabToggleWidget[2]";

        public override string Id => "UILauncherPrefabExtension8";
        public override Action<XmlNode> Patcher { get; } = node =>
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                return;
            }

            var ownerDocument = node is XmlDocument xmlDocument ? xmlDocument : node.OwnerDocument!;

            if (node.Attributes!["IsHidden"] is null)
            {
                var attribute = ownerDocument.CreateAttribute("IsHidden");
                node.Attributes.Append(attribute);
            }

            node.Attributes!["IsHidden"].Value = "@SkipMods";
        };
    }

    /// <summary>
    /// Replaces ModsPage - uses 'SkipNews' instead of 'IsMultiplayer'
    /// </summary>
    internal sealed class UILauncherPrefabExtension9 : RawPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/ListPanel/Children/TabToggleWidget[1]";

        public override string Id => "UILauncherPrefabExtension9";
        public override Action<XmlNode> Patcher { get; } = node =>
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                return;
            }

            var ownerDocument = node is XmlDocument xmlDocument ? xmlDocument : node.OwnerDocument!;

            if (node.Attributes!["IsHidden"] is null)
            {
                var attribute = ownerDocument.CreateAttribute("IsHidden");
                node.Attributes.Append(attribute);
            }

            node.Attributes!["IsHidden"].Value = "@SkipNews";
        };
    }

    /// <summary>
    /// Changing to Option screen will change image
    /// </summary>
    internal sealed class UILauncherPrefabExtension10 : RawPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/LauncherRandomImageWidget";

        public override string Id => "UILauncherPrefabExtension10";
        public override Action<XmlNode> Patcher { get; } = node =>
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                return;
            }

            var ownerDocument = node is XmlDocument xmlDocument ? xmlDocument : node.OwnerDocument!;

            if (node.Attributes!["ChangeTrigger"] is null)
            {
                var attribute = ownerDocument.CreateAttribute("ChangeTrigger");
                node.Attributes.Append(attribute);
            }

            node.Attributes!["ChangeTrigger"].Value = "@RandomImageSwitch";
        };
    }

    /// <summary>
    /// Hides PLAY button when in Options
    /// </summary>
    internal sealed class UILauncherPrefabExtension11 : RawPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "descendant::ButtonWidget[@Id='PlayButton']";

        public override string Id => "UILauncherPrefabExtension11";
        public override Action<XmlNode> Patcher { get; } = node =>
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                return;
            }

            var ownerDocument = node is XmlDocument xmlDocument ? xmlDocument : node.OwnerDocument!;

            if (node.Attributes!["IsHidden"] is null)
            {
                var attribute = ownerDocument.CreateAttribute("IsHidden");
                node.Attributes.Append(attribute);
            }

            node.Attributes!["IsHidden"].Value = "@IsOptions";
        };
    }

    /// <summary>
    /// Hides PLAY button when in Options
    /// </summary>
    internal sealed class UILauncherPrefabExtension12 : RawPatch
    {
        public static string Movie { get; } = "UILauncher";
        public static string XPath { get; } = "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/TabControl/Children/Launcher.News";

        public override string Id => "UILauncherPrefabExtension12";
        public override Action<XmlNode> Patcher { get; } = node =>
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                return;
            }

            var ownerDocument = node is XmlDocument xmlDocument ? xmlDocument : node.OwnerDocument!;

            if (node.Attributes!["IsDisabled"] is null)
            {
                var attribute = ownerDocument.CreateAttribute("IsDisabled");
                node.Attributes.Append(attribute);
            }

            node.Attributes!["IsDisabled"].Value = "@IsDisabledOnMultiplayer";
        };
    }
}