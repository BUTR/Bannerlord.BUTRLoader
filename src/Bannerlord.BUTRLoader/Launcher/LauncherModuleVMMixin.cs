using Bannerlord.BUTRLoader.Helpers;

using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Launcher
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

            var field = typeof(ViewModel).GetField("_propertyInfos", ReflectionHelper.All);
            var propsObject = field?.GetValue(_launcherModuleVM) as Dictionary<string, PropertyInfo> ??
                              new Dictionary<string, PropertyInfo>();

            propsObject.Add(nameof(IsNoUpdateAvailable), new WrappedPropertyInfo(
                typeof(LauncherModuleVMMixin).GetProperty(nameof(IsNoUpdateAvailable), ReflectionHelper.All)!,
                this,
                () => _launcherModuleVM.OnPropertyChanged(nameof(IsNoUpdateAvailable))));

            _launcherModuleVM.OnPropertyChanged(nameof(IsNoUpdateAvailable));
        }
    }
}