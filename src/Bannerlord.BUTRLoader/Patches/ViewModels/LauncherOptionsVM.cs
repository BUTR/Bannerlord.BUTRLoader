using Bannerlord.BUTRLoader.Helpers;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Patches.ViewModels
{
    internal class LauncherOptionsVM : ViewModel
	{
        [DataSourceProperty]
        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                if (value != _isDisabled)
                {
                    _isDisabled = value;
                    OnPropertyChangedWithValue(value, nameof(IsDisabled));
                }
            }
        }
        private bool _isDisabled;

        [DataSourceProperty]
        public MBBindingList<SettingsPropertyVM> SettingProperties
        {
            get => _settingProperties;
            set
            {
                if (value != _settingProperties)
                {
                    _settingProperties = value;
                    OnPropertyChangedWithValue(value, nameof(SettingProperties));
                }
            }
        }
        private MBBindingList<SettingsPropertyVM> _settingProperties = new();

        public void Refresh(bool disable)
        {
            IsDisabled = disable;

            SettingProperties.Clear();
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Enable Extended Mod Sorting",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(BUTRLoaderAppDomainManager).GetProperty(nameof(BUTRLoaderAppDomainManager.ExtendedSorting))!, this)
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