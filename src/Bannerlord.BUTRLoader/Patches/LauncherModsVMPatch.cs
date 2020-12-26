using Bannerlord.BUTRLoader.Extensions;
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
        private static readonly Dictionary<string, ModuleInfo2> ExtendedModuleInfoCache = new();
        private static ModuleInfo2 GetExtendedModuleInfo(ModuleInfo moduleInfo)
        {
            if (ExtendedModuleInfoCache.ContainsKey(moduleInfo.Id))
                return ExtendedModuleInfoCache[moduleInfo.Id];

            var extendedModuleInfo = new ModuleInfo2();
            extendedModuleInfo.Load(moduleInfo.Alias);
            ExtendedModuleInfoCache[moduleInfo.Id] = extendedModuleInfo;
            return extendedModuleInfo;
        }

        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools.Method(typeof(LauncherModsVM), "GetDependentModulesOf"),
                prefix: AccessTools.Method(typeof(LauncherModsVMPatch), nameof(GetDependentModulesOfPrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                AccessTools.Method(typeof(LauncherModsVM), "IsAllDependenciesOfModulePresent"),
                prefix: AccessTools.Method(typeof(LauncherModsVMPatch), nameof(IsAllDependenciesOfModulePresentPrefix)));
            if (!res2) return false;

            return true;
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
            var dependencies = extendedDependencies.Select(em => sourceList.Find(m => m.Id == em.Id));

            __result = dependencies;
            return false;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsAllDependenciesOfModulePresentPrefix(ModuleInfo info, List<ModuleInfo> ____modulesCache, ref bool __result)
        {
            __result = true;
            var extendedModuleInfo = GetExtendedModuleInfo(info);

            // Merge TW's dependency implementation and BUTR implementation
            var allDependencies = extendedModuleInfo.DependedModules.Select(dm => dm.ModuleId).Concat(
                extendedModuleInfo.DependedModuleMetadatas.Where(dmm => dmm.LoadType == LoadType.LoadBeforeThis).Select(dmm => dmm.Id))
                .ToArray();

            // Check any dependencies issues first
            foreach (var dependedModuleId in allDependencies)
            {
                var module = ____modulesCache.Find(m => m.Id == dependedModuleId);
                IsAllDependenciesOfModulePresentPrefix(module, ____modulesCache, ref __result);
                if (!__result)
                    return false;
            }

            // Check if all dependencies are present
            foreach (var dependedModuleId in allDependencies)
            {
                var metadata = extendedModuleInfo.DependedModuleMetadatas.Find(dmm => dmm.Id == dependedModuleId);
                if (metadata.IsOptional)
                    continue;

                if (!ExtendedModuleInfoCache.ContainsKey(dependedModuleId))
                {
                    __result = false;
                    return false;
                }
            }

            // Check if the dependencies have the minimum required version
            var comparer = new ApplicationVersionComparer();
            foreach (var dependedModuleMetadata in extendedModuleInfo.DependedModuleMetadatas)
            {
                if (dependedModuleMetadata.Version == ApplicationVersion.Empty)
                    continue;

                var dependedModule = ExtendedModuleInfoCache[dependedModuleMetadata.Id];
                // dependedModuleMetadata.Version > dependedModule.Version
                if (dependedModule is null || comparer.Compare(dependedModuleMetadata.Version, dependedModule.Version) > 0)
                {
                    __result = false;
                    return false;
                }
            }

            return false;
        }
    }
}