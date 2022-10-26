﻿using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Extensions;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherModsVMMixin
    {
        private delegate void ExecuteSelectDelegate(object instance);
        private static readonly ExecuteSelectDelegate? ExecuteSelect =
            AccessTools2.GetDelegate<ExecuteSelectDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM:ExecuteSelect");

        private delegate void ExecuteLoadSubModulesDelegate(object instance, bool isMultiplayer);
        private static readonly ExecuteLoadSubModulesDelegate? LoadSubModules =
            AccessTools2.GetDelegate<ExecuteLoadSubModulesDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModsVM:LoadSubModules");

        private delegate void SaveUserDataDelegate(object instance);
        private static readonly SaveUserDataDelegate? SaveUserData =
            AccessTools2.GetDelegate<SaveUserDataDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.UserDatas.UserDataManager:SaveUserData");

        private AccessTools.FieldRef<LauncherModsVM, UserDataManager>? UserDataManager =
            AccessTools2.FieldRefAccess<LauncherModsVM, UserDataManager>("_userDataManager");

        public bool GlobalCheckboxState
        {
            get => _isDisabled;
            set
            {
                if (value != _isDisabled)
                {
                    _isDisabled = value;
                    _launcherModsVM.OnPropertyChangedWithValue(value, nameof(GlobalCheckboxState));
                }
            }
        }
        private bool _isDisabled;

        public bool IsSingleplayer => !((bool) _launcherModsVM.GetPropertyValue("IsDisabledOnMultiplayer"));

        private readonly LauncherModsVM _launcherModsVM;

        public LauncherModsVMMixin(LauncherModsVM launcherModsVM)
        {
            _launcherModsVM = launcherModsVM;

            void SetVMProperty(string property)
            {
                var propertyInfo = new WrappedPropertyInfo(AccessTools2.Property($"Bannerlord.BUTRLoader.Patches.Mixins.LauncherModsVMMixin:{property}")!, this);
                _launcherModsVM.AddProperty(property, propertyInfo);
                propertyInfo.PropertyChanged += (_, e) => _launcherModsVM.OnPropertyChanged(e.PropertyName);
            }

            SetVMProperty(nameof(GlobalCheckboxState));
            SetVMProperty(nameof(IsSingleplayer));

            _launcherModsVM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == "IsDisabledOnMultiplayer")
                    _launcherModsVM.OnPropertyChanged(nameof(IsSingleplayer));
            };
            _launcherModsVM.PropertyChangedWithValue += (_, e) =>
            {
                if (e.PropertyName == "IsDisabledOnMultiplayer")
                    _launcherModsVM.OnPropertyChanged(nameof(IsSingleplayer));
            };
        }

        public void ExecuteGlobalCheckbox()
        {
            GlobalCheckboxState = !GlobalCheckboxState;

            foreach (var launcherModuleVM in _launcherModsVM.Modules)
            {
                if (GlobalCheckboxState)
                {
                    if (ExecuteSelect is not null && !launcherModuleVM.IsSelected)
                        ExecuteSelect(launcherModuleVM);
                }
                else
                {
                    launcherModuleVM.IsSelected = false;
                }
            }
        }

        public void ExecuteRefresh()
        {
            if (SaveUserData is null || LoadSubModules is null || UserDataManager?.Invoke(_launcherModsVM) is not { } mng) return;

            SaveUserData(mng);
            LoadSubModules(_launcherModsVM, false);
        }
    }
}