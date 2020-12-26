using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.ModuleInfoExtended;

using HarmonyLib;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherModsVMPatch
    {
        private static readonly Dictionary<string, ModuleInfo2> ExtendedModuleInfos = new();
        private static ModuleInfo2 GetExtendedModuleInfo(ModuleInfo moduleInfo)
        {
            if (ExtendedModuleInfos.ContainsKey(moduleInfo.Id))
                return ExtendedModuleInfos[moduleInfo.Id];

            var extendedModuleInfo = new ModuleInfo2();
            extendedModuleInfo.Load(moduleInfo.Alias);
            ExtendedModuleInfos[moduleInfo.Id] = extendedModuleInfo;
            return extendedModuleInfo;
        }

        public static void Enable(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(LauncherModsVM), "GetDependentModulesOf"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(LauncherModsVMPatch), nameof(GetDependentModulesOfPrefix))));

            harmony.Patch(
                AccessTools.Method(typeof(LauncherModsVM), "IsAllDependenciesOfModulePresent"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(LauncherModsVMPatch), nameof(IsAllDependenciesOfModulePresentPrefix))));
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetDependentModulesOfPrefix(IEnumerable<ModuleInfo> source, ModuleInfo module, ref IEnumerable<ModuleInfo> __result)
        {
            if (!BUTRLoaderAppDomainManager.ExtendedSorting)
                return true;

            var sourceList = source.ToList();
            var extendedSourceList = sourceList.ConvertAll(GetExtendedModuleInfo);

            var extendedModuleInfo = GetExtendedModuleInfo(module);

            var extendedDependencies = ModuleSorter.GetDependentModulesOf(extendedSourceList, extendedModuleInfo);
            var dependencies = extendedDependencies.Select(em => sourceList.First(m => m.Id == em.Id));

            __result = dependencies;
            return false;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsAllDependenciesOfModulePresentPrefix(ModuleInfo info, ref bool __result)
        {
            __result = true;

            var extendedModuleInfo = GetExtendedModuleInfo(info);

            var allDependencies = extendedModuleInfo.DependedModules.Select(dm => dm.ModuleId).Concat(extendedModuleInfo
                .DependedModuleMetadatas.Where(dmm => dmm.LoadType == LoadType.LoadBeforeThis).Select(dmm => dmm.Id)).ToArray();

            foreach (var dependedModuleId in allDependencies)
            {
                var metadata = extendedModuleInfo.DependedModuleMetadatas.Find(dmm => dmm.Id == dependedModuleId);
                if (metadata.IsOptional)
                    continue;

                if (!ExtendedModuleInfos.ContainsKey(dependedModuleId))
                {
                    __result = false;
                    break;
                }
            }

            return false;
        }
    }
}