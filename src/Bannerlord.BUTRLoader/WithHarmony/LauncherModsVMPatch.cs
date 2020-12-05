using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.ModuleInfoExtended;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.WithHarmony
{
    internal static class LauncherModsVMPatch
    {
        private static readonly Dictionary<string, ModuleInfo2> ExtendedModuleInfos = new Dictionary<string, ModuleInfo2>();
        private static ModuleInfo2 GetExtendedModuleInfo(ModuleInfo moduleInfo)
        {
            if (ExtendedModuleInfos.ContainsKey(moduleInfo.Id))
                return ExtendedModuleInfos[moduleInfo.Id];

            var extendedModuleInfo = new ModuleInfo2();
            extendedModuleInfo.Load(moduleInfo.Alias);
            ExtendedModuleInfos[moduleInfo.Id] = extendedModuleInfo;
            return extendedModuleInfo;
        }

        private static bool GetDependentModulesOfPrefix(IEnumerable<ModuleInfo> source, ModuleInfo module, ref IEnumerable<ModuleInfo> __result)
        {
            var sourceList = source.ToList();
            var extendedSourceList = sourceList.ConvertAll(GetExtendedModuleInfo);

            var extendedModuleInfo = GetExtendedModuleInfo(module);

            var extendedDependencies = ModuleSorter.GetDependentModulesOf(extendedSourceList, extendedModuleInfo);
            var dependencies = extendedDependencies.Select(em => sourceList.First(m => m.Id == em.Id));

            __result = dependencies;
            return false;
        }

        public static void Enable(Harmony harmony)
        {
            var toPatchMethod = typeof(LauncherModsVM).GetMethod("GetDependentModulesOf", ReflectionHelper.All);
            var prefixMethod = typeof(LauncherModsVMPatch).GetMethod(nameof(GetDependentModulesOfPrefix), ReflectionHelper.All);

            if (toPatchMethod == null || prefixMethod == null)
                return;

            harmony.Patch(toPatchMethod, new HarmonyMethod(prefixMethod));
        }
    }
}