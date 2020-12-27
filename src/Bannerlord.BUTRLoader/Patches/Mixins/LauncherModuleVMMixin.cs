using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;

using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal class LauncherModuleVMMixin
    {
        public bool IsNoUpdateAvailable
        {
            get => _isUpdateAvailable;
            set
            {
                if (value != _isUpdateAvailable)
                {
                    _isUpdateAvailable = value;
                    _launcherModuleVM.OnPropertyChangedWithValue(value, nameof(IsNoUpdateAvailable));
                }
            }
        }

        private bool _isUpdateAvailable;

        private readonly LauncherModuleVM _launcherModuleVM;

        public LauncherModuleVMMixin(LauncherModuleVM launcherModuleVM)
        {
            _launcherModuleVM = launcherModuleVM;

            var propsObject = AccessTools.Field(typeof(ViewModel), "_propertyInfos")?.GetValue(_launcherModuleVM) as Dictionary<string, PropertyInfo>
                              ?? new Dictionary<string, PropertyInfo>();

            void SetVMProperty(string property)
            {
                propsObject[property] = new WrappedPropertyInfo(
                    AccessTools.Property(typeof(LauncherModuleVMMixin), property),
                    this,
                    () => _launcherModuleVM.OnPropertyChanged(property));
            }

            SetVMProperty(nameof(IsNoUpdateAvailable));

            _launcherModuleVM.OnPropertyChanged(nameof(IsNoUpdateAvailable));
        }
    }
}