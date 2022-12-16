using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Extensions;

using HarmonyLib.BUTR.Extensions;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherNewsVMMixin
    {
        public bool IsDisabled2
        {
            get => _isDisabled2;
            set
            {
                if (value != _isDisabled2)
                {
                    _isDisabled2 = value;
                    _launcherNewsVM.OnPropertyChanged(nameof(IsDisabled2));
                }
            }
        }
        private bool _isDisabled2;


        private readonly LauncherNewsVM _launcherNewsVM;

        public LauncherNewsVMMixin(LauncherNewsVM launcherNewsVM)
        {
            _launcherNewsVM = launcherNewsVM;

            void SetVMProperty(string property)
            {
                var propertyInfo = new WrappedPropertyInfo(AccessTools2.DeclaredProperty(typeof(LauncherNewsVMMixin), property)!, this);
                _launcherNewsVM.AddProperty(property, propertyInfo);
                propertyInfo.PropertyChanged += (_, e) => _launcherNewsVM.OnPropertyChanged(e.PropertyName);
            }

            SetVMProperty(nameof(IsDisabled2));
        }
    }
}