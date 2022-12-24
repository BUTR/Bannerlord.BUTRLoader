using Bannerlord.BUTRLoader.Helpers;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.ViewModels
{
    internal sealed class BUTRLauncherOptionsVM : BUTRViewModel
    {
        [BUTRDataSourceProperty]
        public bool IsDisabled { get => _isDisabled; set => SetField(ref _isDisabled, value, nameof(IsDisabled)); }
        private bool _isDisabled;

        [BUTRDataSourceProperty]
        public MBBindingList<SettingsPropertyVM> SettingProperties { get => _settingProperties; set => SetField(ref _settingProperties, value, nameof(SettingProperties)); }
        private MBBindingList<SettingsPropertyVM> _settingProperties = new();

        public void Refresh()
        {
            SettingProperties.Clear();
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Enable File Unblocking",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.UnblockFiles))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Fix Common Issues",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.FixCommonIssues))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Compact Module List (Requires restart)",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.CompactModuleList))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Disable Binary Compatibility Check (Requires restart) (DISABLED)",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.DisableBinaryCheck))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Hide Random Image",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.HideRandomImage))!, this)
            }));
            /*
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Automatically Check for Updates",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(BUTRLoaderAppDomainManager).GetProperty(nameof(BUTRLoaderAppDomainManager.AutomaticallyCheckForUpdates))!, this)
            }));
            */
        }
    }
}