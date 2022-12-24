using Bannerlord.BUTR.Shared.Extensions;
using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.ViewModels;
using Bannerlord.ModuleManager;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal enum ModuleType
    {
        Framework, Graphical, Standard, Patches

    }

    internal sealed class LauncherModsVMMixin : ViewModelMixin<LauncherModsVMMixin, LauncherModsVM>
    {
        private static readonly AccessTools.FieldRef<LauncherModsVM, UserDataManager>? _userDataManager =
            AccessTools2.FieldRefAccess<LauncherModsVM, UserDataManager>("_userDataManager");

        // All installed Modules
        private readonly Dictionary<string, ModuleInfoExtended> _extendedModuleInfoCache =
            // Not real modules, we declare this way our launcher capabilities
            new(FeatureIds.Features.ToDictionary(x => x, x => new ModuleInfoExtended { Id = x, IsSingleplayerModule = true }));

        // Fast lookup for the ViewModels
        public readonly Dictionary<string, BUTRLauncherModuleVM> Modules2Lookup = new();

        [BUTRDataSourceProperty]
        public bool GlobalCheckboxState { get => _checkboxState; set => SetField(ref _checkboxState, value, nameof(GlobalCheckboxState)); }
        private bool _checkboxState;

        [BUTRDataSourceProperty]
        public bool IsSingleplayer { get => _isSingleplayer; set => SetField(ref _isSingleplayer, value, nameof(IsSingleplayer)); }
        private bool _isSingleplayer;

        [BUTRDataSourceProperty]
        public bool IsDisabled2 { get => _isDisabled2; set => SetField(ref _isDisabled2, value, nameof(IsDisabled2)); }
        private bool _isDisabled2;

        [BUTRDataSourceProperty]
        public MBBindingList<BUTRLauncherModuleVM> Modules2 { get; } = new();


        [BUTRDataSourceProperty]
        public bool IsForceSorted
        {
            get => _isForceSorted;
            set
            {
                if (SetField(ref _isForceSorted, value, nameof(IsForceSorted)))
                {
                    OnPropertyChanged(nameof(IsNotForceSorted));
                }
            }
        }
        private bool _isForceSorted;

        [BUTRDataSourceProperty]
        public bool IsNotForceSorted => !IsForceSorted;

        [BUTRDataSourceProperty]
        public LauncherHintVM? ForceSortedHint { get => _forceSortedHint; set => SetField(ref _forceSortedHint, value, nameof(ForceSortedHint)); }
        private LauncherHintVM? _forceSortedHint;

        public string ModuleListCode => $"_MODULES_*{string.Join("*", Modules2.Where(x => x.IsSelected).Select(x => x.ModuleInfoExtended.Id))}*_MODULES_";

        public LauncherModsVMMixin(LauncherModsVM launcherModsVM) : base(launcherModsVM)
        {
            _extendedModuleInfoCache.AddRange(ModuleInfoHelper.GetModules().Cast<ModuleInfoExtended>().ToDictionary(x => x.Id, x => x));
        }

        [BUTRDataSourceMethod]
        public void ExecuteGlobalCheckbox()
        {
            GlobalCheckboxState = !GlobalCheckboxState;

            foreach (var moduleVM in Modules2)
            {
                if (GlobalCheckboxState)
                {
                    if (moduleVM.IsValid && !moduleVM.IsSelected)
                        ToggleModuleSelection(moduleVM);
                }
                else
                {
                    if (!moduleVM.ModuleInfoExtended.IsNative() && moduleVM.IsSelected)
                        ToggleModuleSelection(moduleVM);
                }
            }
        }

        [BUTRDataSourceMethod]
        public void ExecuteRefresh()
        {
            SortByDefault();
        }


        private bool GetIsValid(ModuleInfoExtended module)
        {
            if (FeatureIds.Features.Contains(module.Id))
                return true;

            return Modules2Lookup[module.Id].IsValid;
        }
        private bool GetIsSelected(ModuleInfoExtended module)
        {
            if (FeatureIds.Features.Contains(module.Id))
                return false;

            return Modules2Lookup[module.Id].IsSelected;
        }
        private void SetIsSelected(ModuleInfoExtended module, bool value)
        {
            if (FeatureIds.Features.Contains(module.Id))
                return;
            Modules2Lookup[module.Id].IsSelected = value;
        }
        private bool GetIsDisabled(ModuleInfoExtended module)
        {
            if (FeatureIds.Features.Contains(module.Id))
                return false;

            return Modules2Lookup[module.Id].IsDisabled;
        }
        private void SetIsDisabled(ModuleInfoExtended module, bool value)
        {
            if (FeatureIds.Features.Contains(module.Id))
                return;
            Modules2Lookup[module.Id].IsDisabled = value;
        }

        public void Initialize(bool isMultiplayer)
        {
            // Set IsSingleplayer2 for further use
            IsSingleplayer = !isMultiplayer;

            if (_userDataManager is null || ViewModel is null) return;

            var userDataManager = _userDataManager(ViewModel);

            // Clear previous data
            Modules2Lookup.Clear();
            Modules2.Clear();

            // Initialize ViewModels
            foreach (var moduleInfoExtended in _extendedModuleInfoCache.Values.Where(x => IsVisible(IsSingleplayer, x)).OfType<ModuleInfoExtendedWithMetadata>())
            {
                var viewModel = new BUTRLauncherModuleVM(moduleInfoExtended, ToggleModuleSelection, ValidateModule);
                Modules2Lookup[moduleInfoExtended.Id] = viewModel;
                Modules2.Add(viewModel);
            }

            var userGameTypeData = isMultiplayer ? userDataManager.UserData.MultiplayerData : userDataManager.UserData.SingleplayerData;

            // Apply saved IsSelected
            foreach (var userModData in userGameTypeData.ModDatas.Where(x => x.IsSelected))
            {
                if (!Modules2Lookup.TryGetValue(userModData.Id, out var moduleVM)) continue;

                if (!moduleVM.IsSelected && moduleVM.IsValid)
                    ToggleModuleSelection(moduleVM);
            }

            // Apply the default deterministic order
            SortByDefault();

            // Apply saved ordering
            var orderIssues = OrderBy(userGameTypeData.ModDatas.Select(x => x.Id).ToList()).ToList();
            if (orderIssues.Count != 0)
            {
                IsForceSorted = true;
                ForceSortedHint = new LauncherHintVM(@$"The Load Order was re-sorted! Reasons:
{string.Join(Environment.NewLine, orderIssues)}");
            }
            else
            {
                IsForceSorted = false;
            }

            // Validate all VM's after they were selected and ordered
            foreach (var modules in Modules2)
                modules.Validate();
        }

        // Tries to order the modules list with the relations described in the ordered list
        public IEnumerable<string> OrderBy(IReadOnlyList<string> orderedIds)
        {
            // Ignore missing modules
            var currentOrderedIds = orderedIds.Intersect(Modules2.Select(x => x.ModuleInfoExtended.Id).ToHashSet()).ToList();

            var loadOrderValidationIssues = IsLoadOrderCorrect(currentOrderedIds.Select(x => Modules2Lookup[x].ModuleInfoExtended).ToList()).ToList();
            if (loadOrderValidationIssues.Count != 0)
                return loadOrderValidationIssues.Select(x => x.Reason);

            // Not even sure a loop is needed
            // And I'm pretty sure that this is a dumb and non optimal solution.
            // ChangeModulePosition should move any nested dependencies higher?
            var hasInvalid = true;
            var retryCount = 0;
            var retryCountMax = currentOrderedIds.Count + 1;
            while (hasInvalid && retryCount < retryCountMax)
            {
                hasInvalid = false;
                retryCount++;
                for (var i = 0; i < currentOrderedIds.Count - 1; i++)
                {
                    var xId = currentOrderedIds[i];
                    var yId = currentOrderedIds[i + 1];

                    var xIdx = Modules2.IndexOf(z => z.ModuleInfoExtended.Id == xId);
                    var yIdx = Modules2.IndexOf(z => z.ModuleInfoExtended.Id == yId);
                    if (xIdx > yIdx)
                    {
                        if (!ChangeModulePosition(Modules2Lookup[xId], yIdx))
                        {
                            if (!ChangeModulePosition(Modules2Lookup[yId], xIdx))
                            {
                                hasInvalid = true;
                            }
                        }
                    }
                }
            }
            return retryCount >= retryCountMax ? new[] { "Failed to order the module list!" } : Array.Empty<string>();
        }

        public void SortByDefault()
        {
            static IEnumerable<ModuleInfoExtended> Sort(IEnumerable<ModuleInfoExtended> source)
            {
                var orderedModules = source
                    .OrderByDescending(x => x.IsOfficial)
                    .ThenByDescending(x => x.Id, new AlphanumComparatorFast())
                    .ToArray();

                return ModuleSorter.TopologySort(orderedModules, module => ModuleUtilities.GetDependencies(orderedModules, module));
            }

            var sorted = Sort(Modules2.Select(x => x.ModuleInfoExtended)).Select((x, i) => new { Item = x.Id, Index = i }).ToDictionary(x => x.Item, x => x.Index);
            Modules2.Sort(new ByIndexComparer<BUTRLauncherModuleVM>(x => sorted.TryGetValue(x.ModuleInfoExtended.Id, out var idx) ? idx : -1));
            //var sorted = Sort(Modules2.Select(x => x.ModuleInfoExtended)).Select(x => x.Id).ToList();
            //SortBy(sorted);
        }

        private IEnumerable<ModuleIssue> ValidateModule(BUTRLauncherModuleVM moduleVM)
        {
            var moduleInfoExtended = moduleVM.ModuleInfoExtended;
            var modules = FeatureIds.Features.Select(x => new ModuleInfoExtended { Id = x })
                .Concat(Modules2.Select(x => x.ModuleInfoExtended)).ToList();

            return ModuleUtilities.ValidateModule(modules, moduleInfoExtended, GetIsSelected, GetIsValid);
        }

        // Working
        private void ToggleModuleSelection(BUTRLauncherModuleVM moduleVM)
        {
            var moduleInfoExtended = moduleVM.ModuleInfoExtended;
            var modules = Modules2.Select(x => x.ModuleInfoExtended).ToList();

            if (moduleVM.IsSelected)
                ModuleUtilities.DisableModule(modules, moduleInfoExtended, GetIsSelected, SetIsSelected, GetIsDisabled, SetIsDisabled);
            else
                ModuleUtilities.EnableModule(modules, moduleInfoExtended, GetIsSelected, SetIsSelected, GetIsDisabled, SetIsDisabled);
        }

        // Working
        [BUTRDataSourceMethod(OverrideName = "ChangeLoadingOrderOf")]
        public void ChangeModulePosition(BUTRLauncherModuleVM targetModuleVM, int insertIndex, string _)
        {
            ChangeModulePosition(targetModuleVM, insertIndex, (issues) =>
            {
                HintManager.ShowHint(@$"Failed to place the module to the desired position! Placing to the nearest available! Reason:
{string.Join(Environment.NewLine, issues.Select(x => x.Reason))}");
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(5000);
                    HintManager.HideHint();
                }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
            });
        }

        public bool ChangeModulePosition(BUTRLauncherModuleVM targetModuleVM, int insertIndex, Action<IReadOnlyCollection<ModuleIssue>>? onIssues = null)
        {
            if (insertIndex >= Modules2.IndexOf(targetModuleVM)) insertIndex--;
            insertIndex = (int) MathF.Clamp(insertIndex, 0f, Modules2.Count - 1);
            var currentModuleIndex = Modules2.IndexOf(targetModuleVM);

            var modules = Modules2.Select(x => x.ModuleInfoExtended).ToList();
            var issuesReported = false;
            while (insertIndex != currentModuleIndex)
            {
                modules.RemoveAt(currentModuleIndex);
                modules.Insert(insertIndex, targetModuleVM.ModuleInfoExtended);
                var loadOrderValidationIssues = IsLoadOrderCorrect(modules.Where(x => Modules2Lookup[x.Id] is { IsValid: true }).ToList()).ToList();
                if (loadOrderValidationIssues.Count == 0)
                {
                    Modules2.RemoveAt(currentModuleIndex);
                    Modules2.Insert(insertIndex, targetModuleVM);
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

        // Working
        public static IEnumerable<ModuleIssue> IsLoadOrderCorrect(IReadOnlyList<ModuleInfoExtended> modules)
        {
            var loadOrder = FeatureIds.Features.Select(x => new ModuleInfoExtended { Id = x }).Concat(modules).ToList();
            foreach (var module in modules)
            {
                var issues = ModuleUtilities.ValidateLoadOrder(loadOrder, module).ToList();
                if (issues.Any())
                    return issues;
            }
            return Enumerable.Empty<ModuleIssue>();
        }

        private static bool IsVisible(bool isSignleplayer, ModuleInfoExtended moduleInfo) =>
            moduleInfo.IsNative() || !isSignleplayer && moduleInfo.IsMultiplayerModule || isSignleplayer && moduleInfo.IsSingleplayerModule;
    }
}