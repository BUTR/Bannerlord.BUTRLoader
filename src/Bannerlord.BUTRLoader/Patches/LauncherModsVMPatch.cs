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

        // LoadType.LoadBeforeThis overrides any TW declaration
        // LoadType.LoadAfterThis overrides any LoadType.LoadBeforeThis declaration
        private static string[] GetAllDependencies(ModuleInfo module, ICollection<ModuleInfo> loadedModules)
        {
            var extendedModuleInfo = GetExtendedModuleInfo(module);

            // We can't cache the dependencies because this collection is a dynamic one based
            // on whether the external module we check is selected
            var customExternalDependencies = (loadedModules.Where(mi => mi.IsSelected)
                    .Select(GetExtendedModuleInfo)
                    .SelectMany(emi => emi.DependedModuleMetadatas, (emi, metadata) => (emi, metadata))
                    .Where(tuple => tuple.metadata.LoadType == LoadType.LoadAfterThis && tuple.metadata.Id == extendedModuleInfo.Id)
                    .Select(tuple => tuple.emi.Id))
                .ToArray();

            var customDirectDependencies = extendedModuleInfo.DependedModuleMetadatas
                .Where(dmm => !customExternalDependencies.Contains(dmm.Id) && dmm.LoadType == LoadType.LoadBeforeThis)
                .Select(dmm => dmm.Id)
                .ToArray();

            var twDependencies = extendedModuleInfo.DependedModules
                .Where(dm => !customDirectDependencies.Contains(dm.ModuleId) && !customExternalDependencies.Contains(dm.ModuleId))
                .Select(dm => dm.ModuleId)
                .ToArray();

            // Merge TW's dependency implementation and BUTR implementation
            var dependencies = customDirectDependencies
                .Concat(customExternalDependencies)
                .Concat(twDependencies)
                .ToArray();
            return dependencies;
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

            var res3 = harmony.TryPatch(
                AccessTools.Method(typeof(LauncherModsVM), "ChangeIsSelectedOf"),
                prefix: AccessTools.Method(typeof(LauncherModsVMPatch), nameof(ChangeIsSelectedOfPrefix)));
            if (!res3) return false;

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

        /// <summary>
        /// Decides if the Module is valid.
        /// We replace the original method with our implementation.
        /// </summary>
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsAllDependenciesOfModulePresentPrefix(LauncherModsVM __instance, ModuleInfo info, List<ModuleInfo> ____modulesCache, ref bool __result)
        {
            try
            {
                var extendedModuleInfo = GetExtendedModuleInfo(info);
                var allDependencies = GetAllDependencies(info, ____modulesCache);

                // Check that the dependencies themselves have all dependencies present
                foreach (var dependedModuleId in allDependencies)
                {
                    var metadata = extendedModuleInfo.DependedModuleMetadatas.Find(dmm => dmm.Id == dependedModuleId);

                    // Ignore the check for Optional
                    if (metadata.IsOptional) continue;

                    var module = ____modulesCache.Find(m => m.Id == dependedModuleId);

                    if (!AreAllDependenciesOfModulePresent(__instance, module, ____modulesCache))
                    {
                        __result = false;
                        return false;
                    }
                }

                // Check that all dependencies are present
                foreach (var dependedModuleId in allDependencies)
                {
                    var metadata = extendedModuleInfo.DependedModuleMetadatas.Find(dmm => dmm.Id == dependedModuleId);

                    // Handle only direct dependencies
                    if (metadata.LoadType != LoadType.LoadBeforeThis) continue;

                    // Ignore the check for Optional
                    if (metadata.IsOptional) continue;

                    if (!ExtendedModuleInfoCache.ContainsKey(dependedModuleId))
                    {
                        __result = false;
                        return false;
                    }
                }

                // Check that the dependencies have the minimum required version set by DependedModuleMetadatas
                var comparer = new ApplicationVersionFullComparer();
                foreach (var metadata in extendedModuleInfo.DependedModuleMetadatas  )
                {
                    // Ignore the check for Optional
                    if (metadata.IsOptional) continue;

                    // Ignore the check for non-provided versions
                    if (metadata.Version == ApplicationVersion.Empty) continue;

                    var dependedModule = ExtendedModuleInfoCache[metadata.Id];
                    // dependedModuleMetadata.Version > dependedModule.Version
                    if (dependedModule is null || comparer.Compare(metadata.Version, dependedModule.Version) > 0)
                    {
                        __result = false;
                        return false;
                    }
                }

                // Do not load this mod if an incompatible mod is selected
                foreach (var metadata in extendedModuleInfo.DependedModuleMetadatas)
                {
                    if (metadata.IsIncompatible)
                    {
                        var moduleVM = __instance.Modules.FirstOrDefault(m => m.Info.Id == metadata.Id);

                        // If the incompatible mod is selected, this mod is disabled
                        if (moduleVM?.IsSelected == true)
                        {
                            __result = false;
                            return false;
                        }
                    }
                }

                // If another mod declared incompatibility and is selected, disable this
                foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
                {
                    if (key == info.Id) continue;

                    foreach (var metadata in moduleInfo.DependedModuleMetadatas)
                    {
                        if (metadata.IsIncompatible && metadata.Id == extendedModuleInfo.Id)
                        {
                            var moduleVM = __instance.Modules.FirstOrDefault(m => m.Info.Id == key);

                            // If the incompatible mod is selected, this mod is disabled
                            if (moduleVM?.IsSelected == true)
                            {
                                __result = false;
                                return false;
                            }
                        }
                    }
                }

                __result = true;
                return false;
            }
            catch
            {
                return true;
            }
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool ChangeIsSelectedOfPrefix(LauncherModsVM __instance, LauncherModuleVM targetModule, List<ModuleInfo> ____modulesCache)
        {
            if (!AreAllDependenciesOfModulePresent(__instance, targetModule.Info, ____modulesCache))
            {
                // Direct and current External Dependencies are not valid, do nothing
                return false;
            }

            var extendedModuleInfo = GetExtendedModuleInfo(targetModule.Info);
            var allDependencies = GetAllDependencies(targetModule.Info, ____modulesCache);

            targetModule.IsSelected = !targetModule.IsSelected;

            if (targetModule.IsSelected)
            {
                // Vanilla check
                // Select all dependencies if they are not selected
                foreach (var module in __instance.Modules)
                {
                    if (!module.IsSelected && allDependencies.Any(id => id == module.Info.Id))
                        ChangeIsSelectedOf(__instance, module, ____modulesCache);
                    //module.IsSelected |= allDependencies.Any(id => id == module.Info.Id);
                }

                // Deselect and disable any mod that is incompatible with this one
                foreach (var dmm in extendedModuleInfo.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible))
                {
                    var incompatibleModuleVM = __instance.Modules.First(m => m.Info.Id == dmm.Id);
                    if (incompatibleModuleVM is not null)
                    {
                        if (incompatibleModuleVM.IsSelected)
                            ChangeIsSelectedOf(__instance, incompatibleModuleVM, ____modulesCache);

                        incompatibleModuleVM.IsDisabled = true;
                    }
                }

                // Disable any mod that declares this mod as incompatible
                foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
                {
                    foreach (var dmm in moduleInfo.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible && dmm.Id == extendedModuleInfo.Id))
                    {
                        var incompatibleModuleVM = __instance.Modules.FirstOrDefault(m => m.Info.Id == key);
                        if (incompatibleModuleVM is not null)
                        {
                            if (incompatibleModuleVM.IsSelected)
                                ChangeIsSelectedOf(__instance, incompatibleModuleVM, ____modulesCache);

                            // We need to re-check that everything is alright with the external dependency
                            incompatibleModuleVM.IsDisabled |= !AreAllDependenciesOfModulePresent(__instance, incompatibleModuleVM.Info, ____modulesCache);
                        }
                    }
                }

                return false;
            }
            else
            {
                // Vanilla check
                // Deselect all modules that depend on this module if they are selected
                foreach (var module in __instance.Modules)
                {
                    var allDependencies2 = GetAllDependencies(module.Info, ____modulesCache);

                    if (module.IsSelected && allDependencies2.Any(id => id == targetModule.Info.Id))
                        ChangeIsSelectedOf(__instance, module, ____modulesCache);
                    //module.IsSelected &= allDependencies2.All(id => id != targetModule.Info.Id);
                }

                // Enable for selection any mod that is incompatible with this one
                foreach (var dmm in extendedModuleInfo.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible))
                {
                    var incompatibleModuleVM = __instance.Modules.First(m => m.Info.Id == dmm.Id);
                    if (incompatibleModuleVM is not null )
                    {
                        incompatibleModuleVM.IsDisabled = false;
                    }
                }

                // Check if any mod that declares this mod as incompatible can be Enabled
                foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
                {
                    foreach (var dmm in moduleInfo.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible && dmm.Id == extendedModuleInfo.Id))
                    {
                        var incompatibleModuleVM = __instance.Modules.FirstOrDefault(m => m.Info.Id == key);
                        if (incompatibleModuleVM is not null)
                        {
                            // We need to re-check that everything is alright with the external dependency
                            incompatibleModuleVM.IsDisabled &= !AreAllDependenciesOfModulePresent(__instance, incompatibleModuleVM.Info, ____modulesCache);
                        }
                    }
                }

                return false;
            }
        }


        private static bool AreAllDependenciesOfModulePresent(LauncherModsVM launcherModsVM, ModuleInfo moduleInfo, List<ModuleInfo> modulesCache)
        {
            var result = false;
            IsAllDependenciesOfModulePresentPrefix(launcherModsVM, moduleInfo, modulesCache, ref result);
            return result;
        }

        private static void ChangeIsSelectedOf(LauncherModsVM launcherModsVM, LauncherModuleVM launcherModuleVM, List<ModuleInfo> modulesCache)
        {
            ChangeIsSelectedOfPrefix(launcherModsVM, launcherModuleVM, modulesCache);
        }
    }
}