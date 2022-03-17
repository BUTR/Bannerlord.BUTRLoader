using Bannerlord.BUTR.Shared.Extensions;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.LauncherEx;
using Bannerlord.BUTRLoader.LauncherEx.Helpers;
using Bannerlord.ModuleManager;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;

using static Bannerlord.BUTRLoader.Helpers.ModuleInfoHelper2;

using ApplicationVersion = Bannerlord.ModuleManager.ApplicationVersion;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherModsVMPatch
    {
        internal delegate IEnumerable CastDelegate(IEnumerable instance);
        internal delegate IEnumerable ToListDelegate(IEnumerable instance);

        internal static readonly CastDelegate? CastMethod = AccessTools2.GetDelegate<CastDelegate>(typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))?.MakeGenericMethod(ModuleInfoWrapper.ModuleInfoType)!);
        internal static readonly ToListDelegate? ToListMethod = AccessTools2.GetDelegate<ToListDelegate>(typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))?.MakeGenericMethod(ModuleInfoWrapper.ModuleInfoType)!);

        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(LauncherModsVMWrapper.LauncherModsVMType!, "GetDependentModulesOf"),
                prefix: AccessTools2.Method(typeof(LauncherModsVMPatch), nameof(GetDependentModulesOfPrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                AccessTools2.Method(LauncherModsVMWrapper.LauncherModsVMType!, "IsAllDependenciesOfModulePresent"),
                prefix: AccessTools2.Method(typeof(LauncherModsVMPatch), nameof(AreAllDependenciesOfModulePresentPrefix)));
            if (!res2) return false;

            var res3 = harmony.TryPatch(
                AccessTools2.Method(LauncherModsVMWrapper.LauncherModsVMType!, "ChangeIsSelectedOf"),
                prefix: AccessTools2.Method(typeof(LauncherModsVMPatch), nameof(ChangeIsSelectedOfPrefix)));
            if (!res3) return false;

            return true;
        }

        public static void Disable(Harmony harmony)
        {
            harmony.Unpatch(
                AccessTools2.Method(LauncherModsVMWrapper.LauncherModsVMType!, "GetDependentModulesOf"),
                AccessTools2.Method(typeof(LauncherModsVMPatch), nameof(GetDependentModulesOfPrefix)));

            harmony.Unpatch(
                AccessTools2.Method(LauncherModsVMWrapper.LauncherModsVMType!, "IsAllDependenciesOfModulePresent"),
                AccessTools2.Method(typeof(LauncherModsVMPatch), nameof(AreAllDependenciesOfModulePresentPrefix)));

            harmony.Unpatch(
                AccessTools2.Method(LauncherModsVMWrapper.LauncherModsVMType!, "ChangeIsSelectedOf"),
                AccessTools2.Method(typeof(LauncherModsVMPatch), nameof(ChangeIsSelectedOfPrefix)));
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetDependentModulesOfPrefix(IEnumerable<object> source, object module, ref IEnumerable __result)
        {
            if (!LauncherSettings.ExtendedSorting || CastMethod is null || ToListMethod is null)
                return true;

            var extendedModuleInfo = GetExtendedModuleInfo(module);
            if (extendedModuleInfo is null) return true; // Exception

            var sourceList = source.ToList();
            var extendedSourceList = sourceList.ConvertAll(GetExtendedModuleInfo);
            // Any incorrect module will be null. Filter it out
            while (extendedSourceList.IndexOf(null!) is var idx && idx != -1)
            {
                sourceList.RemoveAt(idx);
                extendedSourceList.RemoveAt(idx);
            }

            var extendedDependencies = ModuleSorter.GetDependentModulesOf(extendedSourceList, extendedModuleInfo).Except(new[] { extendedModuleInfo });
            var dependencies = extendedDependencies.Select(em => sourceList.Find(m => ModuleInfoWrapper.Create(m).Id == em.Id));

            var castedItems = CastMethod.Invoke(dependencies);
            __result = ToListMethod.Invoke(castedItems);
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
        private static bool AreAllDependenciesOfModulePresentPrefix(object __instance, object info, List<object> ____modulesCache, ref bool __result)
        {
            if (!LauncherSettings.ExtendedSorting)
                return true;

            var wrapped = LauncherModsVMWrapper.Create(__instance);
            var info2 = GetExtendedModuleInfo(info);
            if (info2 is null) // Any incorrect module will be null. Filter it out
            {
                __result = false;
                return false;
            }

            // External dependencies should not disable a mod. Instead, they itself should be disabled
            __result = true;
            var visited = new HashSet<ModuleInfoExtended>();
            foreach (var dependency in ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, info2, visited, new ModuleSorterOptions { SkipOptionals = true, SkipExternalDependencies = true }))
            {
                if (!ModuleIsCorrect(wrapped, dependency, ____modulesCache, visited))
                {
                    if (dependency != info2)
                        AppendIssue(wrapped, info2, $"'{dependency.Name}' has unresolved issues!");

                    __result = false;
                    return false;
                }
            }
            return false;
        }
        private static bool AreAllDependenciesOfModulePresent(object? launcherModsVM, object? moduleInfo, List<object> modulesCache)
        {
            var result = false;
            AreAllDependenciesOfModulePresentPrefix(launcherModsVM, moduleInfo, modulesCache, ref result);
            return result;
        }
        private static bool CheckModuleCompatibility(LauncherModsVMWrapper instance, ModuleInfoExtended moduleInfoExtended)
        {
            static bool CheckIfSubModuleCanBeLoaded(SubModuleInfoExtended subModuleInfo)
            {
                if (subModuleInfo.Tags.Count > 0)
                {
                    foreach (var (key, values) in subModuleInfo.Tags)
                    {
                        if (!Enum.TryParse<SubModuleTags>(key, out var tag))
                            continue;

                        foreach (var value in values)
                        {
                            if (!GetSubModuleTagValiditiy(tag, value))
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                return true;
            }
            static bool GetSubModuleTagValiditiy(SubModuleTags tag, string value) => tag switch
            {
                SubModuleTags.RejectedPlatform => !Enum.TryParse<Platform>(value, out var platform) || ApplicationPlatform.CurrentPlatform != platform,
                SubModuleTags.ExclusivePlatform => !Enum.TryParse<Platform>(value, out var platform) || ApplicationPlatform.CurrentPlatform == platform,
                SubModuleTags.DependantRuntimeLibrary => !Enum.TryParse<Runtime>(value, out var runtime) || ApplicationPlatform.CurrentRuntimeLibrary == runtime,
                SubModuleTags.IsNoRenderModeElement => value.Equals("false"),
                SubModuleTags.DedicatedServerType => value.ToLower() switch
                {
                    "none" => true,
                    _ => false
                },
                _ => true
            };

            foreach (var subModule in moduleInfoExtended.SubModules.Where(CheckIfSubModuleCanBeLoaded))
            {
                var asm = Path.GetFullPath(Path.Combine(BasePath.Name, "Modules", moduleInfoExtended.Id, "bin", "Win64_Shipping_Client", subModule.DLLName));
                switch (Manager._compatibilityChecker.CheckAssembly(asm))
                {
                    case CheckResult.TypeLoadException:
                        AppendIssue(instance, moduleInfoExtended, "Not binary compatible with the current game version!");
                        return false;
                    case CheckResult.ReflectionTypeLoadException:
                        AppendIssue(instance, moduleInfoExtended, "Not binary compatible with the current game version!");
                        return false;
                    case CheckResult.GenericException:
                        AppendIssue(instance, moduleInfoExtended, "There was an error checking for binary compatibility with the current game version");
                        return false;
                }
            }

            return true;
        }
        private static bool ModuleIsCorrect(LauncherModsVMWrapper instance, ModuleInfoExtended moduleInfoExtended, List<object> modules, HashSet<ModuleInfoExtended> visited)
        {
            if (!CheckModuleCompatibility(instance, moduleInfoExtended))
                return false;

            // Check that all dependencies are present
            foreach (var dependency in moduleInfoExtended.DependentModules)
            {
                if (dependency.IsOptional) continue;

                if (!ExtendedModuleInfoCache.ContainsKey(dependency.Id))
                {
                    AppendIssue(instance, moduleInfoExtended, $"Missing {dependency.Id} {dependency.Version}");
                    return false;
                }
            }
            foreach (var metadata in moduleInfoExtended.DependentModuleMetadatas)
            {
                // Ignore the check for Optional
                if (metadata.IsOptional) continue;

                if (metadata.IsIncompatible) continue;

                if (!ExtendedModuleInfoCache.ContainsKey(metadata.Id))
                {
                    if (metadata.Version != ApplicationVersion.Empty)
                        AppendIssue(instance, moduleInfoExtended, $"Missing {metadata.Id} {metadata.Version}");
                    if (metadata.VersionRange != ApplicationVersionRange.Empty)
                        AppendIssue(instance, moduleInfoExtended, $"Missing {metadata.Id} {metadata.VersionRange}");
                    return false;
                }
            }

            // Check that the dependencies themselves have all dependencies present
            foreach (var dependency in ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, moduleInfoExtended, visited, new ModuleSorterOptions { SkipOptionals = true, SkipExternalDependencies = true }).ToArray())
            {
                var metadata = moduleInfoExtended.DependentModuleMetadatas.FirstOrDefault(dmm => dmm.Id == dependency.Id);

                // Not found
                if (metadata is null) continue;

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
                    AppendIssue(instance, moduleInfoExtended, $"'{dependency.Name}' has unresolved issues!");
                    return false;
                }
            }

            // Check that the dependencies have the minimum required version set by DependedModuleMetadatas
            var comparer = new ApplicationVersionComparer();
            foreach (var metadata in moduleInfoExtended.DependentModuleMetadatas.Where(m => /*!m.IsOptional &&*/ !m.IsIncompatible))
            {
                // Handle only direct dependencies
                if (metadata.LoadType != LoadType.LoadBeforeThis) continue;

                // Ignore the check for non-provided versions
                if (metadata.Version == ApplicationVersion.Empty && metadata.VersionRange == ApplicationVersionRange.Empty) continue;

                if (!ExtendedModuleInfoCache.TryGetValue(metadata.Id, out var dependedModule) && metadata.IsOptional) continue;

                if (metadata.Version != ApplicationVersion.Empty)
                {
                    // dependedModuleMetadata.Version > dependedModule.Version
                    if (!metadata.IsOptional && (comparer.Compare(metadata.Version, dependedModule?.Version) > 0))
                    {
                        AppendIssue(instance, moduleInfoExtended, $"'{dependedModule?.Name}' wrong version <= {metadata.Version}");
                        return false;
                    }
                }

                if (metadata.VersionRange != ApplicationVersionRange.Empty)
                {
                    // dependedModuleMetadata.Version > dependedModule.VersionRange.Min
                    // dependedModuleMetadata.Version < dependedModule.VersionRange.Max
                    if (!metadata.IsOptional)
                    {
                        if (comparer.Compare(metadata.VersionRange.Min, dependedModule?.Version) > 0)
                        {
                            AppendIssue(instance, moduleInfoExtended, $"'{dependedModule?.Name}' wrong version < [{metadata.VersionRange}]");
                            return false;
                        }
                        if (comparer.Compare(metadata.VersionRange.Max, dependedModule?.Version) < 0)
                        {
                            AppendIssue(instance, moduleInfoExtended, $"'{dependedModule?.Name}' wrong version > [{metadata.VersionRange}]");
                            return false;
                        }
                    }
                }
            }

            // Do not load this mod if an incompatible mod is selected
            foreach (var dependency in moduleInfoExtended.IncompatibleModules)
            {
                var moduleVM2 = instance.Modules.FirstOrDefault(m => m.Info?.Id == dependency.Id);

                // If the incompatible mod is selected, this mod is disabled
                if (moduleVM2?.IsSelected == true)
                {
                    AppendIssue(instance, moduleInfoExtended, $"'{moduleVM2.Name}' is incompatible with this");
                    return false;
                }
            }
            foreach (var metadata in moduleInfoExtended.DependentModuleMetadatas.Where(m => m.IsIncompatible))
            {
                var moduleVM2 = instance.Modules.FirstOrDefault(m => m.Info?.Id == metadata.Id);

                // If the incompatible mod is selected, this mod is disabled
                if (moduleVM2?.IsSelected == true)
                {
                    AppendIssue(instance, moduleInfoExtended, $"'{moduleVM2.Name}' is incompatible with this");
                    return false;
                }
            }

            // If another mod declared incompatibility and is selected, disable this
            foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
            {
                if (key == moduleInfoExtended.Id) continue;

                //if (!moduleInfo.IsSelected) continue;

                foreach (var metadata in moduleInfo.DependentModuleMetadatas.Where(m => m.IsIncompatible && m.Id == moduleInfoExtended.Id))
                {
                    var moduleVM2 = instance.Modules.FirstOrDefault(m => m.Info?.Id == key);

                    // If the incompatible mod is selected, this mod is disabled
                    if (moduleVM2?.IsSelected == true)
                    {
                        AppendIssue(instance, moduleInfoExtended, $"'{moduleVM2.Name}' is incompatible with this");
                        return false;
                    }
                }
            }

            ClearIssues(instance, moduleInfoExtended);
            return true;
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool ChangeIsSelectedOfPrefix(object __instance, object targetModule, List<object> ____modulesCache)
        {
            if (!LauncherSettings.ExtendedSorting)
                return true;

            var wrapped = LauncherModsVMWrapper.Create(__instance);
            var targetModuleWrapped = LauncherModuleVMWrapper.Create(targetModule);

            if (!AreAllDependenciesOfModulePresent(wrapped.Object, targetModuleWrapped.Info?.Object, ____modulesCache))
            {
                // Direct and current External Dependencies are not valid, do nothing
                return false;
            }

            var visited = new HashSet<ModuleInfoExtended>();
            ChangeIsSelectedOf(wrapped, targetModuleWrapped, ____modulesCache, visited);

            return false;
        }
        private static void ChangeIsSelectedOf(LauncherModsVMWrapper instance, LauncherModuleVMWrapper targetModule, List<object> modules, HashSet<ModuleInfoExtended> visited)
        {
            var opt = new ModuleSorterOptions { SkipOptionals = true, SkipExternalDependencies = true };

            var targetModuleInfoExtended = GetExtendedModuleInfo(targetModule?.Info);
            var dependencies = ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, targetModuleInfoExtended, visited, opt).ToArray();

            targetModule.IsSelected = !targetModule.IsSelected;

            if (targetModule.IsSelected)
            {
                // Vanilla check
                // Select all dependencies if they are not selected
                foreach (var module in instance.Modules)
                {
                    var moduleId = module.Info?.Id ?? string.Empty;
                    if (!module.IsSelected && dependencies.Any(d => d.Id == moduleId))
                        ChangeIsSelectedOf(instance, module, modules, visited);
                    //module.IsSelected |= allDependencies.Any(id => id == module.Info.Id);
                }

                // Deselect and disable any mod that is incompatible with this one
                foreach (var dmm in targetModuleInfoExtended.DependentModuleMetadatas.Where(dmm => dmm.IsIncompatible))
                {
                    var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => m.Info?.Id == dmm.Id);
                    if (incompatibleModuleVM is not null)
                    {
                        if (incompatibleModuleVM.IsSelected)
                            ChangeIsSelectedOf(instance, incompatibleModuleVM, modules, visited);

                        incompatibleModuleVM.IsDisabled = true;
                        AppendIssue(incompatibleModuleVM, $"'{targetModuleInfoExtended.Name}' is incompatible with this");
                    }
                }

                // Disable any mod that declares this mod as incompatible
                foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
                {
                    foreach (var dmm in moduleInfo.DependentModuleMetadatas.Where(dmm => dmm.IsIncompatible && dmm.Id == targetModuleInfoExtended.Id))
                    {
                        var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => m.Info?.Id == key);
                        if (incompatibleModuleVM is not null)
                        {
                            if (incompatibleModuleVM.IsSelected)
                                ChangeIsSelectedOf(instance, incompatibleModuleVM, modules, visited);

                            // We need to re-check that everything is alright with the external dependency
                            incompatibleModuleVM.IsDisabled |= !AreAllDependenciesOfModulePresent(instance, incompatibleModuleVM.Info?.Object, modules);
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
                    var moduleInfoExtended = GetExtendedModuleInfo(module?.Info);
                    var dependencies2 = ModuleSorter.GetDependentModulesOf(ExtendedModuleInfoCache.Values, moduleInfoExtended, opt);
                    if (module.IsSelected && dependencies2.Any(d => d.Id == targetModuleInfoExtended.Id))
                        ChangeIsSelectedOf(instance, module, modules, visited);
                }

                // Enable for selection any mod that is incompatible with this one
                foreach (var dmm in targetModuleInfoExtended.DependentModuleMetadatas.Where(dmm => dmm.IsIncompatible))
                {
                    var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => m.Info?.Id == dmm.Id);
                    if (incompatibleModuleVM is not null)
                    {
                        incompatibleModuleVM.IsDisabled = false;
                        ClearIssues(incompatibleModuleVM);
                    }
                }

                // Check if any mod that declares this mod as incompatible can be Enabled
                foreach (var (key, moduleInfo) in ExtendedModuleInfoCache)
                {
                    foreach (var dmm in moduleInfo.DependentModuleMetadatas.Where(dmm => dmm.IsIncompatible && dmm.Id == targetModuleInfoExtended.Id))
                    {
                        var incompatibleModuleVM = instance.Modules.FirstOrDefault(m => m.Info?.Id == key);
                        if (incompatibleModuleVM is not null)
                        {
                            // We need to re-check that everything is alright with the external dependency
                            incompatibleModuleVM.IsDisabled &= !AreAllDependenciesOfModulePresent(instance, incompatibleModuleVM.Info?.Object, modules);
                        }
                    }
                }
            }

            //targetModule.IsDisabled = !AreAllDependenciesOfModulePresent(instance, targetModule.Info, modules);
        }


        public static void AppendIssue(LauncherModsVMWrapper viewModelWrapper, ModuleInfoExtended ModuleInfoExtended, string issue)
        {
            //viewModelWrapper.ExecuteCommand("SetIssue", new object[] { ModuleInfoExtended.Id,  new string[] { issue } });
            SetIssue(ModuleInfoExtended.Id, new[] { issue });

            var moduleVM = viewModelWrapper.Modules.FirstOrDefault(m => m.Info?.Id == ModuleInfoExtended.Id);
            moduleVM?.ExecuteCommand("UpdateIssues", Array.Empty<object>());
        }
        public static void AppendIssue(LauncherModuleVMWrapper viewModelWrapper, string issue)
        {
            var id = viewModelWrapper.Info?.Id ?? string.Empty;
            SetIssue(id, new[] { issue });

            viewModelWrapper.ExecuteCommand("UpdateIssues", Array.Empty<object>());
        }
        public static void ClearIssues(LauncherModsVMWrapper viewModelWrapper, ModuleInfoExtended ModuleInfoExtended)
        {
            //Issues.Remove(ModuleInfoExtended.Id);
            if (Issues.TryGetValue(ModuleInfoExtended.Id, out var list))
                list.Clear();

            var moduleVM = viewModelWrapper.Modules.FirstOrDefault(m => m.Info?.Id == ModuleInfoExtended.Id);
            moduleVM?.ExecuteCommand("UpdateIssues", Array.Empty<object>());
        }
        public static void ClearIssues(LauncherModuleVMWrapper viewModelWrapper)
        {
            var id = viewModelWrapper.Info?.Id ?? string.Empty;

            //Issues.Remove(ModuleInfoExtended.Id);
            if (Issues.TryGetValue(id, out var list))
                list.Clear();

            viewModelWrapper.ExecuteCommand("UpdateIssues", Array.Empty<object>());
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