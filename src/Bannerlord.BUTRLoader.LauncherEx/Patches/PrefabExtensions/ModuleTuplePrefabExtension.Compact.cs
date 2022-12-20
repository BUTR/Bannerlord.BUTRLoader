﻿using Bannerlord.BUTRLoader.Helpers;

namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    internal sealed class ModuleTuplePrefabExtension4 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "Launcher.Mods.ModuleTuple2";
        public static string XPath => "descendant::ListPanel[@DragWidget='DragWidget']";

        public override string Attribute => "SuggestedHeight";
        public override string Value => LauncherSettings.CompactModuleList ? "24" : "26";
    }
    internal sealed class ModuleTuplePrefabExtension5 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "Launcher.Mods.ModuleTuple2";
        public static string XPath => "descendant::ListPanel[@DragWidget='DragWidget']";

        public override string Attribute => "MarginBottom";
        public override string Value => LauncherSettings.CompactModuleList ? "2" : "10";
    }
    internal sealed class ModuleTuplePrefabExtension6 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "Launcher.Mods.ModuleTuple2";
        public static string XPath => "descendant::TextWidget[@Text='@Name']";

        public override string Attribute => "Brush.FontSize";
        public override string Value => LauncherSettings.CompactModuleList ? "20" : "26";
    }
    internal sealed class ModuleTuplePrefabExtension7 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "Launcher.Mods.ModuleTuple2";
        public static string XPath => "descendant::TextWidget[@Text='@VersionText']";

        public override string Attribute => "Brush.FontSize";
        public override string Value => LauncherSettings.CompactModuleList ? "20" : "26";
    }
}