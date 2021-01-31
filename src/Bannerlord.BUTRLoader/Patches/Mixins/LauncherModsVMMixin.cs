using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;

using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherModsVMMixin
    {
        private readonly LauncherModsVM _launcherModsVM;

        public LauncherModsVMMixin(LauncherModsVM launcherModsVM)
        {
            _launcherModsVM = launcherModsVM;

            var propsObject = AccessTools.Field(typeof(ViewModel), "_propertyInfos")?.GetValue(_launcherModsVM) as Dictionary<string, PropertyInfo>
                              ?? new Dictionary<string, PropertyInfo>();

            void SetVMProperty(string property)
            {
                propsObject[property] = new WrappedPropertyInfo(
                    AccessTools.Property(typeof(LauncherModsVMMixin), property),
                    this,
                    () => _launcherModsVM.OnPropertyChanged(property));
            }

        }
    }
}