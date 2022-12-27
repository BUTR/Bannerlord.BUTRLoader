using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.ViewModels;
using Bannerlord.ModuleManager;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Mixins
{
    internal sealed partial class LauncherModsVMMixin
    {
        private class ModuleContext
        {
            private readonly IDictionary<string, BUTRLauncherModuleVM> _lookup;
            public ModuleContext(IEnumerable<BUTRLauncherModuleVM> moduleVMs)
            {
                _lookup = moduleVMs.ToDictionary(x => x.ModuleInfoExtended.Id, x => x);
            }
            public ModuleContext(IDictionary<string, BUTRLauncherModuleVM> lookup)
            {
                _lookup = lookup;
            }

            public bool GetIsValid(ModuleInfoExtended module)
            {
                if (FeatureIds.LauncherFeatures.Contains(module.Id))
                    return true;

                return _lookup[module.Id].IsValid;
            }
            public bool GetIsSelected(ModuleInfoExtended module)
            {
                if (FeatureIds.LauncherFeatures.Contains(module.Id))
                    return false;

                return _lookup[module.Id].IsSelected;
            }
            public void SetIsSelected(ModuleInfoExtended module, bool value)
            {
                if (FeatureIds.LauncherFeatures.Contains(module.Id))
                    return;
                _lookup[module.Id].IsSelected = value;
            }
            public bool GetIsDisabled(ModuleInfoExtended module)
            {
                if (FeatureIds.LauncherFeatures.Contains(module.Id))
                    return false;

                return _lookup[module.Id].IsDisabled;
            }
            public void SetIsDisabled(ModuleInfoExtended module, bool value)
            {
                if (FeatureIds.LauncherFeatures.Contains(module.Id))
                    return;
                _lookup[module.Id].IsDisabled = value;
            }
        }

        private static void ToggleModuleSelectionInternal(IEnumerable<BUTRLauncherModuleVM> moduleVMs, IDictionary<string, BUTRLauncherModuleVM> lookup, BUTRLauncherModuleVM moduleVM)
        {
            var modules = moduleVMs.Select(x => x.ModuleInfoExtended).ToList();
            var ctx = new ModuleContext(lookup);

            if (moduleVM.IsSelected)
                ModuleUtilities.DisableModule(modules, moduleVM.ModuleInfoExtended, ctx.GetIsSelected, ctx.SetIsSelected, ctx.GetIsDisabled, ctx.SetIsDisabled);
            else
                ModuleUtilities.EnableModule(modules, moduleVM.ModuleInfoExtended, ctx.GetIsSelected, ctx.SetIsSelected, ctx.GetIsDisabled, ctx.SetIsDisabled);
        }

        private static IEnumerable<ModuleIssue> ValidateModuleInternal(IEnumerable<BUTRLauncherModuleVM> moduleVMs, IDictionary<string, BUTRLauncherModuleVM> lookup, BUTRLauncherModuleVM moduleVM)
        {
            var modules = moduleVMs.Select(x => x.ModuleInfoExtended).Concat(FeatureIds.LauncherFeatures.Select(x => new ModuleInfoExtended { Id = x })).ToList();
            var ctx = new ModuleContext(lookup);

            return ModuleUtilities.ValidateModule(modules, moduleVM.ModuleInfoExtended, ctx.GetIsSelected, ctx.GetIsValid);
        }

        private static void SortByDefaultInternal(MBBindingList<BUTRLauncherModuleVM> modules)
        {
            static IEnumerable<ModuleInfoExtended> Sort(IEnumerable<ModuleInfoExtended> source)
            {
                var orderedModules = source
                    .OrderByDescending(x => x.IsOfficial)
                    .ThenBy(x => x.Id, new AlphanumComparatorFast())
                    .ToArray();

                return ModuleSorter.TopologySort(orderedModules, module => ModuleUtilities.GetDependencies(orderedModules, module));
            }

            var sorted = Sort(modules.Select(x => x.ModuleInfoExtended)).Select((x, i) => new { Item = x.Id, Index = i }).ToDictionary(x => x.Item, x => x.Index);
            modules.Sort(new ByIndexComparer<BUTRLauncherModuleVM>(x => sorted.TryGetValue(x.ModuleInfoExtended.Id, out var idx) ? idx : -1));
            //var sorted = Sort(Modules2.Select(x => x.ModuleInfoExtended)).Select(x => x.Id).ToList();
            //SortBy(sorted);
        }

        private static bool ChangeModulePositionInternal(MBBindingList<BUTRLauncherModuleVM> moduleVMs, IDictionary<string, BUTRLauncherModuleVM> lookup, BUTRLauncherModuleVM targetModuleVM, int insertIndex, Action<IReadOnlyCollection<ModuleIssue>>? onIssues = null)
        {
            if (insertIndex >= moduleVMs.IndexOf(targetModuleVM)) insertIndex--;
            insertIndex = (int) MathF.Clamp(insertIndex, 0f, moduleVMs.Count - 1);
            var currentModuleIndex = moduleVMs.IndexOf(targetModuleVM);
            if (insertIndex == -1 || currentModuleIndex == -1) return false;

            var modules = moduleVMs.Select(x => x.ModuleInfoExtended).ToList();
            var issuesReported = false;
            while (insertIndex != currentModuleIndex)
            {
                modules.RemoveAt(currentModuleIndex);
                modules.Insert(insertIndex, targetModuleVM.ModuleInfoExtended);
                var loadOrderValidationIssues = LoadOrderChecker.IsLoadOrderCorrect(modules.Where(x => lookup[x.Id] is { IsValid: true }).ToList()).ToList();
                if (loadOrderValidationIssues.Count == 0)
                {
                    moduleVMs.RemoveAt(currentModuleIndex);
                    moduleVMs.Insert(insertIndex, targetModuleVM);
                    return true;
                }

                if (!issuesReported)
                {
                    issuesReported = true;
                    onIssues?.Invoke(loadOrderValidationIssues);
                }

                // Do it until we find the nearest acceptable index or stop if we failes
                modules.RemoveAt(insertIndex);
                modules.Insert(currentModuleIndex, targetModuleVM.ModuleInfoExtended);

                if (currentModuleIndex < insertIndex) insertIndex--;
                if (currentModuleIndex > insertIndex) insertIndex++;
            }

            return false;
        }

        private IEnumerable<string> TryOrderByLoadOrderTW(IEnumerable<string> loadOrder, Func<string, bool> isModuleSelected)
        {
            var semiOrderedModules = new List<ModuleInfoExtendedWithMetadata>();

            // Load the load order modules
            foreach (var id in loadOrder)
            {
                if (!_extendedModuleInfoCache.TryGetValue(id, out var moduleInfoExtended)) continue;
                if (!IsVisible(IsSingleplayer, moduleInfoExtended)) continue;
                if (moduleInfoExtended is not ModuleInfoExtendedWithMetadata moduleInfoExtendedWithMetadata) continue;

                semiOrderedModules.Add(moduleInfoExtendedWithMetadata);
            }
            // Load the rest of modules
            foreach (var moduleInfoExtended in _extendedModuleInfoCache.Values)
            {
                if (semiOrderedModules.Contains(moduleInfoExtended)) continue;
                if (!IsVisible(IsSingleplayer, moduleInfoExtended)) continue;
                if (moduleInfoExtended is not ModuleInfoExtendedWithMetadata moduleInfoExtendedWithMetadata) continue;

                semiOrderedModules.Add(moduleInfoExtendedWithMetadata);
            }

            // Topological sort them
            var orderedModules = ModuleSorter.TopologySort<ModuleInfoExtended>(semiOrderedModules, x => ModuleUtilities.GetDependencies(semiOrderedModules, x))
                .OfType<ModuleInfoExtendedWithMetadata>()
                .Select(x => new BUTRLauncherModuleVM(x, ToggleModuleSelection, ValidateModule))
                .ToList();
            var lookup = orderedModules.ToDictionary(x => x.ModuleInfoExtended.Id, x => x);

            // Toggle IsSelected
            foreach (var moduleVM in orderedModules)
            {
                if (isModuleSelected(moduleVM.ModuleInfoExtended.Id) && !moduleVM.IsSelected)
                    ToggleModuleSelectionInternal(orderedModules, lookup, moduleVM);
            }

            var issues = LoadOrderChecker.IsLoadOrderCorrect(orderedModules.Select(x => x.ModuleInfoExtended).ToList()).ToList();
            if (issues.Count != 0) return issues.Select(x => x.Reason);

            OverrideModuleVMs(orderedModules);
            return Enumerable.Empty<string>();
        }

        private IEnumerable<string> TryOrderByLoadOrderBeta(IEnumerable<string> loadOrder, Func<string, bool> isModuleSelected)
        {
            var semiOrderedModules = new List<ModuleInfoExtendedWithMetadata>();

            // Load all modules
            foreach (var moduleInfoExtended in _extendedModuleInfoCache.Values)
            {
                if (!IsVisible(IsSingleplayer, moduleInfoExtended)) continue;
                if (moduleInfoExtended is not ModuleInfoExtendedWithMetadata moduleInfoExtendedWithMetadata) continue;

                semiOrderedModules.Add(moduleInfoExtendedWithMetadata);
            }

            // Get all present modules, ignore missing
            var presentOrderedIds = loadOrder.Intersect(semiOrderedModules.Select(x => x.Id).ToHashSet()).ToList();

            // Check the present load order
            var loadOrderValidationIssues = LoadOrderChecker.IsLoadOrderCorrect(presentOrderedIds.Select(x => _extendedModuleInfoCache[x]).ToList()).ToList();
            if (loadOrderValidationIssues.Count != 0)
                return loadOrderValidationIssues.Select(x => x.Reason);

            var orderedModules = semiOrderedModules
                .Select(x => new BUTRLauncherModuleVM(x, ToggleModuleSelection, ValidateModule))
                .ToMBBindingList();
            var lookup = orderedModules.ToDictionary(x => x.ModuleInfoExtended.Id, x => x);

            // Toggle IsSelected
            foreach (var moduleVM in orderedModules)
            {
                if (isModuleSelected(moduleVM.ModuleInfoExtended.Id) && !moduleVM.IsSelected)
                    ToggleModuleSelectionInternal(orderedModules, lookup, moduleVM);
            }

            SortByDefaultInternal(orderedModules);

            // Not even sure a loop is needed
            // And I'm pretty sure that this is a dumb and non optimal solution.
            // ChangeModulePosition should move any nested dependencies higher?
            var hasInvalid = true;
            var retryCount = 0;
            var retryCountMax = presentOrderedIds.Count + 1;
            while (hasInvalid && retryCount < retryCountMax)
            {
                hasInvalid = false;
                retryCount++;
                for (var i = 0; i < presentOrderedIds.Count - 1; i++)
                {
                    var xId = presentOrderedIds[i];
                    var yId = presentOrderedIds[i + 1];

                    var xIdx = orderedModules.IndexOf(z => z.ModuleInfoExtended.Id == xId);
                    var yIdx = orderedModules.IndexOf(z => z.ModuleInfoExtended.Id == yId);
                    if (xIdx > yIdx)
                    {
                        if (!ChangeModulePositionInternal(orderedModules, lookup, lookup[xId], yIdx))
                        {
                            if (!ChangeModulePositionInternal(orderedModules, lookup, lookup[yId], xIdx))
                            {
                                hasInvalid = true;
                            }
                        }
                    }
                }
            }

            if (retryCount >= retryCountMax)
                return new[] { "Failed to order the module list!" };

            OverrideModuleVMs(orderedModules);
            return Enumerable.Empty<string>();
        }
    }
}