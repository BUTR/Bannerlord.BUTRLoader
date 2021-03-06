using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTR.Shared.ModuleInfoExtended;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

using static Bannerlord.BUTRLoader.Helpers.LauncherModuleVMExtensions;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherModsVMPatch
    {
        internal static readonly Dictionary<string, ModuleInfo2> ExtendedModuleInfoCache = new();

        private static ModuleInfo2 GetExtendedModuleInfo(object moduleInfo) => GetExtendedModuleInfo(ModuleInfoWrapper.Create(moduleInfo));
        private static ModuleInfo2 GetExtendedModuleInfo(ModuleInfoWrapper moduleInfoWrapper)
        {
            if (ExtendedModuleInfoCache.ContainsKey(moduleInfoWrapper.Id))
                return ExtendedModuleInfoCache[moduleInfoWrapper.Id];

            var extendedModuleInfo = new ModuleInfo2();
            extendedModuleInfo.Load(string.IsNullOrEmpty(moduleInfoWrapper.Alias) ? moduleInfoWrapper.Id : moduleInfoWrapper.Alias);
            ExtendedModuleInfoCache[moduleInfoWrapper.Id] = extendedModuleInfo;
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
        private static bool GetDependentModulesOfPrefix(IEnumerable<object> source, object module, ref IEnumerable __result)
        {
            if (!BUTRLoaderAppDomainManager.ExtendedSorting || CastMethod is null || ToListMethod is null)
                return true;

            var sourceList = source.ToList();
            var extendedSourceList = sourceList.ConvertAll(GetExtendedModuleInfo);

            var extendedModuleInfo = GetExtendedModuleInfo(module);

            var extendedDependencies = ModuleSorter.GetDependentModulesOf(extendedSourceList, extendedModuleInfo).Except(new [] { extendedModuleInfo });
            var dependencies = extendedDependencies.Select(em => sourceList.Find(m => ModuleInfoWrapper.Create(m).Id == em.Id));

            var castedItems = CastMethod.Invoke(null, new object[] { dependencies });
            __result = (IEnumerable) ToListMethod.Invoke(null, new object[] { castedItems });
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
        private static bool AreAllDependenciesOfModulePresentPrefix(LauncherModsVM __instance, object info, List<object> ____modulesCache, ref bool __result)
        {
            if (!BUTRLoaderAppDomainManager.ExtendedSorting)
                return true;

            var info2 = GetExtendedModuleInfo(info);

            // External dependencies should not disable a mod. Instead, they itself should be disabled
            __result = true;
            var visited = new HashSet<ModuleInfo2>();
            foreach (var dependency in ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, info2, visited, true))
            {
                if (!ModuleIsCorrect(__instance, dependency, ____modulesCache, visited))
                {
                    if (dependency != info2)
                        AppendIssue(__instance, info2, $"'{dependency.Name}' has unresolved issues!");

                    __result = false;
                    return false;
                }
            }
            return false;
        }
        private static bool AreAllDependenciesOfModulePresent(LauncherModsVM launcherModsVM, object moduleInfo, List<object> modulesCache)
        {
            var result = false;
            AreAllDependenciesOfModulePresentPrefix(launcherModsVM, moduleInfo, modulesCache, ref result);
            return result;
        }
        private static bool ModuleIsCorrect(LauncherModsVM instance, ModuleInfo2 moduleInfo2, List<object> modules, HashSet<ModuleInfo2> visited)
        {
            // Check that all dependencies are present
            foreach (var dependency in moduleInfo2.DependedModules)
            {
                if (!ExtendedModuleInfoCache.ContainsKey(dependency.ModuleId))
                {
                    AppendIssue(instance, moduleInfo2, $"Missing {dependency.ModuleId} {dependency.Version}");
                    return false;
                }
            }
            foreach (var metadata in moduleInfo2.DependedModuleMetadatas)
            {
                // Ignore the check for Optional
                if (metadata.IsOptional) continue;

                if (metadata.IsIncompatible) continue;

                if (!ExtendedModuleInfoCache.ContainsKey(metadata.Id))
                {
                    if (metadata.Version != ApplicationVersionHelper.Empty)
                        AppendIssue(instance, moduleInfo2, $"Missing {metadata.Id} {metadata.Version}");
                    if (metadata.VersionRange != ApplicationVersionRange.Empty)
                        AppendIssue(instance, moduleInfo2, $"Missing {metadata.Id} {metadata.VersionRange}");
                    return false;
                }
            }

            // Check that the dependencies themselves have all dependencies present
            foreach (var dependency in ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, moduleInfo2, visited).ToArray())
            {
                var metadata = moduleInfo2.DependedModuleMetadatas.Find(dmm => dmm.Id == dependency.Id);

                // Handle only direct dependencies
                if (metadata.LoadType != LoadType.LoadBeforeThis) continue;

                // Ignore the check for Optional
                if (metadata.IsOptional) continue;

                // Ignore the check for Incompatible
                if (metadata.IsIncompatible) continue;

                var module = modules.Find(m => ModuleInfoWrapper.Create(m).Id == dependency.Id);
                var moduleIsCorrect = ModuleIsCorrect(instance, GetExtendedModuleInfo(module), modules, visited);
                //if (!moduleIsCorrect)
                //    return false;
                // Handle only direct dependencies
                if (metadata.LoadType != LoadType.LoadAfterThis && !moduleIsCorrect)
                {
                    AppendIssue(instance, moduleInfo2, $"'{dependency.Name}' has unresolved issues!");
                    return false;
                }
            }

            // Check that the dependencies have the minimum required version set by DependedModuleMetadatas
            var comparer = new ApplicationVersionFullComparer();
            foreach (var metadata in moduleInfo2.DependedModuleMetadatas.Where(m => /*!m.IsOptional &&*/ !m.IsIncompatible))
            {
                // Handle only direct dependencies
                if (metadata.LoadType != LoadType.LoadBeforeThis) continue;

                // Ignore the check for non-provided versions
                if (metadata.Version == ApplicationVersionHelper.Empty && metadata.VersionRange == ApplicationVersionRange.Empty) continue;

                var dependedModule = ExtendedModuleInfoCache[metadata.Id];

                if (metadata.Version != ApplicationVersion.Empty)
                {
                    // dependedModuleMetadata.Version > dependedModule.Version
                    if (!metadata.IsOptional && (comparer.Compare(metadata.Version, dependedModule.Version) > 0))
                    {
                        AppendIssue(instance, moduleInfo2, $"'{dependedModule.Name}' wrong version <= {metadata.Version}");
                        return false;
                    }
                }

                if (metadata.VersionRange != ApplicationVersionRange.Empty)
                {
                    // dependedModuleMetadata.Version > dependedModule.VersionRange.Min
                    // dependedModuleMetadata.Version < dependedModule.VersionRange.Max
                    if (!metadata.IsOptional)
                    {
                        if (comparer.Compare(metadata.VersionRange.Min, dependedModule.Version) > 0)
                        {
                            AppendIssue(instance, moduleInfo2, $"'{dependedModule.Name}' wrong version < [{metadata.VersionRange}]");
                            return false;
                        }
                        if (comparer.Compare(metadata.VersionRange.Max, dependedModule.Version) < 0)
                        {
                            AppendIssue(instance, moduleInfo2, $"'{dependedModule.Name}' wrong version > [{metadata.VersionRange}]");
                            return false;
                        }
                    }
                }
            }

            // Do not load this mod if an incompatible mod is selected
            foreach (var metadata in moduleInfo2.DependedModuleMetadatas.Where(m => m.IsIncompatible))
            {
                var moduleVM2 = instance.Modules.FirstOrDefault(m => LauncherModuleVMWrapper.Create(m).Info.Id == metadata.Id);

                // If the incompatible mod is selected, this mod is disabled
                if (moduleVM2?.IsSelected == true)
                {
                    AppendIssue(instance, moduleInfo2, $"'{moduleVM2.Name}' is incompatible with this");
                    return false;
                }
            }

            // If another mod declared incompatibility and is selected, disable this
            foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
            {
                if (key == moduleInfo2.Id) continue;

                if (!moduleInfo.IsSelected) continue;

                foreach (var metadata in moduleInfo.DependedModuleMetadatas.Where(m => m.IsIncompatible && m.Id == moduleInfo2.Id))
                {
                    var moduleVM2 = instance.Modules.FirstOrDefault(m => LauncherModuleVMWrapper.Create(m).Info.Id == key);

                    // If the incompatible mod is selected, this mod is disabled
                    if (moduleVM2?.IsSelected == true)
                    {
                        AppendIssue(instance, moduleInfo2, $"'{moduleVM2.Name}' is incompatible with this");
                        return false;
                    }
                }
            }

            ClearIssues(instance, moduleInfo2);
            return true;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool ChangeIsSelectedOfPrefix(LauncherModsVM __instance, LauncherModuleVM targetModule, List<object> ____modulesCache)
        {
            if (!BUTRLoaderAppDomainManager.ExtendedSorting)
                return true;

            if (!AreAllDependenciesOfModulePresent(__instance, LauncherModuleVMWrapper.Create(targetModule).Info.Object, ____modulesCache))
            {
                // Direct and current External Dependencies are not valid, do nothing
                return false;
            }

            var visited = new HashSet<ModuleInfo2>();
            ChangeIsSelectedOf(__instance, targetModule, ____modulesCache, visited);

            return false;
        }
        private static void ChangeIsSelectedOf(LauncherModsVM instance, LauncherModuleVM targetModule, List<object> modules, HashSet<ModuleInfo2> visited)
        {
            var targetModuleInfo2 = GetExtendedModuleInfo(LauncherModuleVMWrapper.Create(targetModule).Info);
            var dependencies = ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, targetModuleInfo2, visited, skipExternalDependencies: true).ToArray();

            targetModule.IsSelected = !targetModule.IsSelected;

            if (targetModule.IsSelected)
            {
                // Vanilla check
                // Select all dependencies if they are not selected
                foreach (var module in instance.Modules)
                {
                    var moduleId = LauncherModuleVMWrapper.Create(module).Info.Id;
                    if (!module.IsSelected && dependencies.Any(d => d.Id == moduleId))
                        ChangeIsSelectedOf(instance, module, modules, visited);
                        //module.IsSelected |= allDependencies.Any(id => id == module.Info.Id);
                }

                // Deselect and disable any mod that is incompatible with this one
                foreach (var dmm in targetModuleInfo2.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible))
                {
                    var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => LauncherModuleVMWrapper.Create(m).Info.Id == dmm.Id);
                    if (incompatibleModuleVM is not null)
                    {
                        if (incompatibleModuleVM.IsSelected)
                            ChangeIsSelectedOf(instance, incompatibleModuleVM, modules, visited);

                        incompatibleModuleVM.IsDisabled = true;
                        AppendIssue(incompatibleModuleVM, $"'{targetModuleInfo2.Name}' is incompatible with this");
                    }
                }

                // Disable any mod that declares this mod as incompatible
                foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
                {
                    foreach (var dmm in moduleInfo.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible && dmm.Id == targetModuleInfo2.Id))
                    {
                        var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => LauncherModuleVMWrapper.Create(m).Info.Id == key);
                        if (incompatibleModuleVM is not null)
                        {
                            if (incompatibleModuleVM.IsSelected)
                                ChangeIsSelectedOf(instance, incompatibleModuleVM, modules, visited);

                            // We need to re-check that everything is alright with the external dependency
                            incompatibleModuleVM.IsDisabled |= !AreAllDependenciesOfModulePresent(instance, LauncherModuleVMWrapper.Create(incompatibleModuleVM).Info.Object, modules);
                        }
                    }
                }
            }
            else
            {
                // Vanilla check
                // Deselect all modules that depend on this module if they are selected
                foreach (var module in instance.Modules/*.Where(m => !m.IsOfficial)*/)
                {
                    var moduleInfo2 = GetExtendedModuleInfo(LauncherModuleVMWrapper.Create(module).Info);
                    var dependencies2 = ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, moduleInfo2, skipExternalDependencies: true);
                    if (module.IsSelected && dependencies2.Any(d => d.Id == targetModuleInfo2.Id))
                        ChangeIsSelectedOf(instance, module, modules, visited);
                }

                // Enable for selection any mod that is incompatible with this one
                foreach (var dmm in targetModuleInfo2.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible))
                {
                    var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => LauncherModuleVMWrapper.Create(m).Info.Id == dmm.Id);
                    if (incompatibleModuleVM is not null )
                    {
                        incompatibleModuleVM.IsDisabled = false;
                        ClearIssues(incompatibleModuleVM);
                    }
                }

                // Check if any mod that declares this mod as incompatible can be Enabled
                foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
                {
                    foreach (var dmm in moduleInfo.DependedModuleMetadatas.Where(dmm => dmm.IsIncompatible && dmm.Id == targetModuleInfo2.Id))
                    {
                        var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => LauncherModuleVMWrapper.Create(m).Info.Id == key);
                        if (incompatibleModuleVM is not null)
                        {
                            // We need to re-check that everything is alright with the external dependency
                            incompatibleModuleVM.IsDisabled &= !AreAllDependenciesOfModulePresent(instance, LauncherModuleVMWrapper.Create(incompatibleModuleVM).Info.Object, modules);
                        }
                    }
                }
            }

            //targetModule.IsDisabled = !AreAllDependenciesOfModulePresent(instance, targetModule.Info, modules);
        }


        public static void AppendIssue(LauncherModsVM viewModel, ModuleInfo2 moduleInfo2, string issue)
        {
            //viewModel.ExecuteCommand("SetIssue", new object[] { moduleInfo2.Id,  new string[] { issue } });
            SetIssue(moduleInfo2.Id,  new string[] { issue });

            var moduleVM = viewModel.Modules.FirstOrDefault(m => LauncherModuleVMWrapper.Create(m).Info.Id == moduleInfo2.Id);
            moduleVM?.ExecuteCommand("UpdateIssues", Array.Empty<object>());
        }
        public static void AppendIssue(LauncherModuleVM viewModel, string issue)
        {
            var id = LauncherModuleVMWrapper.Create(viewModel).Info.Id;
            SetIssue(id,  new[] { issue });

            viewModel.ExecuteCommand("UpdateIssues", Array.Empty<object>());
        }
        public static void ClearIssues(LauncherModsVM viewModel, ModuleInfo2 moduleInfo2)
        {
            //Issues.Remove(moduleInfo2.Id);
            if (Issues.TryGetValue(moduleInfo2.Id, out var list))
                list.Clear();

            var moduleVM = viewModel.Modules.FirstOrDefault(m => LauncherModuleVMWrapper.Create(m).Info.Id == moduleInfo2.Id);
            moduleVM?.ExecuteCommand("UpdateIssues", Array.Empty<object>());
        }
        public static void ClearIssues(LauncherModuleVM viewModel)
        {
            var id = LauncherModuleVMWrapper.Create(viewModel).Info.Id;

            //Issues.Remove(moduleInfo2.Id);
            if (Issues.TryGetValue(id, out var list))
                list.Clear();

            viewModel.ExecuteCommand("UpdateIssues", Array.Empty<object>());
        }

        public static Dictionary<string, HashSet<string>> Issues = new();
        private static void SetIssue(string moduleId, string[] issues)
        {
            if (Issues.TryGetValue(moduleId, out var list))
            {
                foreach (var issue in issues)
                {
                    if (!list.Contains(issue))
                        list.Add(issue);
                }
            }
            else
            {
                Issues.Add(moduleId, new HashSet<string>(issues));
            }
        }
    }
}