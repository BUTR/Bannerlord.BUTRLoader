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
        internal static readonly Dictionary<string, ModuleInfo2> ExtendedModuleInfoCache = new();
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
                prefix: AccessTools.Method(typeof(LauncherModsVMPatch), nameof(AreAllDependenciesOfModulePresentPrefix)));
            if (!res2) return false;

            var res3 = harmony.TryPatch(
                AccessTools.Method(typeof(LauncherModsVM), "ChangeIsSelectedOf"),
                prefix: AccessTools.Method(typeof(LauncherModsVMPatch), nameof(ChangeIsSelectedOfPrefix)));
            if (!res3) return false;

            return true;
        }

        public static void Disable(Harmony harmony)
        {
            harmony.Unpatch(
                AccessTools.Method(typeof(LauncherModsVM), "GetDependentModulesOf"),
                AccessTools.Method(typeof(LauncherModsVMPatch), nameof(GetDependentModulesOfPrefix)));

            harmony.Unpatch(
                AccessTools.Method(typeof(LauncherModsVM), "IsAllDependenciesOfModulePresent"),
                AccessTools.Method(typeof(LauncherModsVMPatch), nameof(AreAllDependenciesOfModulePresentPrefix)));

            harmony.Unpatch(
                AccessTools.Method(typeof(LauncherModsVM), "ChangeIsSelectedOf"),
                AccessTools.Method(typeof(LauncherModsVMPatch), nameof(ChangeIsSelectedOfPrefix)));
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

        /// <summary>
        /// Decides if the Module is valid.
        /// We replace the original method with our implementation.
        /// </summary>
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool AreAllDependenciesOfModulePresentPrefix(LauncherModsVM __instance, ModuleInfo info, List<ModuleInfo> ____modulesCache, ref bool __result)
        {
            if (!BUTRLoaderAppDomainManager.ExtendedSorting)
                return true;

            var info2 = GetExtendedModuleInfo(info);

            var visited = new HashSet<ModuleInfo2>();
            var dependencies = ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, info2, visited);

            // Iterate each dependency including this
            __result = dependencies.All(dependency => ModuleIsCorrect(__instance, dependency, ____modulesCache, visited));
            return false;
        }
        private static bool AreAllDependenciesOfModulePresent(LauncherModsVM launcherModsVM, ModuleInfo moduleInfo, List<ModuleInfo> modulesCache)
        {
            var result = false;
            AreAllDependenciesOfModulePresentPrefix(launcherModsVM, moduleInfo, modulesCache, ref result);
            return result;
        }
        private static bool ModuleIsCorrect(LauncherModsVM instance, ModuleInfo2 moduleInfo2, List<ModuleInfo> modules, HashSet<ModuleInfo2> visited)
        {
            // Check that all dependencies are present
            foreach (var dependency in moduleInfo2.DependedModules)
            {
                if (!ExtendedModuleInfoCache.ContainsKey(dependency.ModuleId))
                    return false;
            }
            foreach (var metadata in moduleInfo2.DependedModuleMetadatas)
            {
                // Ignore the check for Optional
                if (metadata.IsOptional) continue;

                if (metadata.IsIncompatible) continue;

                if (!ExtendedModuleInfoCache.ContainsKey(metadata.Id))
                    return false;
            }


            var dependencies = ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, moduleInfo2, visited).ToArray();

            // Check that the dependencies themselves have all dependencies present
            foreach (var dependency in dependencies)
            {
                var metadata = moduleInfo2.DependedModuleMetadatas.Find(dmm => dmm.Id == dependency.Id);

                // Ignore the check for Optional
                if (metadata.IsOptional) continue;

                var module = modules.Find(m => m.Id == dependency.Id);
                if (!ModuleIsCorrect(instance, GetExtendedModuleInfo(module), modules, visited))
                    return false;
            }

            // Check that all present dependencies are valid
            foreach (var dependency in dependencies)
            {
                var metadata = moduleInfo2.DependedModuleMetadatas.Find(dmm => dmm.Id == dependency.Id);

                // Handle only direct dependencies
                if (metadata.LoadType != LoadType.LoadBeforeThis) continue;

                // Ignore the check for Optional
                if (metadata.IsOptional) continue;

                if (!ExtendedModuleInfoCache.ContainsKey(dependency.Id))
                    return false;
            }

            // Check that the dependencies have the minimum required version set by DependedModuleMetadatas
            var comparer = new ApplicationVersionFullComparer();
            foreach (var metadata in moduleInfo2.DependedModuleMetadatas.Where(m => !m.IsOptional && !m.IsIncompatible))
            {
                // Ignore the check for non-provided versions
                if (metadata.Version == ApplicationVersion.Empty) continue;

                if (!ExtendedModuleInfoCache.TryGetValue(metadata.Id, out var dependedModule))
                    return false;
                // dependedModuleMetadata.Version > dependedModule.Version
                if (dependedModule is null || comparer.Compare(metadata.Version, dependedModule.Version) > 0)
                    return false;
            }

            // Do not load this mod if an incompatible mod is selected
            foreach (var metadata in moduleInfo2.DependedModuleMetadatas.Where(m => m.IsIncompatible))
            {
                var moduleVM = instance.Modules.FirstOrDefault(m => m.Info.Id == metadata.Id);

                // If the incompatible mod is selected, this mod is disabled
                if (moduleVM?.IsSelected == true)
                    return false;
            }

            // If another mod declared incompatibility and is selected, disable this
            foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
            {
                if (key == moduleInfo2.Id) continue;

                foreach (var metadata in moduleInfo.DependedModuleMetadatas.Where(m => m.IsIncompatible && m.Id == moduleInfo2.Id))
                {
                    var moduleVM = instance.Modules.FirstOrDefault(m => m.Info.Id == key);

                    // If the incompatible mod is selected, this mod is disabled
                    if (moduleVM?.IsSelected == true)
                        return false;
                }
            }

            return true;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool ChangeIsSelectedOfPrefix(LauncherModsVM __instance, LauncherModuleVM targetModule, List<ModuleInfo> ____modulesCache)
        {
            if (!BUTRLoaderAppDomainManager.ExtendedSorting)
                return true;

            if (!AreAllDependenciesOfModulePresent(__instance, targetModule.Info, ____modulesCache))
            {
                // Direct and current External Dependencies are not valid, do nothing
                return false;
            }

            ChangeIsSelectedOf(__instance, targetModule, ____modulesCache);

            return false;
        }
        private static void ChangeIsSelectedOf(LauncherModsVM instance, LauncherModuleVM targetModule, List<ModuleInfo> modules)
        {
            var targetModuleInfo2 = GetExtendedModuleInfo(targetModule.Info);

            var visited = new HashSet<ModuleInfo2>();
            var dependencies = ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, targetModuleInfo2, visited).ToArray();

            targetModule.IsSelected = !targetModule.IsSelected;

            if (targetModule.IsSelected)
            {
                // Vanilla check
                // Select all dependencies if they are not selected
                foreach (var module in instance.Modules)
                {
                    if (!module.IsSelected && dependencies.Any(d => d.Id == module.Info.Id))
                        ChangeIsSelectedOf(instance, module, modules);
                        //module.IsSelected |= allDependencies.Any(id => id == module.Info.Id);
                }

                // Deselect and disable any mod that is incompatible with this one
                foreach (var dmm in targetModuleInfo2.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible))
                {
                    var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => m.Info.Id == dmm.Id);
                    if (incompatibleModuleVM is not null)
                    {
                        if (incompatibleModuleVM.IsSelected)
                            ChangeIsSelectedOf(instance, incompatibleModuleVM, modules);

                        incompatibleModuleVM.IsDisabled = true;
                    }
                }

                // Disable any mod that declares this mod as incompatible
                foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
                {
                    foreach (var dmm in moduleInfo.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible && dmm.Id == targetModuleInfo2.Id))
                    {
                        var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => m.Info.Id == key);
                        if (incompatibleModuleVM is not null)
                        {
                            if (incompatibleModuleVM.IsSelected)
                                ChangeIsSelectedOf(instance, incompatibleModuleVM, modules);

                            // We need to re-check that everything is alright with the external dependency
                            incompatibleModuleVM.IsDisabled |= !AreAllDependenciesOfModulePresent(instance, incompatibleModuleVM.Info, modules);
                        }
                    }
                }
            }
            else
            {
                // Vanilla check
                // Deselect all modules that depend on this module if they are selected
                foreach (var module in instance.Modules.Where(m => !m.IsOfficial && m.IsSelected))
                {
                    var moduleInfo2 = GetExtendedModuleInfo(module.Info);
                    var dependencies2 = ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, moduleInfo2, skipExternalDependencies: true);
                    if (dependencies2.Any(d => d.Id == targetModuleInfo2.Id))
                        ChangeIsSelectedOf(instance, module, modules);
                }

                // Enable for selection any mod that is incompatible with this one
                foreach (var dmm in targetModuleInfo2.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible))
                {
                    var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => m.Info.Id == dmm.Id);
                    if (incompatibleModuleVM is not null )
                    {
                        incompatibleModuleVM.IsDisabled = false;
                    }
                }

                // Check if any mod that declares this mod as incompatible can be Enabled
                foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
                {
                    foreach (var dmm in moduleInfo.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible && dmm.Id == targetModuleInfo2.Id))
                    {
                        var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => m.Info.Id == key);
                        if (incompatibleModuleVM is not null)
                        {
                            // We need to re-check that everything is alright with the external dependency
                            incompatibleModuleVM.IsDisabled &= !AreAllDependenciesOfModulePresent(instance, incompatibleModuleVM.Info, modules);
                        }
                    }
                }
            }

            //targetModule.IsDisabled = !AreAllDependenciesOfModulePresent(instance, targetModule.Info, modules);
        }
    }
}