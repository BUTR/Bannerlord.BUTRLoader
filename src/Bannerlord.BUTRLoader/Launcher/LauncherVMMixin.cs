using Bannerlord.BUTRLoader.Helpers;

using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Launcher
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
        private string _versionTextSingleplayer = string.Empty;

        public string VersionTextMultiplayer
        {
            get => _versionTextMultiplayer;
            set
            {
                if (value != _versionTextMultiplayer)
                {
                    _versionTextMultiplayer = value;
                    _launcherVM.OnPropertyChangedWithValue(value, nameof(VersionTextMultiplayer));
                }
            }
        }
        private string _versionTextMultiplayer = string.Empty;

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

            propsObject.Add(nameof(VersionTextMultiplayer), new WrappedPropertyInfo(
                typeof(LauncherVMMixin).GetProperty(nameof(VersionTextMultiplayer), ReflectionHelper.All)!,
                this,
                () => _launcherVM.OnPropertyChanged(nameof(VersionTextMultiplayer))));

            _launcherVM.PropertyChangedWithValue += LauncherVM2_PropertyChangedWithValue;
            LauncherVM2_PropertyChangedWithValue(this, new PropertyChangedWithValueEventArgs(nameof(LauncherVM.VersionText), _launcherVM.VersionText));
        }

        private void LauncherVM2_PropertyChangedWithValue(object sender, PropertyChangedWithValueEventArgs e)
        {
            if (e.PropertyName == nameof(LauncherVM.VersionText) && e.Value is string str)
            {
                VersionTextSingleplayer = _launcherVM.IsMultiplayer ? VersionTextSingleplayer : str + " BUTRLoader";
                VersionTextMultiplayer = _launcherVM.IsMultiplayer ? str : VersionTextMultiplayer;
            }
        }
    }
}