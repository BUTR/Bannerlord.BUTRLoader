using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Patches.ViewModels;

using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal class LauncherVMMixin
    {
        private enum TopTabs
        {
            NONE,
            Singleplayer,
            Multiplayer,
            Options
        }

        private TopTabs _state;

        public bool IsSingleplayer
        {
            get => _state == TopTabs.Singleplayer;
            set
            {
                if (value && _state != TopTabs.Singleplayer)
                {
                    _state = TopTabs.Singleplayer;

                    _launcherVM.IsSingleplayer = true;
                    _launcherVM.OnPropertyChanged(nameof(IsSingleplayer));
                    _launcherVM.OnPropertyChanged(nameof(IsOptions));

                    _launcherVM.OnPropertyChanged(nameof(IsNotOptions));
                    _launcherVM.OnPropertyChanged(nameof(SkipNews));
                    _launcherVM.OnPropertyChanged(nameof(SkipMods));

                    RandomImageSwitch = !RandomImageSwitch;

                    _launcherVM.ModsData.IsDisabledOnMultiplayer = false;
                    _launcherVM.News.IsDisabledOnMultiplayer = false;
                    OptionsData.IsDisabled = true;
                }
            }
        }

        public bool IsMultiplayer
        {
            get => _state == TopTabs.Multiplayer;
            set
            {
                if (value && _state != TopTabs.Multiplayer)
                {
                    _state = TopTabs.Multiplayer;

                    _launcherVM.IsMultiplayer = true;
                    _launcherVM.OnPropertyChanged(nameof(IsMultiplayer));
                    _launcherVM.OnPropertyChanged(nameof(IsOptions));

                    _launcherVM.OnPropertyChanged(nameof(IsNotOptions));
                    _launcherVM.OnPropertyChanged(nameof(SkipNews));
                    _launcherVM.OnPropertyChanged(nameof(SkipMods));

                    RandomImageSwitch = !RandomImageSwitch;

                    _launcherVM.ModsData.IsDisabledOnMultiplayer = true;
                    _launcherVM.News.IsDisabledOnMultiplayer = false;
                    OptionsData.IsDisabled = true;
                }
            }
        }

        public bool IsOptions
        {
            get => _state == TopTabs.Options;
            set
            {
                if (value && _state != TopTabs.Options)
                {
                    _state = TopTabs.Options;

                    _launcherVM.OnPropertyChangedWithValue(value, nameof(IsOptions));
                    //_launcherVM.OnPropertyChanged(nameof(IsOptions));
                    _launcherVM.OnPropertyChanged(nameof(IsMultiplayer));

                    RandomImageSwitch = !RandomImageSwitch;

                    _launcherVM.OnPropertyChanged(nameof(IsNotOptions));
                    _launcherVM.OnPropertyChanged(nameof(SkipNews));
                    _launcherVM.OnPropertyChanged(nameof(SkipMods));

                    _launcherVM.News.IsDisabledOnMultiplayer = true;
                    _launcherVM.ModsData.IsDisabledOnMultiplayer = true;
                    OptionsData.Refresh(false);
                }
            }
        }
        public bool IsNotOptions => !IsOptions;

        public bool SkipNews => !IsSingleplayer && !IsMultiplayer;
        public bool SkipMods => !IsSingleplayer;

        public bool RandomImageSwitch
        {
            get => _randomImageSwitch;
            set
            {
                if (value != _randomImageSwitch)
                {
                    _randomImageSwitch = value;
                    _launcherVM.OnPropertyChangedWithValue(value, nameof(RandomImageSwitch));
                }
            }
        }
        private bool _randomImageSwitch;

        public string OptionsText
        {
            get => _optionsText;
            set
            {
                if (value != _optionsText)
                {
                    _optionsText = value;
                    _launcherVM.OnPropertyChangedWithValue(value, nameof(OptionsText));
                }
            }
        }
        private string _optionsText = "Options";

        public string GeneralText
        {
            get => _generalText;
            set
            {
                if (value != _generalText)
                {
                    _generalText = value;
                    _launcherVM.OnPropertyChangedWithValue(value, nameof(GeneralText));
                }
            }
        }
        private string _generalText = "General";

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

        public LauncherOptionsVM OptionsData
        {
            get => _optionsData;
            set
            {
                if (value != _optionsData)
                {
                    _optionsData = value;
                    _launcherVM.OnPropertyChangedWithValue(value, nameof(OptionsData));
                }
            }
        }
        private LauncherOptionsVM _optionsData = new ();

        private readonly LauncherVM _launcherVM;

        public LauncherVMMixin(LauncherVM launcherVM)
        {
            _launcherVM = launcherVM;

            var field = typeof(ViewModel).GetField( "_propertyInfos", ReflectionHelper.All);
            var propsObject = field?.GetValue(_launcherVM) as Dictionary<string, PropertyInfo> ?? new Dictionary<string, PropertyInfo>();

            void SetVMProperty(string property)
            {
                propsObject[property] = new WrappedPropertyInfo(
                    typeof(LauncherVMMixin).GetProperty(property, ReflectionHelper.All)!,
                    this,
                    () => _launcherVM.OnPropertyChanged(property));
            }

            SetVMProperty(nameof(IsSingleplayer));
            SetVMProperty(nameof(IsMultiplayer));
            SetVMProperty(nameof(IsOptions));
            SetVMProperty(nameof(IsNotOptions));
            SetVMProperty(nameof(SkipNews));
            SetVMProperty(nameof(SkipMods));
            SetVMProperty(nameof(RandomImageSwitch));

            SetVMProperty(nameof(OptionsText));
            SetVMProperty(nameof(GeneralText));

            SetVMProperty(nameof(VersionTextSingleplayer));
            SetVMProperty(nameof(OptionsData));

            if (_launcherVM.IsMultiplayer)
                IsMultiplayer = true;
            else
                IsSingleplayer = true;
        }
    }
}