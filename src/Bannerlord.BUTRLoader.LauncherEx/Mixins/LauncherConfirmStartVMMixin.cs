using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Localization;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Mixins
{
    internal sealed class LauncherConfirmStartVMMixin : ViewModelMixin<LauncherConfirmStartVMMixin, LauncherConfirmStartVM>
    {
        [BUTRDataSourceProperty]
        public string CancelText2 => new BUTRTextObject("{=DzJmcvsP}Cancel").ToString();
        [BUTRDataSourceProperty]
        public string ConfirmText2 => new BUTRTextObject("{=epTxGUqT}Confirm").ToString();

        public LauncherConfirmStartVMMixin(LauncherConfirmStartVM launcherNewsVM) : base(launcherNewsVM) { }
    }
}