using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.LauncherEx;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherModsVMMixin
    {
        private delegate void LoadSubModulesDelegate(LauncherModsVM instance, bool isMultiplayer);
        private static readonly LoadSubModulesDelegate? LoadSubModules =
            AccessTools2.GetDelegate<LoadSubModulesDelegate>(typeof(LauncherModsVM), "LoadSubModules");

        private delegate void ExecuteSelectDelegate(LauncherModuleVM instance);
        private static readonly ExecuteSelectDelegate? ExecuteSelect =
            AccessTools2.GetDelegate<ExecuteSelectDelegate>(typeof(LauncherModuleVM), "ExecuteSelect");

        static LauncherModsVMMixin()
        {
            Manager._launcherHarmony.Patch(
                AccessTools2.Method(typeof(LauncherModsVM), "LoadSubModules"),
                postfix: new HarmonyMethod(typeof(LauncherModsVMMixin), nameof(LoadSubModulesPostfix)));
        }

        public bool GlobalCheckboxState
        {
            get => _checkboxState;
            set
            {
                if (value != _checkboxState)
                {
                    _checkboxState = value;
                    _launcherModsVM.OnPropertyChanged(nameof(GlobalCheckboxState));
                }
            }
        }
        private bool _checkboxState;

        public bool IsSingleplayer
        {
            get => _isSingleplayer;
            set
            {
                if (value != _isSingleplayer)
                {
                    _isSingleplayer = value;
                    _launcherModsVM.OnPropertyChanged(nameof(IsSingleplayer));
                }
            }
        }
        private bool _isSingleplayer;

        public bool IsDisabled2
        {
            get => _isDisabled2;
            set
            {
                if (value != _isDisabled2)
                {
                    _isDisabled2 = value;
                    _launcherModsVM.OnPropertyChanged(nameof(IsDisabled2));
                }
            }
        }
        private bool _isDisabled2;


        private readonly LauncherModsVM _launcherModsVM;

        public LauncherModsVMMixin(LauncherModsVM launcherModsVM)
        {
            _launcherModsVM = launcherModsVM;

            void SetVMProperty(string property)
            {
                var propertyInfo = new WrappedPropertyInfo(AccessTools2.DeclaredProperty(typeof(LauncherModsVMMixin), property)!, this);
                _launcherModsVM.AddProperty(property, propertyInfo);
                propertyInfo.PropertyChanged += (_, e) => _launcherModsVM.OnPropertyChanged(e.PropertyName);
            }

            SetVMProperty(nameof(GlobalCheckboxState));
            SetVMProperty(nameof(IsSingleplayer));
            SetVMProperty(nameof(IsDisabled2));
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
            LoadSubModules(_launcherModsVM, !IsSingleplayer);
        }
        
        public static void LoadSubModulesPostfix(LauncherModsVM __instance, bool isMultiplayer)
        {
            __instance.SetPropertyValue(nameof(IsSingleplayer), !isMultiplayer);
        }
    }
}