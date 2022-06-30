using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Wrappers;

using HarmonyLib.BUTR.Extensions;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherModsVMMixin
    {
        private delegate void ExecuteSelectDelegate(object instance);
        private static readonly ExecuteSelectDelegate? ExecuteSelect =
            AccessTools2.GetDelegateObjectInstance<ExecuteSelectDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherModuleVM:ExecuteSelect") ??
            AccessTools2.GetDelegateObjectInstance<ExecuteSelectDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM:ExecuteSelect");

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

        private readonly ViewModel _launcherModsVM;

        public LauncherModsVMMixin(ViewModel launcherModsVM)
        {
            _launcherModsVM = launcherModsVM;

            void SetVMProperty(string property)
            {
                var propertyInfo = new WrappedPropertyInfo(AccessTools2.Property($"Bannerlord.BUTRLoader.Patches.Mixins.LauncherModsVMMixin:{property}")!, this);
                _launcherModsVM.AddProperty(property, propertyInfo);
                propertyInfo.PropertyChanged += (_, e) => _launcherModsVM.OnPropertyChanged(e.PropertyName);
            }

            SetVMProperty(nameof(GlobalCheckboxState));
        }

        public void ExecuteGlobalCheckbox()
        {
            GlobalCheckboxState = !GlobalCheckboxState;

            var wrapper = LauncherModsVMWrapper.Create(_launcherModsVM);

            foreach (var launcherModuleVM in wrapper.Modules)
            {
                if (GlobalCheckboxState)
                {
                    if (ExecuteSelect is not null && launcherModuleVM.Object is not null)
                        ExecuteSelect(launcherModuleVM.Object);
                }
                else
                {
                    launcherModuleVM.IsSelected = false;
                }
            }
        }
    }
}