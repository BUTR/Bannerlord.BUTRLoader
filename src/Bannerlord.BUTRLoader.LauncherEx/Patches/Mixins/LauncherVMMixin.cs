using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.LauncherEx;
using Bannerlord.BUTRLoader.Options;
using Bannerlord.BUTRLoader.Patches.ViewModels;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Runtime.CompilerServices;

using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherVMMixin : ViewModelMixin<LauncherVM>
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

        private delegate void SetModsIsDisabledOnMultiplayerDelegate(LauncherModsVM instance, bool value);
        private static readonly SetModsIsDisabledOnMultiplayerDelegate? SetModsIsDisabledOnMultiplayer =
            AccessTools2.GetPropertySetterDelegate<SetModsIsDisabledOnMultiplayerDelegate>(typeof(LauncherModsVM), "IsDisabledOnMultiplayer") ??
            AccessTools2.GetPropertySetterDelegate<SetModsIsDisabledOnMultiplayerDelegate>(typeof(LauncherModsVM), "IsDisabled");

        private delegate void SetNewsIsDisabledOnMultiplayerDelegate(LauncherNewsVM instance, bool value);
        private static readonly SetNewsIsDisabledOnMultiplayerDelegate? SetNewsIsDisabledOnMultiplayer =
            AccessTools2.GetPropertySetterDelegate<SetNewsIsDisabledOnMultiplayerDelegate>(typeof(LauncherNewsVM), "IsDisabledOnMultiplayer") ??
            AccessTools2.GetPropertySetterDelegate<SetNewsIsDisabledOnMultiplayerDelegate>(typeof(LauncherNewsVM), "IsDisabled");

        private delegate void SetIsDigitalCompanionDelegate(LauncherVM instance, bool value);
        private static readonly SetIsDigitalCompanionDelegate? SetIsDigitalCompanion =
            AccessTools2.GetPropertySetterDelegate<SetIsDigitalCompanionDelegate>(typeof(LauncherVM), "IsDigitalCompanion");

        private delegate void RefreshDelegate(LauncherVM instance);
        private static readonly RefreshDelegate? Refresh =
            AccessTools2.GetPropertySetterDelegate<RefreshDelegate>(typeof(LauncherVM), "Refresh");

        private enum TopTabs { NONE, Singleplayer, Multiplayer, Options, DigitalCompanion }
        private TopTabs _state;

        public bool IsSingleplayer
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

                    ViewModel.IsSingleplayer = true;
                    ViewModel.IsMultiplayer = false;
                    SetIsDigitalCompanion?.Invoke(ViewModel, false);

                    OnPropertyChanged(nameof(IsOptions));
                    OnPropertyChanged(nameof(IsNotOptions));
                    OnPropertyChanged(nameof(IsNotSingleplayer));
                    OnPropertyChanged(nameof(HideBUTRLoaderVersionText));
                    OnPropertyChanged(nameof(PlayButtonAlignment));
                    OnPropertyChanged(nameof(HasNoMods));
                    OnPropertyChanged(nameof(HasNoNews));

                    RandomImageSwitch = !RandomImageSwitch;

                    ViewModel.News.SetPropertyValue("IsDisabled2", false);
                    ViewModel.ModsData.SetPropertyValue("IsDisabled2", false);
                    OptionsData.IsDisabled = true;
                    Refresh?.Invoke(ViewModel);
                }
            }
        }
        public bool IsNotSingleplayer => !IsSingleplayer;

        public bool IsMultiplayer
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

                    ViewModel.IsMultiplayer = true;
                    ViewModel.IsSingleplayer = false;
                    SetIsDigitalCompanion?.Invoke(ViewModel, false);

                    OnPropertyChanged(nameof(IsOptions));
                    OnPropertyChanged(nameof(IsNotOptions));
                    OnPropertyChanged(nameof(IsNotSingleplayer));
                    OnPropertyChanged(nameof(HideBUTRLoaderVersionText));
                    OnPropertyChanged(nameof(PlayButtonAlignment));
                    OnPropertyChanged(nameof(HasNoMods));
                    OnPropertyChanged(nameof(HasNoNews));

                    RandomImageSwitch = !RandomImageSwitch;

                    ViewModel.News.SetPropertyValue("IsDisabled2", false);
                    ViewModel.ModsData.SetPropertyValue("IsDisabled2", false);
                    OptionsData.IsDisabled = true;
                    Refresh?.Invoke(ViewModel);
                }
            }
        }

        public bool IsOptions
        {
            get => _state == TopTabs.Options;
            set
            {
                if (value && _state != TopTabs.Options && ViewModel is not null)
                {
                    _state = TopTabs.Options;

                    ViewModel.IsSingleplayer = false;
                    ViewModel.IsMultiplayer = false;
                    SetIsDigitalCompanion?.Invoke(ViewModel, false);

                    OnPropertyChanged(nameof(IsOptions));
                    OnPropertyChanged(nameof(IsNotOptions));
                    OnPropertyChanged(nameof(IsNotSingleplayer));
                    OnPropertyChanged(nameof(HideBUTRLoaderVersionText));
                    OnPropertyChanged(nameof(PlayButtonAlignment));
                    OnPropertyChanged(nameof(HasNoMods));
                    OnPropertyChanged(nameof(HasNoNews));

                    RandomImageSwitch = !RandomImageSwitch;

                    ViewModel.News.SetPropertyValue("IsDisabled2", true);
                    ViewModel.ModsData.SetPropertyValue("IsDisabled2", true);
                    Refresh?.Invoke(ViewModel);
                    OptionsData.Refresh(false);
                }
            }
        }
        public bool IsNotOptions => !IsOptions;

        public bool HideBUTRLoaderVersionText => !IsSingleplayer && !IsOptions;

        public bool IsDigitalCompanion
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

                    SetIsDigitalCompanion?.Invoke(ViewModel, true);
                    ViewModel.IsSingleplayer = false;
                    ViewModel.IsMultiplayer = false;

                    OnPropertyChanged(nameof(IsOptions));
                    OnPropertyChanged(nameof(IsNotOptions));
                    OnPropertyChanged(nameof(IsNotSingleplayer));
                    OnPropertyChanged(nameof(HideBUTRLoaderVersionText));
                    OnPropertyChanged(nameof(PlayButtonAlignment));
                    OnPropertyChanged(nameof(HasNoMods));
                    OnPropertyChanged(nameof(HasNoNews));

                    RandomImageSwitch = !RandomImageSwitch;

                    ViewModel.News.SetPropertyValue("IsDisabled2", false);
                    ViewModel.ModsData.SetPropertyValue("IsDisabled2", true);
                    OptionsData.IsDisabled = true;
                    Refresh?.Invoke(ViewModel);
                }
            }
        }

        public HorizontalAlignment PlayButtonAlignment => _state == TopTabs.Singleplayer ? HorizontalAlignment.Right : HorizontalAlignment.Center;

        public bool HasNoMods => !IsSingleplayer && !IsMultiplayer;
        public bool HasNoNews => !IsSingleplayer && !IsMultiplayer && !IsDigitalCompanion;

        public bool RandomImageSwitch { get => _randomImageSwitch; set => SetField(ref _randomImageSwitch, value, nameof(RandomImageSwitch)); }
        private bool _randomImageSwitch;

        public string OptionsText { get => _optionsText; set => SetField(ref _optionsText, value, nameof(OptionsText)); }
        private string _optionsText = "Options";

        public string GeneralText { get => _generalText; set => SetField(ref _generalText, value, nameof(GeneralText)); }
        private string _generalText = "General";

        public string BUTRLoaderVersionText { get => _butrLoaderVersionText; set => SetField(ref _butrLoaderVersionText, value, nameof(BUTRLoaderVersionText)); }
        private string _butrLoaderVersionText = $"BUTRLoader v{typeof(LauncherVMMixin).Assembly.GetName().Version.ToString(3)}";

        public LauncherOptionsVM OptionsData { get => _optionsData; set => SetField(ref _optionsData, value, nameof(OptionsData)); }
        private LauncherOptionsVM _optionsData = new();

        public bool HideRandomImage { get => _hideRandomImage; set => SetField(ref _hideRandomImage, value, nameof(HideRandomImage)); }
        private bool _hideRandomImage;


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


            void SetVMProperty(string property)
            {
                var propertyInfo = new WrappedPropertyInfo(AccessTools2.Property(typeof(LauncherVMMixin), property)!, this);
                launcherVM.AddProperty(property, propertyInfo);
                propertyInfo.PropertyChanged += (_, e) => launcherVM.OnPropertyChanged(e.PropertyName);
            }

            SetVMProperty(nameof(IsSingleplayer));
            SetVMProperty(nameof(IsNotSingleplayer));
            SetVMProperty(nameof(IsMultiplayer));
            SetVMProperty(nameof(IsOptions));
            SetVMProperty(nameof(IsNotOptions));
            SetVMProperty(nameof(HideBUTRLoaderVersionText));
            SetVMProperty(nameof(IsDigitalCompanion));
            SetVMProperty(nameof(PlayButtonAlignment));
            SetVMProperty(nameof(HasNoMods));
            SetVMProperty(nameof(HasNoNews));
            SetVMProperty(nameof(RandomImageSwitch));

            SetVMProperty(nameof(OptionsText));
            SetVMProperty(nameof(GeneralText));

            SetVMProperty(nameof(BUTRLoaderVersionText));

            SetVMProperty(nameof(OptionsData));

            SetVMProperty(nameof(HideRandomImage));

            HideRandomImage = LauncherSettings.HideRandomImage;

            if (launcherVM.IsMultiplayer)
                IsMultiplayer = true;
            else
                IsSingleplayer = true;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void Save()
        {
            LauncherVMPatch.UpdateAndSaveUserModsData(ViewModel, IsMultiplayer);
        }

        // Ensure save is triggered when launching the game
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void ExecuteConfirmUnverifiedDLLStart()
        {
            Save();
            Manager.Disable();
            ExecuteConfirmUnverifiedDLLStartOriginal?.Invoke(ViewModel);
        }


        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void ExecuteBeginHintImport()
        {
            AddHintInformation?.Invoke("Import Mod List");
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void ExecuteBeginHintExport()
        {
            AddHintInformation?.Invoke("Export Current Mod List");
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
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

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void ExecuteImport()
        {
            _currentModuleListHandler = new ModuleListHandler(ViewModel, _userDataManager);
            _currentModuleListHandler.Import();
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void ExecuteExport()
        {
            _currentModuleListHandler = new ModuleListHandler(ViewModel, _userDataManager);
            _currentModuleListHandler.Export();
        }

        private void SaveOptions()
        {
            HideRandomImage = LauncherSettings.HideRandomImage;

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
    }
}