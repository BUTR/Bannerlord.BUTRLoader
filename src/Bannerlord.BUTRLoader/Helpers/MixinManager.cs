using Bannerlord.BUTRLoader.Patches.Mixins;

using System.Collections.Generic;

using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class MixinManager
    {
        private static readonly List<object> Mixins = new();

        public static void AddMixins(LauncherVM launcherVM)
        {
            Mixins.Add(new LauncherVMMixin(launcherVM));
            foreach (var launcherModuleVM in launcherVM.ModsData.Modules)
            {
                Mixins.Add(new LauncherModuleVMMixin(launcherModuleVM));
            }
        }
    }
}
