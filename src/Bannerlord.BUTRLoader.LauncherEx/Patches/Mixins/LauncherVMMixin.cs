﻿using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Features.ContinueSaveFile;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.LauncherEx;
using Bannerlord.BUTRLoader.ViewModels;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Text;

using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.BUTRLoader.Patches.Mixins
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
        public string LauncherText { get => _launcherText; set => SetField(ref _launcherText, value, nameof(LauncherText)); }
        private string _launcherText = "Launcher";

        [BUTRDataSourceProperty]
        public string GameText { get => _gameText; set => SetField(ref _gameText, value, nameof(GameText)); }
        private string _gameText = "Game";

        [BUTRDataSourceProperty]
        public string EngineText { get => _engineText; set => SetField(ref _engineText, value, nameof(EngineText)); }
        private string _engineText = "Engine";

        [BUTRDataSourceProperty]
        public string SavesText { get => _savesText; set => SetField(ref _savesText, value, nameof(SavesText)); }
        private string _savesText = "Saves";

        [BUTRDataSourceProperty]
        public string BUTRLoaderVersionText { get => _butrLoaderVersionText; set => SetField(ref _butrLoaderVersionText, value, nameof(BUTRLoaderVersionText)); }
        private string _butrLoaderVersionText = $"BUTRLoader v{typeof(LauncherVMMixin).Assembly.GetName().Version.ToString(3)}";

        [BUTRDataSourceProperty]
        public BUTRLauncherOptionsVM OptionsLauncherData { get => _optionsLauncherData; set => SetField(ref _optionsLauncherData, value, nameof(OptionsLauncherData)); }
        private BUTRLauncherOptionsVM _optionsLauncherData;

        [BUTRDataSourceProperty]
        public BUTRLauncherOptionsVM OptionsGameData { get => _optionsGameData; set => SetField(ref _optionsGameData, value, nameof(OptionsGameData)); }
        private BUTRLauncherOptionsVM _optionsGameData;

        [BUTRDataSourceProperty]
        public BUTRLauncherOptionsVM OptionsEngineData { get => _optionsEngineData; set => SetField(ref _optionsEngineData, value, nameof(OptionsEngineData)); }
        private BUTRLauncherOptionsVM _optionsEngineData;

        [BUTRDataSourceProperty]
        public BUTRLauncherSavesVM? SavesData { get => _savesData; set => SetField(ref _savesData, value, nameof(SavesData)); }
        private BUTRLauncherSavesVM? _savesData;

        [BUTRDataSourceProperty]
        public bool HideRandomImage { get => _hideRandomImage; set => SetField(ref _hideRandomImage, value, nameof(HideRandomImage)); }
        private bool _hideRandomImage;

        [BUTRDataSourceProperty]
        public bool IsModsDataSelected
        {
            get => _isModsDataSelected;
            set
            {
                if (SetField(ref _isModsDataSelected, value, nameof(IsModsDataSelected)))
                {
                    OnPropertyChanged(nameof(IsModsDataNotSelected));
                    OnPropertyChanged(nameof(IsSavesDataSelected));
                    OnPropertyChanged(nameof(IsSavesDataNotSelected));
                }
            }
        }
        private bool _isModsDataSelected;
        [BUTRDataSourceProperty]
        public bool IsModsDataNotSelected => !IsModsDataSelected;

        [BUTRDataSourceProperty]
        public bool IsSavesDataSelected
        {
            get => _isSavesDataSelected;
            set
            {
                if (SetField(ref _isSavesDataSelected, value, nameof(IsSavesDataSelected)))
                {
                    OnPropertyChanged(nameof(IsSavesDataNotSelected));
                    OnPropertyChanged(nameof(ShowPlaySingleplayerButton));
                    OnPropertyChanged(nameof(IsModsDataSelected));
                    OnPropertyChanged(nameof(IsModsDataNotSelected));
                }
            }
        }
        private bool _isSavesDataSelected;
        [BUTRDataSourceProperty]
        public bool IsSavesDataNotSelected => !IsSavesDataSelected;

        [BUTRDataSourceProperty]
        public float ContentTabControlMargin { get => _contentTabControlMargin; set => SetField(ref _contentTabControlMargin, value, nameof(ContentTabControlMargin)); }
        private float _contentTabControlMargin;

        [BUTRDataSourceProperty]
        public bool ShowPlaySingleplayerButton => IsSingleplayer2 && !IsSavesDataSelected;

        private readonly UserDataManager? _userDataManager;

        private ModuleListHandler? _currentModuleListHandler;

        public LauncherVMMixin(LauncherVM launcherVM) : base(launcherVM)
        {
            _userDataManager = UserDataManagerFieldRef?.Invoke(launcherVM);

            _optionsEngineData = new BUTRLauncherOptionsVM(OptionsType.Engine, SaveUserData, RefreshOptions);
            _optionsGameData = new BUTRLauncherOptionsVM(OptionsType.Game, SaveUserData, RefreshOptions);
            _optionsLauncherData = new BUTRLauncherOptionsVM(OptionsType.Launcher, SaveUserData, RefreshOptions);

            if (launcherVM.GetPropertyValue("ModsData") is LauncherModsVM launcherModsVM && launcherModsVM.GetMixin<LauncherModsVMMixin, LauncherModsVM>() is { } mixin)
            {
                _savesData = new BUTRLauncherSavesVM(mixin.GetModuleById, mixin.GetModuleByName);
                mixin.SetGetSelectedSave(() => SavesData?.Selected);
            }

            HideRandomImage = LauncherSettings.HideRandomImage;
            ContentTabControlMargin = LauncherSettings.HideRandomImage ? 5 : 114;

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
            OnPropertyChanged(nameof(IsNotOptions));
            OnPropertyChanged(nameof(IsNotSingleplayer));
            OnPropertyChanged(nameof(HideBUTRLoaderVersionText));
            OnPropertyChanged(nameof(PlayButtonAlignment));
            OnPropertyChanged(nameof(HasNoNews));
            OnPropertyChanged(nameof(HasNoMods));
            OnPropertyChanged(nameof(ShowPlaySingleplayerButton));

            ViewModel.IsSingleplayer = IsSingleplayer2;
            ViewModel.IsMultiplayer = IsMultiplayer2;
            SetIsDigitalCompanion?.Invoke(ViewModel, IsDigitalCompanion2);

            RandomImageSwitch = !RandomImageSwitch;

            ViewModel.News.SetPropertyValue(nameof(LauncherNewsVMMixin.IsDisabled2), HasNoNews);
            ViewModel.ModsData.SetPropertyValue(nameof(LauncherModsVMMixin.IsDisabled2), HasNoMods);
            if (SavesData is not null)
                SavesData.IsDisabled = !IsSingleplayer2;
            OptionsLauncherData.IsDisabled = !IsOptions;
            OptionsGameData.IsDisabled = !IsOptions;
            OptionsEngineData.IsDisabled = !IsOptions;
            if (IsOptions)
            {
                RefreshOptions();
            }
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

            HideRandomImage = LauncherSettings.HideRandomImage;
            ContentTabControlMargin = LauncherSettings.HideRandomImage ? 5 : 114;
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
            Manager.Disable();
            ExecuteConfirmUnverifiedDLLStartOriginal?.Invoke(ViewModel);
        }

        [BUTRDataSourceMethod]
        public void ExecuteBeginHintImport()
        {
            HintManager.ShowHint("Import Load Order");
        }

        [BUTRDataSourceMethod]
        public void ExecuteBeginHintExport()
        {
            HintManager.ShowHint("Export Current Load Order");
        }

        [BUTRDataSourceMethod]
        public void ExecuteEndHint()
        {
            if (_currentModuleListHandler is null)
            {
                HintManager.HideHint();
            }
            else
            {
                _currentModuleListHandler = null;
            }
        }

        [BUTRDataSourceMethod]
        public void ExecuteImport()
        {
            if (ViewModel is null) return;

            _currentModuleListHandler = new ModuleListHandler(ViewModel);
            _currentModuleListHandler.Import();
        }

        [BUTRDataSourceMethod]
        public void ExecuteExport()
        {
            if (ViewModel is null) return;

            _currentModuleListHandler = new ModuleListHandler(ViewModel);
            _currentModuleListHandler.Export();
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