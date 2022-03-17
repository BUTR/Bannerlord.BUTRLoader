using Bannerlord.BUTRLoader.Helpers;

using System.Xml;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Patches.PrefabExtensions
{
    /// <summary>
    /// Adds a checkbox to disable/enable everything
    /// </summary>
    internal sealed class LauncherModsPrefabExtension8 : PrefabExtensionReplacePatch
    {
        public static string Movie { get; } = "Launcher.Mods";
        public static string XPath { get; } = "/Prefab/Window/Widget/Children/ListPanel/Children/Widget[1]";

        private XmlDocument XmlDocument { get; } = new();

        public LauncherModsPrefabExtension8()
        {
            XmlDocument.LoadXml(@"
<ButtonWidget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed""
        SuggestedWidth=""!ModToggle.Width"" SuggestedHeight=""!ModToggle.Height""
        VerticalAlignment=""Center""
        Brush=""Launcher.Mods.ModToggle"" DoNotPassEventsToChildren=""true""
        IsSelected=""@GlobalCheckboxState"" Command.Click=""ExecuteGlobalCheckbox"">
  <Children>
    <ImageWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent""
                 Brush=""Launcher.Mods.ModToggle.Checkmark""
                 IsVisible=""@GlobalCheckboxState"" />
  </Children>
</ButtonWidget>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
}