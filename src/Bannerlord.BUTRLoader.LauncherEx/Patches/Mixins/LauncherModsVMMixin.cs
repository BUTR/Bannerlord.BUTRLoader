using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib.BUTR.Extensions;

using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.Library;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherModsVMMixin
    {
        private delegate void ExecuteSelectDelegate(object instance);
        private static readonly ExecuteSelectDelegate? ExecuteSelect =
            AccessTools2.GetDelegateObjectInstance<ExecuteSelectDelegate>(LauncherModuleVMWrapper.LauncherModuleVMType!, "ExecuteSelect");

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

            var propsObject = AccessTools2.Field(typeof(ViewModel), "_propertyInfos")?.GetValue(_launcherModsVM) as Dictionary<string, PropertyInfo>
                              ?? new Dictionary<string, PropertyInfo>();

            void SetVMProperty(string property)
            {
                var propertyInfo = new WrappedPropertyInfo(AccessTools2.Property(typeof(LauncherModsVMMixin), property), this);
                propertyInfo.PropertyChanged += (_, e) => _launcherModsVM.OnPropertyChanged(e.PropertyName);
                propsObject[property] = propertyInfo;
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
                    ExecuteSelect?.Invoke(launcherModuleVM);
                }
                else
                {
                    launcherModuleVM.IsSelected = false;
                }
            }
        }
    }
}