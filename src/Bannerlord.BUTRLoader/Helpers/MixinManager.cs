using Bannerlord.BUTRLoader.Patches.Mixins;

using System.Collections.Generic;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class MixinManager
    {
        public static readonly Dictionary<ViewModel, List<object>> Mixins = new();

        private static void AddMixin(ViewModel viewModel, object mixin)
        {
            if (Mixins.TryGetValue(viewModel, out var list))
            {
                list.Add(mixin);
            }
            else
            {
                Mixins.Add(viewModel, new List<object> { mixin });
            }
        }

        public static void AddMixins(LauncherVM launcherVM)
        {
            AddMixin(launcherVM, new LauncherVMMixin(launcherVM));
            AddMixin(launcherVM.ModsData, new LauncherModsVMMixin(launcherVM.ModsData));
            foreach (var launcherModuleVM in launcherVM.ModsData.Modules)
            {
                AddMixin(launcherModuleVM, new LauncherModuleVMMixin(launcherModuleVM));
            }
        }
    }
}