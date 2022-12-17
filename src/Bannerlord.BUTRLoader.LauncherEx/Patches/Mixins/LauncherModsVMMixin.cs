using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.LauncherEx;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Linq;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherModsVMMixin : ViewModelMixin<LauncherModsVM>
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

        public bool GlobalCheckboxState { get => _checkboxState; set => SetField(ref _checkboxState, value, nameof(GlobalCheckboxState)); }
        private bool _checkboxState;

        public bool IsSingleplayer { get => _isSingleplayer; set => SetField(ref _isSingleplayer, value, nameof(IsSingleplayer)); }
        private bool _isSingleplayer;

        public bool IsDisabled2 { get => _isDisabled2; set => SetField(ref _isDisabled2, value, nameof(IsDisabled2)); }
        private bool _isDisabled2;


        public LauncherModsVMMixin(LauncherModsVM launcherModsVM) : base(launcherModsVM)
        {
            void SetVMProperty(string property)
            {
                var propertyInfo = new WrappedPropertyInfo(AccessTools2.DeclaredProperty(typeof(LauncherModsVMMixin), property)!, this);
                launcherModsVM.AddProperty(property, propertyInfo);
                propertyInfo.PropertyChanged += (_, e) => launcherModsVM.OnPropertyChanged(e.PropertyName);
            }

            SetVMProperty(nameof(GlobalCheckboxState));
            SetVMProperty(nameof(IsSingleplayer));
            SetVMProperty(nameof(IsDisabled2));
        }

        public void ExecuteGlobalCheckbox()
        {
            GlobalCheckboxState = !GlobalCheckboxState;

            foreach (var launcherModuleVM in ViewModel?.Modules ?? Enumerable.Empty<LauncherModuleVM>())
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
            LoadSubModules(ViewModel, !IsSingleplayer);
        }
        
        public static void LoadSubModulesPostfix(LauncherModsVM __instance, bool isMultiplayer)
        {
            __instance.SetPropertyValue(nameof(IsSingleplayer), !isMultiplayer);
        }
    }
}