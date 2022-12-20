using Bannerlord.BUTRLoader.Patches.Mixins;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherModsVMPatch
    {
        private static readonly AccessTools.FieldRef<LauncherModsVM, UserData>? _userData =
            AccessTools2.FieldRefAccess<LauncherModsVM, UserData>("_userData");

        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(typeof(LauncherModsVM), "LoadSubModules"),
                prefix: AccessTools2.DeclaredMethod(typeof(LauncherModsVMPatch), nameof(LoadSubModulesPrefix)));

            return true;
        }

        public static bool LoadSubModulesPrefix(LauncherModsVM __instance, bool isMultiplayer)
        {
            if (_userData is null)
                return true;

            if (__instance.GetPropertyValue(nameof(LauncherModsVMMixin)) is LauncherModsVMMixin mixin)
                mixin.Initialize(isMultiplayer, _userData(__instance));

            return false;
        }
    }
}