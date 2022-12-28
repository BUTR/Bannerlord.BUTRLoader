using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Features.ContinueSaveFile;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.ViewModels;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Text;

using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.BUTRLoader.Mixins
{
    internal sealed class LauncherVMMixin : ViewModelMixin<LauncherVMMixin, LauncherVM>
    {
        private delegate void ExecuteConfirmUnverifiedDLLStartDelegate(LauncherVM instance);
        private static readonly ExecuteConfirmUnverifiedDLLStartDelegate? ExecuteConfirmUnverifiedDLLStartOriginal =
            AccessTools2.GetDelegate<ExecuteConfirmUnverifiedDLLStartDelegate>(typeof(LauncherVM), "ExecuteConfirmUnverifiedDLLStart");

        private delegate void ExecuteStartGameDelegate(LauncherVM instance, int mode);
        private static readonly ExecuteStartGameDelegate? ExecuteStartGame =
            AccessTools2.GetDelegate<ExecuteStartGameDelegate>(typeof(LauncherVM), "ExecuteStartGame");

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
        public bool IsModsDataSelected
        {
            get => _isModsDataSelected;
            set
            {
                if (SetField(ref _isModsDataSelected, value))
                {
                    OnPropertyChanged(nameof(ShowImportExport));
                }
            }
        }
        private bool _isModsDataSelected;

        [BUTRDataSourceProperty]
        public bool IsSavesDataSelected
        {
            get => _isSavesDataSelected;
            set
            {
                if (SetField(ref _isSavesDataSelected, value))
                {
                    OnPropertyChanged(nameof(ShowPlaySingleplayerButton));
                    OnPropertyChanged(nameof(ShowImportExport));
                }
            }
        }
        private bool _isSavesDataSelected;

        [BUTRDataSourceProperty]
        public HorizontalAlignment PlayButtonAlignment => _state == TopTabs.Singleplayer ? HorizontalAlignment.Right : HorizontalAlignment.Center;

        [BUTRDataSourceProperty]
        public bool RandomImageSwitch { get => _randomImageSwitch; set => SetField(ref _randomImageSwitch, value); }
        private bool _randomImageSwitch;

        [BUTRDataSourceProperty]
        public string OptionsText { get => _optionsText; set => SetField(ref _optionsText, value); }
        private string _optionsText = "Options";

        [BUTRDataSourceProperty]
        public string LauncherText { get => _launcherText; set => SetField(ref _launcherText, value); }
        private string _launcherText = "Launcher";

        [BUTRDataSourceProperty]
        public string GameText { get => _gameText; set => SetField(ref _gameText, value); }
        private string _gameText = "Game";

        [BUTRDataSourceProperty]
        public string EngineText { get => _engineText; set => SetField(ref _engineText, value); }
        private string _engineText = "Engine";

        [BUTRDataSourceProperty]
        public string SavesText { get => _savesText; set => SetField(ref _savesText, value); }
        private string _savesText = "Saves";

        [BUTRDataSourceProperty]
        public string BUTRLoaderVersionText { get => _butrLoaderVersionText; set => SetField(ref _butrLoaderVersionText, value); }
        private string _butrLoaderVersionText = $"BUTRLoader v{typeof(LauncherVMMixin).Assembly.GetName().Version.ToString(3)}";

        [BUTRDataSourceProperty]
        public BUTRLauncherOptionsVM OptionsLauncherData { get => _optionsLauncherData; set => SetField(ref _optionsLauncherData, value); }
        private BUTRLauncherOptionsVM _optionsLauncherData;

        [BUTRDataSourceProperty]
        public BUTRLauncherOptionsVM OptionsGameData { get => _optionsGameData; set => SetField(ref _optionsGameData, value); }
        private BUTRLauncherOptionsVM _optionsGameData;

        [BUTRDataSourceProperty]
        public BUTRLauncherOptionsVM OptionsEngineData { get => _optionsEngineData; set => SetField(ref _optionsEngineData, value); }
        private BUTRLauncherOptionsVM _optionsEngineData;

        [BUTRDataSourceProperty]
        public BUTRLauncherSavesVM? SavesData { get => _savesData; set => SetField(ref _savesData, value); }
        private BUTRLauncherSavesVM? _savesData;

        [BUTRDataSourceProperty]
        public bool ShowMods => IsSingleplayer2 || IsMultiplayer2;
        [BUTRDataSourceProperty]
        public bool ShowNews => IsSingleplayer2 || IsMultiplayer2 || IsDigitalCompanion2;

        [BUTRDataSourceProperty]
        public bool ShowRandomImage { get => _showRandomImage; set => SetField(ref _showRandomImage, value); }
        private bool _showRandomImage;

        [BUTRDataSourceProperty]
        public bool ShowImportExport => IsModsDataSelected || (IsSavesDataSelected && SavesData?.Selected is not null);

        [BUTRDataSourceProperty]
        public bool ShowBUTRLoaderVersionText => IsSingleplayer2 || IsOptions;

        [BUTRDataSourceProperty]
        public bool ShowPlaySingleplayerButton => IsSingleplayer2 && !IsSavesDataSelected;

        [BUTRDataSourceProperty]
        public float ContentTabControlMarginRight { get => _contentTabControlMarginRight; set => SetField(ref _contentTabControlMarginRight, value); }
        private float _contentTabControlMarginRight = 0;

        [BUTRDataSourceProperty]
        public float ContentTabControlMarginBottom { get => _contentTabControlMarginBottom; set => SetField(ref _contentTabControlMarginBottom, value); }
        private float _contentTabControlMarginBottom = 114;

        [BUTRDataSourceProperty]
        public float BUTRLoaderVersionMarginBottom { get => _butrLoaderVersionMarginBottom; set => SetField(ref _butrLoaderVersionMarginBottom, value); }
        private float _butrLoaderVersionMarginBottom = 90;

        [BUTRDataSourceProperty]
        public float DividerMarginBottom { get => _dividerMarginBottom; set => SetField(ref _dividerMarginBottom, value); }
        private float _dividerMarginBottom = 113;

        [BUTRDataSourceProperty]
        public float BackgroundHeight { get => _backgroundHeight; set => SetField(ref _backgroundHeight, value); }
        private float _backgroundHeight = 581; // 700

        private readonly UserDataManager? _userDataManager;

        private ModuleListHandler? _currentModuleListHandler;

        public LauncherVMMixin(LauncherVM launcherVM) : base(launcherVM)
        {
            _userDataManager = UserDataManagerFieldRef?.Invoke(launcherVM);

            _optionsEngineData = new BUTRLauncherOptionsVM(OptionsType.Engine, SaveUserData, RefreshOptions);
            _optionsGameData = new BUTRLauncherOptionsVM(OptionsType.Game, SaveUserData, RefreshOptions);
            _optionsLauncherData = new BUTRLauncherOptionsVM(OptionsType.Launcher, SaveUserData, RefreshOptions);

            if (launcherVM.GetPropertyValue(nameof(LauncherVM.ModsData)) is LauncherModsVM lmvm && lmvm.GetMixin<BUTRLoader.Mixins.LauncherModsVMMixin, LauncherModsVM>() is { } mixin)
            {
                _savesData = new BUTRLauncherSavesVM(mixin.GetModuleById, mixin.GetModuleByName);
                _savesData.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "SaveSelected")
                        OnPropertyChanged(nameof(ShowImportExport));
                };
                mixin.SetGetSelectedSave(() => SavesData?.Selected);
            }

            ShowRandomImage = !LauncherSettings.HideRandomImage;
            ContentTabControlMarginRight = LauncherSettings.HideRandomImage ? 5 : 114;
            BackgroundHeight = LauncherSettings.BigMode ? 700 : 581;

            IsMultiplayer2 = launcherVM.IsMultiplayer;
            IsSingleplayer2 = launcherVM.IsSingleplayer;
            IsDigitalCompanion2 = (bool?) launcherVM.GetPropertyValue("IsDigitalCompanion") ?? false;

            Refresh?.Invoke(launcherVM);
        }

        private void SetState()
        {
            if (ViewModel is null) return;

            OnPropertyChanged(nameof(IsSingleplayer2));
            OnPropertyChanged(nameof(IsMultiplayer2));
            OnPropertyChanged(nameof(IsOptions));
            OnPropertyChanged(nameof(IsDigitalCompanion2));
            OnPropertyChanged(nameof(ShowBUTRLoaderVersionText));
            OnPropertyChanged(nameof(PlayButtonAlignment));
            OnPropertyChanged(nameof(ShowNews));
            OnPropertyChanged(nameof(ShowMods));
            OnPropertyChanged(nameof(ShowPlaySingleplayerButton));

            ViewModel.IsSingleplayer = IsSingleplayer2;
            ViewModel.IsMultiplayer = IsMultiplayer2;
            SetIsDigitalCompanion?.Invoke(ViewModel, IsDigitalCompanion2);

            RandomImageSwitch = !RandomImageSwitch;

            ViewModel.News.SetPropertyValue(nameof(LauncherNewsVMMixin.IsDisabled2), !ShowNews);
            ViewModel.ModsData.SetPropertyValue(nameof(LauncherModsVMMixin.IsDisabled2), !ShowMods);
            if (SavesData is not null)
                SavesData.IsDisabled = !IsSingleplayer2;
            OptionsLauncherData.IsDisabled = !IsOptions;
            OptionsGameData.IsDisabled = !IsOptions;
            OptionsEngineData.IsDisabled = !IsOptions;
            if (IsOptions)
                RefreshOptions();

            ContentTabControlMarginBottom = IsOptions ? 50 : 114;
            BUTRLoaderVersionMarginBottom = IsOptions ? 30 : 90;
            DividerMarginBottom = IsOptions ? 49 : 113;
        }

        public void RefreshOptions()
        {
            OptionsLauncherData.Refresh();
            OptionsGameData.Refresh();
            OptionsEngineData.Refresh();
        }

        public void SaveUserData()
        {
            if (ViewModel is null) return;

            ShowRandomImage = !LauncherSettings.HideRandomImage;
            ContentTabControlMarginRight = LauncherSettings.HideRandomImage ? 5 : 114;
            BackgroundHeight = LauncherSettings.BigMode ? 700 : 581;
            UpdateAndSaveUserModsDataMethod?.Invoke(ViewModel, IsMultiplayer2);
        }

        public void SaveOptions()
        {
            OptionsLauncherData.Save();
            OptionsGameData.Save();
            OptionsEngineData.Save();
        }

        public void UpdateAndSaveUserModsData(bool isMultiplayer)
        {
            if (_userDataManager is null || ViewModel?.ModsData.GetModules() is not { } modules)
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

        // Ensure save is triggered when launching the game
        [BUTRDataSourceMethod]
        public void ExecuteConfirmUnverifiedDLLStart()
        {
            if (ViewModel is null) return;

            SaveUserData();
            ExecuteConfirmUnverifiedDLLStartOriginal?.Invoke(ViewModel);
        }

        [BUTRDataSourceMethod]
        public void ExecuteBeginHintImport()
        {
            if (IsModsDataSelected)
            {
                HintManager.ShowHint("Import Load Order");
            }
            if (IsSavesDataSelected)
            {
                HintManager.ShowHint("Import Save's Load Order");
            }
        }

        [BUTRDataSourceMethod]
        public void ExecuteBeginHintExport()
        {
            if (IsModsDataSelected)
            {
                HintManager.ShowHint("Export Current Load Order");
            }
            if (IsSavesDataSelected)
            {
                HintManager.ShowHint("Export Save's Load Order");
            }
        }

        [BUTRDataSourceMethod]
        public void ExecuteEndHint()
        {
            HintManager.HideHint();
        }

        [BUTRDataSourceMethod]
        public void ExecuteImport()
        {
            if (ViewModel is null) return;

            _currentModuleListHandler = new ModuleListHandler(ViewModel);
            if (IsModsDataSelected)
            {
                _currentModuleListHandler.Import();
            }
            if (IsSavesDataSelected && SavesData?.Selected?.Name is { } saveName)
            {
                _currentModuleListHandler.ImportSaveFile(saveName);
            }
        }

        [BUTRDataSourceMethod]
        public void ExecuteExport()
        {
            if (ViewModel is null) return;

            _currentModuleListHandler = new ModuleListHandler(ViewModel);
            if (IsModsDataSelected)
            {
                _currentModuleListHandler.Export();
            }
            if (IsSavesDataSelected && SavesData?.Selected?.Name is { } saveName)
            {
                _currentModuleListHandler.ExportSaveFile(saveName);
            }
        }

        [BUTRDataSourceMethod(OverrideName = "ExecuteStartGame")]
        public void ExecuteStartGameOverride(int mode)
        {
            if (ViewModel is null || ExecuteStartGame is null) return;

            if (IsSavesDataSelected && SavesData?.Selected is { } saveVM)
            {
                ContinueSaveFileFeature.SetCurrentSaveFile(saveVM.Name);
                if (saveVM.HasWarning || saveVM.HasError)
                {
                    var description = new StringBuilder();
                    if (saveVM.HasError)
                    {
                        description.Append(saveVM.ErrorHint?.Text ?? string.Empty);
                    }

                    if (saveVM is { HasError: true, HasWarning: true })
                    {
                        description.Append("\n");
                    }

                    if (saveVM.HasWarning)
                    {
                        description.Append(saveVM.WarningHint?.Text ?? string.Empty);
                    }

                    description.Append("\n\n");
                    description.Append("An unstable experience could occur.\n");
                    description.Append("Do you wish to continue loading the save?");

                    ViewModel.ConfirmStart = new LauncherConfirmStartVM(() => ExecuteStartGame(ViewModel, 0))
                    {
                        Title = "WARNING",
                        Description = description.ToString(),
                        IsEnabled = true
                    };
                    return;
                }

                ExecuteStartGame(ViewModel, 0);
                return;
            }

            ExecuteStartGame(ViewModel, mode);
        }
    }
}