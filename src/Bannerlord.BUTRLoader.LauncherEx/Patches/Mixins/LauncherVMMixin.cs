using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.LauncherEx;
using Bannerlord.BUTRLoader.Options;
using Bannerlord.BUTRLoader.Patches.ViewModels;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherVMMixin : ViewModelMixin<LauncherVMMixin, LauncherVM>
    {
        private delegate void AddHintInformationDelegate(string message);
        private static readonly AddHintInformationDelegate? AddHintInformation =
            AccessTools2.GetDelegate<AddHintInformationDelegate>(typeof(LauncherUI), "AddHintInformation");

        private delegate void HideHintInformationDelegate();
        private static readonly HideHintInformationDelegate? HideHintInformation =
            AccessTools2.GetDelegate<HideHintInformationDelegate>(typeof(LauncherUI), "HideHintInformation");

        private delegate void ExecuteConfirmUnverifiedDLLStartDelegate(object instance);
        private static readonly ExecuteConfirmUnverifiedDLLStartDelegate? ExecuteConfirmUnverifiedDLLStartOriginal =
            AccessTools2.GetDelegate<ExecuteConfirmUnverifiedDLLStartDelegate>(typeof(LauncherVM), "ExecuteConfirmUnverifiedDLLStart");

        private static readonly AccessTools.FieldRef<LauncherVM, UserDataManager>? UserDataManagerFieldRef =
            AccessTools2.FieldRefAccess<LauncherVM, UserDataManager>("_userDataManager");
        
        private delegate void SetIsDigitalCompanionDelegate(LauncherVM instance, bool value);
        private static readonly SetIsDigitalCompanionDelegate? SetIsDigitalCompanion =
            AccessTools2.GetPropertySetterDelegate<SetIsDigitalCompanionDelegate>(typeof(LauncherVM), "IsDigitalCompanion");

        private delegate void UpdateAndSaveUserModsDataDelegate(LauncherVM instance, bool isMultiplayer);
        private static readonly UpdateAndSaveUserModsDataDelegate? UpdateAndSaveUserModsDataMethod =
            AccessTools2.GetDelegate<UpdateAndSaveUserModsDataDelegate>(typeof(LauncherVM), "UpdateAndSaveUserModsData");

        private delegate void RefreshDelegate(LauncherVM instance);
        private static readonly RefreshDelegate? Refresh =
            AccessTools2.GetDelegate<RefreshDelegate>(typeof(LauncherVM), "Refresh");


        private enum TopTabs { NONE, Singleplayer, Multiplayer, Options, DigitalCompanion }
        private TopTabs _state;

        [BUTRDataSourceProperty]
        public bool IsSingleplayer2
        {
            get => _state == TopTabs.Singleplayer;
            set
            {
                if (value && _state != TopTabs.Singleplayer && ViewModel is not null)
                {
                    if (_state == TopTabs.Options)
                    {
                        SaveOptions();
                    }

                    _state = TopTabs.Singleplayer;

                    SetState();
                }
            }
        }
        [BUTRDataSourceProperty]
        public bool IsNotSingleplayer => !IsSingleplayer2;

        [BUTRDataSourceProperty]
        public bool IsMultiplayer2
        {
            get => _state == TopTabs.Multiplayer;
            set
            {
                if (value && _state != TopTabs.Multiplayer && ViewModel is not null)
                {
                    if (_state == TopTabs.Options)
                    {
                        SaveOptions();
                    }

                    _state = TopTabs.Multiplayer;

                    SetState();
                }
            }
        }

        [BUTRDataSourceProperty]
        public bool IsOptions
        {
            get => _state == TopTabs.Options;
            set
            {
                if (value && _state != TopTabs.Options && ViewModel is not null)
                {
                    _state = TopTabs.Options;

                    SetState();
                }
            }
        }
        [BUTRDataSourceProperty]
        public bool IsNotOptions => !IsOptions;

        [BUTRDataSourceProperty]
        public bool HideBUTRLoaderVersionText => !IsSingleplayer2 && !IsOptions;

        [BUTRDataSourceProperty]
        public bool IsDigitalCompanion2
        {
            get => _state == TopTabs.DigitalCompanion;
            set
            {
                if (value && _state != TopTabs.DigitalCompanion && ViewModel is not null)
                {
                    if (_state == TopTabs.Options)
                    {
                        SaveOptions();
                    }

                    _state = TopTabs.DigitalCompanion;

                    SetState();
                }
            }
        }

        [BUTRDataSourceProperty]
        public HorizontalAlignment PlayButtonAlignment => _state == TopTabs.Singleplayer ? HorizontalAlignment.Right : HorizontalAlignment.Center;

        [BUTRDataSourceProperty]
        public bool HasNoMods => !IsSingleplayer2 && !IsMultiplayer2;
        [BUTRDataSourceProperty]
        public bool HasNoNews => !IsSingleplayer2 && !IsMultiplayer2 && !IsDigitalCompanion2;

        [BUTRDataSourceProperty]
        public bool RandomImageSwitch { get => _randomImageSwitch; set => SetField(ref _randomImageSwitch, value, nameof(RandomImageSwitch)); }
        private bool _randomImageSwitch;

        [BUTRDataSourceProperty]
        public string OptionsText { get => _optionsText; set => SetField(ref _optionsText, value, nameof(OptionsText)); }
        private string _optionsText = "Options";

        [BUTRDataSourceProperty]
        public string GeneralText { get => _generalText; set => SetField(ref _generalText, value, nameof(GeneralText)); }
        private string _generalText = "General";

        [BUTRDataSourceProperty]
        public string BUTRLoaderVersionText { get => _butrLoaderVersionText; set => SetField(ref _butrLoaderVersionText, value, nameof(BUTRLoaderVersionText)); }
        private string _butrLoaderVersionText = $"BUTRLoader v{typeof(LauncherVMMixin).Assembly.GetName().Version.ToString(3)}";

        [BUTRDataSourceProperty]
        public BUTRLauncherOptionsVM OptionsData { get => _optionsData; set => SetField(ref _optionsData, value, nameof(OptionsData)); }
        private BUTRLauncherOptionsVM _optionsData = new();

        [BUTRDataSourceProperty]
        public bool HideRandomImage { get => _hideRandomImage; set => SetField(ref _hideRandomImage, value, nameof(HideRandomImage)); }
        private bool _hideRandomImage;

        [BUTRDataSourceProperty]
        public float ContentTabControlMargin { get => _contentTabControlMargin; set => SetField(ref _contentTabControlMargin, value, nameof(ContentTabControlMargin)); }
        private float _contentTabControlMargin;

        private readonly UserDataManager _userDataManager;
        private readonly LauncherExData _launcherExData;

        private ModuleListHandler? _currentModuleListHandler;

        public LauncherVMMixin(LauncherVM launcherVM) : base(launcherVM)
        {
            _userDataManager = UserDataManagerFieldRef is not null ? UserDataManagerFieldRef(launcherVM) : default!;
            _launcherExData = new LauncherExData(
                LauncherSettings.ExtendedSorting,
                LauncherSettings.AutomaticallyCheckForUpdates,
                LauncherSettings.UnblockFiles,
                LauncherSettings.FixCommonIssues,
                LauncherSettings.CompactModuleList,
                LauncherSettings.HideRandomImage,
                LauncherSettings.DisableBinaryCheck);

            HideRandomImage = LauncherSettings.HideRandomImage;
            ContentTabControlMargin = LauncherSettings.HideRandomImage ? 5 : 114;

            IsMultiplayer2 = launcherVM.IsMultiplayer;
            IsSingleplayer2 = launcherVM.IsSingleplayer;
            IsDigitalCompanion2 = (bool?) launcherVM.GetPropertyValue("IsDigitalCompanion") ?? false;

            Refresh?.Invoke(ViewModel);
        }

        private void SetState()
        {
            OnPropertyChanged(nameof(IsSingleplayer2));
            OnPropertyChanged(nameof(IsMultiplayer2));
            OnPropertyChanged(nameof(IsOptions));
            OnPropertyChanged(nameof(IsDigitalCompanion2));
            OnPropertyChanged(nameof(IsNotOptions));
            OnPropertyChanged(nameof(IsNotSingleplayer));
            OnPropertyChanged(nameof(HideBUTRLoaderVersionText));
            OnPropertyChanged(nameof(PlayButtonAlignment));
            OnPropertyChanged(nameof(HasNoNews));
            OnPropertyChanged(nameof(HasNoMods));

            ViewModel.IsSingleplayer = IsSingleplayer2;
            ViewModel.IsMultiplayer = IsMultiplayer2;
            SetIsDigitalCompanion?.Invoke(ViewModel, IsDigitalCompanion2);

            RandomImageSwitch = !RandomImageSwitch;
                    
            ViewModel.News.SetPropertyValue(nameof(LauncherNewsVMMixin.IsDisabled2), HasNoNews);
            ViewModel.ModsData.SetPropertyValue(nameof(LauncherModsVMMixin.IsDisabled2), HasNoMods);
            OptionsData.IsDisabled = !IsOptions;
            if (IsOptions)
                OptionsData.Refresh();
        }

        private void Save()
        {
            UpdateAndSaveUserModsDataMethod?.Invoke(ViewModel, IsMultiplayer2);
        }

        // Ensure save is triggered when launching the game
        [BUTRDataSourceMethod]
        public void ExecuteConfirmUnverifiedDLLStart()
        {
            Save();
            Manager.Disable();
            ExecuteConfirmUnverifiedDLLStartOriginal?.Invoke(ViewModel);
        }


        [BUTRDataSourceMethod]
        public void ExecuteBeginHintImport()
        {
            AddHintInformation?.Invoke("Import Mod List");
        }

        [BUTRDataSourceMethod]
        public void ExecuteBeginHintExport()
        {
            AddHintInformation?.Invoke("Export Current Mod List");
        }

        [BUTRDataSourceMethod]
        public void ExecuteEndHint()
        {
            if (_currentModuleListHandler is null)
            {
                HideHintInformation?.Invoke();
            }
            else
            {
                _currentModuleListHandler = null;
            }
        }

        [BUTRDataSourceMethod]
        public void ExecuteImport()
        {
            _currentModuleListHandler = new ModuleListHandler(ViewModel, _userDataManager);
            _currentModuleListHandler.Import();
        }

        [BUTRDataSourceMethod]
        public void ExecuteExport()
        {
            _currentModuleListHandler = new ModuleListHandler(ViewModel, _userDataManager);
            _currentModuleListHandler.Export();
        }

        private void SaveOptions()
        {
            HideRandomImage = LauncherSettings.HideRandomImage;
            ContentTabControlMargin = LauncherSettings.HideRandomImage ? 5 : 114;

            if (_launcherExData.ExtendedSorting != LauncherSettings.ExtendedSorting)
            {
                Save();
                return;
            }

            if (_launcherExData.AutomaticallyCheckForUpdates != LauncherSettings.AutomaticallyCheckForUpdates)
            {
                Save();
                return;
            }

            if (_launcherExData.UnblockFiles != LauncherSettings.UnblockFiles)
            {
                Save();
                return;
            }

            if (_launcherExData.FixCommonIssues != LauncherSettings.FixCommonIssues)
            {
                Save();
                return;
            }

            if (_launcherExData.CompactModuleList != LauncherSettings.CompactModuleList)
            {
                Save();
                return;
            }

            if (_launcherExData.HideRandomImage != LauncherSettings.HideRandomImage)
            {
                Save();
                return;
            }

            if (_launcherExData.DisableBinaryCheck != LauncherSettings.DisableBinaryCheck)
            {
                Save();
                return;
            }
        }

        public void UpdateAndSaveUserModsData(bool isMultiplayer)
        {
            if (ViewModel?.ModsData.GetPropertyValue(nameof(LauncherModsVMMixin.Modules2)) is not MBBindingList<BUTRLauncherModuleVM> modules)
                return;

            if (_userDataManager.UserData.GameType == GameType.Singleplayer && isMultiplayer)
                return;
            if (_userDataManager.UserData.GameType == GameType.Multiplayer && !isMultiplayer)
                return;

            var userData = _userDataManager.UserData;
            var userGameTypeData = isMultiplayer ? userData.MultiplayerData : userData.SingleplayerData;
            userGameTypeData.ModDatas.Clear();
            foreach (var moduleVM in modules)
            {
                userGameTypeData.ModDatas.Add(new UserModData
                {
                    Id = moduleVM.ModuleInfoExtended.Id,
                    IsSelected = moduleVM.IsSelected,
                });
            }
            _userDataManager.SaveUserData();
        }
    }
}