using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Patches.ViewModels;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;
using TaleWorlds.MountAndBlade.Launcher.UserDatas;

using UserDataOld = TaleWorlds.MountAndBlade.Launcher.UserDatas.UserData;
using UserDataOptions = Bannerlord.BUTRLoader.Options.UserData;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherVMMixin
    {
        private delegate void ExecuteConfirmUnverifiedDLLStartDelegate(LauncherVM instance);
        private static readonly ExecuteConfirmUnverifiedDLLStartDelegate? ExecuteConfirmUnverifiedDLLStartOriginal;
        private static readonly AccessTools.FieldRef<LauncherVM, UserDataManager>? UserDataManagerFieldRef =
            AccessTools2.FieldRefAccess<LauncherVM, UserDataManager>("_userDataManager");

        static LauncherVMMixin()
        {
            ExecuteConfirmUnverifiedDLLStartOriginal =
                AccessTools2.GetDelegate<ExecuteConfirmUnverifiedDLLStartDelegate>(typeof(LauncherVM), "ExecuteConfirmUnverifiedDLLStart");
        }

        private enum TopTabs { NONE, Singleplayer, Multiplayer, Options }
        private TopTabs _state;

        public bool IsSingleplayer
        {
            get => _state == TopTabs.Singleplayer;
            set
            {
                if (value && _state != TopTabs.Singleplayer)
                {
                    if (_state == TopTabs.Options)
                    {
                        SaveOptions();
                    }

                    _state = TopTabs.Singleplayer;

                    _launcherVM.IsSingleplayer = true;
                    _launcherVM.OnPropertyChanged(nameof(IsSingleplayer));
                    _launcherVM.OnPropertyChanged(nameof(IsOptions));

                    _launcherVM.OnPropertyChanged(nameof(IsNotSingleplayer));
                    _launcherVM.OnPropertyChanged(nameof(IsNotOptions));
                    _launcherVM.OnPropertyChanged(nameof(PlayButtonAlignment));
                    _launcherVM.OnPropertyChanged(nameof(SkipNews));
                    _launcherVM.OnPropertyChanged(nameof(SkipMods));

                    RandomImageSwitch = !RandomImageSwitch;

                    _launcherVM.ModsData.IsDisabledOnMultiplayer = false;
                    _launcherVM.News.IsDisabledOnMultiplayer = false;
                    OptionsData.IsDisabled = true;
                }
            }
        }
        public bool IsNotSingleplayer => !IsSingleplayer;

        public bool IsMultiplayer
        {
            get => _state == TopTabs.Multiplayer;
            set
            {
                if (value && _state != TopTabs.Multiplayer)
                {
                    if (_state == TopTabs.Options)
                    {
                        SaveOptions();
                    }

                    _state = TopTabs.Multiplayer;

                    _launcherVM.IsMultiplayer = true;
                    _launcherVM.OnPropertyChanged(nameof(IsMultiplayer));
                    _launcherVM.OnPropertyChanged(nameof(IsOptions));

                    _launcherVM.OnPropertyChanged(nameof(IsNotSingleplayer));
                    _launcherVM.OnPropertyChanged(nameof(IsNotOptions));
                    _launcherVM.OnPropertyChanged(nameof(PlayButtonAlignment));
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

                    _launcherVM.OnPropertyChanged(nameof(IsNotSingleplayer));
                    _launcherVM.OnPropertyChanged(nameof(IsNotOptions));
                    _launcherVM.OnPropertyChanged(nameof(PlayButtonAlignment));
                    _launcherVM.OnPropertyChanged(nameof(SkipNews));
                    _launcherVM.OnPropertyChanged(nameof(SkipMods));

                    _launcherVM.News.IsDisabledOnMultiplayer = true;
                    _launcherVM.ModsData.IsDisabledOnMultiplayer = true;
                    OptionsData.Refresh(false);
                }
            }
        }
        public bool IsNotOptions => !IsOptions;

        public HorizontalAlignment PlayButtonAlignment => _state == TopTabs.Singleplayer ? HorizontalAlignment.Right : HorizontalAlignment.Center;

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

#if CONTINUE
        public string Continue
        {
            get => _continue;
            set
            {
                if (value != _continue)
                {
                    _continue = value;
                    _launcherVM.OnPropertyChangedWithValue(value, nameof(Continue));
                }
            }
        }
        private string _continue = "No save found!";
#endif

        private readonly LauncherVM _launcherVM;
        private readonly UserDataManager _userDataManager;

        public LauncherVMMixin(LauncherVM launcherVM)
        {
            _launcherVM = launcherVM;
            _userDataManager = UserDataManagerFieldRef(launcherVM);

            var propsObject = AccessTools.Field(typeof(ViewModel), "_propertyInfos")?.GetValue(_launcherVM) as Dictionary<string, PropertyInfo>
                              ?? new Dictionary<string, PropertyInfo>();

            void SetVMProperty(string property)
            {
                propsObject[property] = new WrappedPropertyInfo(
                    AccessTools.Property(typeof(LauncherVMMixin), property),
                    this,
                    () => _launcherVM.OnPropertyChanged(property));
            }

            SetVMProperty(nameof(IsSingleplayer));
            SetVMProperty(nameof(IsNotSingleplayer));
            SetVMProperty(nameof(IsMultiplayer));
            SetVMProperty(nameof(IsOptions));
            SetVMProperty(nameof(IsNotOptions));
            SetVMProperty(nameof(PlayButtonAlignment));
            SetVMProperty(nameof(SkipNews));
            SetVMProperty(nameof(SkipMods));
            SetVMProperty(nameof(RandomImageSwitch));

            SetVMProperty(nameof(OptionsText));
            SetVMProperty(nameof(GeneralText));

            SetVMProperty(nameof(VersionTextSingleplayer));
            SetVMProperty(nameof(OptionsData));

#if CONTINUE
            SetVMProperty(nameof(Continue));
#endif

            if (_launcherVM.IsMultiplayer)
                IsMultiplayer = true;
            else
                IsSingleplayer = true;

#if CONTINUE
            if (SaveUtils.GetLatestSave() is { } latestSave)
            {
                Continue = $"{latestSave.Name}";
            }
#endif
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void Save()
        {
            LauncherVMPatch.UpdateAndSaveUserModsData(_launcherVM, IsMultiplayer);
        }

        // Ensure save is triggered when launching the game
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void ExecuteConfirmUnverifiedDLLStart()
        {
            Save();
            BUTRLoaderAppDomainManager.UnpatchAll();
            ExecuteConfirmUnverifiedDLLStartOriginal?.Invoke(_launcherVM);
        }

        private void SaveOptions()
        {

            if (_userDataManager.UserData is UserDataOptions userData)
            {
                if (userData.ExtendedSorting != BUTRLoaderAppDomainManager.ExtendedSorting)
                    Save();

                if (userData.AutomaticallyCheckForUpdates != BUTRLoaderAppDomainManager.AutomaticallyCheckForUpdates)
                    Save();

                if (userData.UnblockFiles != BUTRLoaderAppDomainManager.UnblockFiles)
                    Save();

                if (userData.FixCommonIssues != BUTRLoaderAppDomainManager.FixCommonIssues)
                    Save();

                if (userData.CompactModuleList != BUTRLoaderAppDomainManager.CompactModuleList)
                    Save();

                if (userData.ResetModuleList != BUTRLoaderAppDomainManager.ResetModuleList)
                    Save();
            }
        }
    }
}