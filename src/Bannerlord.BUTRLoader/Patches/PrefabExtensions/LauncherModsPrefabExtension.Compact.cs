﻿using Bannerlord.BUTRLoader.Helpers;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
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