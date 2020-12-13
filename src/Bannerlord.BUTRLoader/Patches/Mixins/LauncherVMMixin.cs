using Bannerlord.BUTRLoader.Helpers;

using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal class LauncherVMMixin
    {
        public string VersionTextSingleplayer
        {
            get => _versionTextSingleplayer;
            set
            {
                if (value != _versionTextSingleplayer)
                {
                    _versionTextSingleplayer = value;
                    _launcherVM.OnPropertyChangedWithValue(value, nameof(VersionTextSingleplayer));
                }
            }
        }
        private string _versionTextSingleplayer = $"BUTRLoader v{typeof(LauncherVMMixin).Assembly.GetName().Version.ToString(3)}";

        private readonly LauncherVM _launcherVM;
        public LauncherVMMixin(LauncherVM launcherVM)
        {
            _launcherVM = launcherVM;

            var field = typeof(ViewModel).GetField( "_propertyInfos", ReflectionHelper.All);
            var propsObject = field?.GetValue(_launcherVM) as Dictionary<string, PropertyInfo> ?? new Dictionary<string, PropertyInfo>();

            propsObject.Add(nameof(VersionTextSingleplayer), new WrappedPropertyInfo(
                typeof(LauncherVMMixin).GetProperty(nameof(VersionTextSingleplayer), ReflectionHelper.All)!,
                this,
                () => _launcherVM.OnPropertyChanged(nameof(VersionTextSingleplayer))));
        }
    }
}