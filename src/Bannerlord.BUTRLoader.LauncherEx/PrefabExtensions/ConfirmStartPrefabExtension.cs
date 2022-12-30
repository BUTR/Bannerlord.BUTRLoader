using Bannerlord.BUTRLoader.Helpers;

namespace Bannerlord.BUTRLoader.PrefabExtensions
{
    internal sealed class ConfirmStartPrefabExtension1 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "Launcher.ConfirmStart";
        public static string XPath => "descendant::TextWidget[@Text='Cancel']";

        public override string Attribute => "Text";
        public override string Value => "@CancelText2";
    }
    internal sealed class ConfirmStartPrefabExtension2 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "Launcher.ConfirmStart";
        public static string XPath => "descendant::TextWidget[@Text='Confirm']";

        public override string Attribute => "Text";
        public override string Value => "@ConfirmText2";
    }
    internal sealed class ConfirmStartPrefabExtension3 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "Launcher.ConfirmStart";
        public static string XPath => "descendant::ButtonWidget[@Command.Click='ExecuteCancel']";

        public override string Attribute => "SuggestedWidth";
        public override string Value => "200";
    }
    internal sealed class ConfirmStartPrefabExtension4 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "Launcher.ConfirmStart";
        public static string XPath => "descendant::ButtonWidget[@Command.Click='ExecuteConfirm']";

        public override string Attribute => "SuggestedWidth";
        public override string Value => "200";
    }
}