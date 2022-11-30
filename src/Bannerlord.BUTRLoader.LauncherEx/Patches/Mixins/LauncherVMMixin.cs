﻿using Bannerlord.BUTR.Shared.Utils;
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
    internal sealed class LauncherVMMixin
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
        private LauncherOptionsVM _optionsData = new();


        private readonly LauncherVM _launcherVM;
        private readonly UserDataManager _userDataManager;
        private readonly LauncherExData _launcherExData;

        private ModuleListHandler? _currentModuleListHandler;

        public LauncherVMMixin(LauncherVM launcherVM)
        {
            _launcherVM = launcherVM;
            _userDataManager = UserDataManagerFieldRef is not null ? UserDataManagerFieldRef(launcherVM) : default!;
            _launcherExData = new LauncherExData(
                LauncherSettings.ExtendedSorting,
                LauncherSettings.AutomaticallyCheckForUpdates,
                LauncherSettings.UnblockFiles,
                LauncherSettings.FixCommonIssues,
                LauncherSettings.CompactModuleList,
                LauncherSettings.ResetModuleList,
                LauncherSettings.DisableBinaryCheck);

            void SetVMProperty(string property)
            {
                var propertyInfo = new WrappedPropertyInfo(AccessTools2.Property(typeof(LauncherVMMixin), property)!, this);
                _launcherVM.AddProperty(property, propertyInfo);
                propertyInfo.PropertyChanged += (_, e) => _launcherVM.OnPropertyChanged(e.PropertyName);
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

            if (_launcherVM.IsMultiplayer)
                IsMultiplayer = true;
            else
                IsSingleplayer = true;
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
            Manager.Disable();
            ExecuteConfirmUnverifiedDLLStartOriginal?.Invoke(_launcherVM);
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
            _currentModuleListHandler = new ModuleListHandler(_launcherVM, _userDataManager);
            _currentModuleListHandler.Import();
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void ExecuteExport()
        {
            _currentModuleListHandler = new ModuleListHandler(_launcherVM, _userDataManager);
            _currentModuleListHandler.Export();
        }

        private void SaveOptions()
        {
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

            if (_launcherExData.ResetModuleList != LauncherSettings.ResetModuleList)
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