using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib.BUTR.Extensions;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherNewsVMMixin : ViewModelMixin<LauncherNewsVM>
    {
        public bool IsDisabled2 { get => _isDisabled2; set => SetField(ref _isDisabled2, value, nameof(IsDisabled2)); }
        private bool _isDisabled2;


        public LauncherNewsVMMixin(LauncherNewsVM launcherNewsVM) : base(launcherNewsVM)
        {
            void SetVMProperty(string property)
            {
                var propertyInfo = new WrappedPropertyInfo(AccessTools2.DeclaredProperty(typeof(LauncherNewsVMMixin), property)!, this);
                launcherNewsVM.AddProperty(property, propertyInfo);
                propertyInfo.PropertyChanged += (_, e) => launcherNewsVM.OnPropertyChanged(e.PropertyName);
            }

            SetVMProperty(nameof(IsDisabled2));
        }
    }
}