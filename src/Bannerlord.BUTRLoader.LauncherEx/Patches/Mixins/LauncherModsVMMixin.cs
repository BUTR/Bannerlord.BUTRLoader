using Bannerlord.BUTR.Shared.Extensions;
using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Patches.ViewModels;
using Bannerlord.ModuleManager;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherModsVMMixin : ViewModelMixin<LauncherModsVMMixin, LauncherModsVM>
    {
        // All installed Modules
        private readonly Dictionary<string, ModuleInfoExtended> _extendedModuleInfoCache =
            // Not real modules, we declare this way our launcher capabilities
            new(FeatureIds.Features.ToDictionary(x => x, x => new ModuleInfoExtended { Id = x, IsSingleplayerModule = true }));

        // All available for loading modules
        private readonly Dictionary<string, ModuleInfoExtended> _availableModules = new();
        // Fast lookup for the ViewModels
        private readonly Dictionary<string, BUTRLauncherModuleVM> _modulesLookup = new();

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
                    if (!moduleVM.IsValid && !moduleVM.IsSelected)
                        ToggleModuleSelection(moduleVM);
                }
                else if (!moduleVM.ModuleInfoExtended.IsNative())
                {
                    if (moduleVM.IsSelected)
                        ToggleModuleSelection(moduleVM);
                }
            }
        }

        [BUTRDataSourceMethod]
        public void ExecuteRefresh()
        {
            SortBySorter(Modules2, _availableModules.Values.ToList());
        }


        private bool GetIsValid(ModuleInfoExtended module)
        {
            if (FeatureIds.Features.Contains(module.Id))
                return true;

            return _modulesLookup[module.Id].IsValid;
        }
        private bool GetIsSelected(ModuleInfoExtended module)
        {
            if (FeatureIds.Features.Contains(module.Id))
                return false;

            return _modulesLookup[module.Id].IsSelected;
        }
        private void SetIsSelected(ModuleInfoExtended module, bool value)
        {
            if (FeatureIds.Features.Contains(module.Id))
                return;
            _modulesLookup[module.Id].IsSelected = value;
        }
        private bool GetIsDisabled(ModuleInfoExtended module)
        {
            if (FeatureIds.Features.Contains(module.Id))
                return false;

            return _modulesLookup[module.Id].IsDisabled;
        }
        private void SetIsDisabled(ModuleInfoExtended module, bool value)
        {
            if (FeatureIds.Features.Contains(module.Id))
                return;
            _modulesLookup[module.Id].IsDisabled = value;
        }

        public void Initialize(bool isMultiplayer, UserData userData)
        {
            // Set IsSingleplayer2 for further use
            IsSingleplayer = !isMultiplayer;

            // Clear previous data
            _availableModules.Clear();
            _modulesLookup.Clear();
            Modules2.Clear();

            // Get available modules based on Singleplayer/Multiplayer
            _availableModules.AddRange(_extendedModuleInfoCache.Values
                .Where(x => IsVisible(IsSingleplayer, x))
                .ToDictionary(x => x.Id, x => x));

            // Initialize ViewModels
            foreach (var moduleInfoExtended in _availableModules.Values.OfType<ModuleInfoExtendedWithMetadata>())
            {
                var viewModel = new BUTRLauncherModuleVM(moduleInfoExtended, ToggleModuleSelection, ValidateModule);
                _modulesLookup[moduleInfoExtended.Id] = viewModel;
                Modules2.Add(viewModel);
            }

            var userGameTypeData = isMultiplayer ? userData.MultiplayerData : userData.SingleplayerData;

            // Apply saved IsSelected
            foreach (var userModData in userGameTypeData.ModDatas.Where(x => x.IsSelected))
            {
                if (!_availableModules.TryGetValue(userModData.Id, out var moduleInfo)) continue;

                var moduleVM = _modulesLookup[moduleInfo.Id];
                if (moduleVM.IsValid)
                    ToggleModuleSelection(moduleVM);
            }

            // Apply saved ordering
            SortByUserGameData(Modules2, userGameTypeData);

            // Force reorder if the current order is not valid
            if (!IsLoadOrderCorrect(Modules2.Where(x => x.IsSelected && x.IsValid).Select(x => x.ModuleInfoExtended).ToList()))
                SortBySorter(Modules2, _availableModules.Values.ToList());

            // Validate all VM's after they were selected and ordered
            foreach (var modules in Modules2)
                modules.Validate();
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
            var modules = _availableModules.Values;

            if (moduleVM.IsSelected)
                ModuleUtilities.DisableModule(modules, moduleInfoExtended, GetIsSelected, SetIsSelected, GetIsDisabled, SetIsDisabled);
            else
                ModuleUtilities.EnableModule(modules, moduleInfoExtended, GetIsSelected, SetIsSelected, GetIsDisabled, SetIsDisabled);
        }

        // Working
        [BUTRDataSourceMethod(OverrideName = "ChangeLoadingOrderOf")]
        public void ChangeModulePosition(BUTRLauncherModuleVM targetModuleVM, int insertIndex, string _)
        {
            if (insertIndex >= Modules2.IndexOf(targetModuleVM)) insertIndex--;
            insertIndex = (int) MathF.Clamp(insertIndex, 0f, Modules2.Count - 1);
            var currentModuleIndex = Modules2.IndexOf(targetModuleVM);

            var modules = Modules2.Select(x => x.ModuleInfoExtended).ToList();
            while (insertIndex != currentModuleIndex)
            {
                modules.RemoveAt(currentModuleIndex);
                modules.Insert(insertIndex, targetModuleVM.ModuleInfoExtended);
                if (IsLoadOrderCorrect(modules.Where(x => _modulesLookup[x.Id] is { IsSelected: true, IsValid: true }).ToList()))
                {
                    Modules2.RemoveAt(currentModuleIndex);
                    Modules2.Insert(insertIndex, targetModuleVM);
                    break;
                }

                // Do it until we find the nearest acceptable index or stop if we failes
                modules.RemoveAt(insertIndex);
                modules.Insert(currentModuleIndex, targetModuleVM.ModuleInfoExtended);

                if (currentModuleIndex < insertIndex) insertIndex--;
                if (currentModuleIndex > insertIndex) insertIndex++;
            }
        }

        // Working
        private bool IsLoadOrderCorrect(IReadOnlyList<ModuleInfoExtended> modules)
        {
            var loadOrder = FeatureIds.Features.Select(x => new ModuleInfoExtended { Id = x }).Concat(modules).ToList();
            foreach (var module in modules)
            {
                if (ModuleUtilities.ValidateLoadOrder(loadOrder, module).Any())
                    return false;
            }
            return true;
        }

        private static bool IsVisible(bool isSignleplayer, ModuleInfoExtended moduleInfo) =>
            moduleInfo.IsNative() || (!isSignleplayer && moduleInfo.IsMultiplayerModule) || (isSignleplayer && moduleInfo.IsSingleplayerModule);

        private static void SortBySorter(MBBindingList<BUTRLauncherModuleVM> moduleVMs, IReadOnlyCollection<ModuleInfoExtended> modules)
        {
            var sorted = ModuleSorter.Sort(modules).Select((x, i) => new { Item = x.Id, Index = i }).ToDictionary(x => x.Item, x => x.Index);
            moduleVMs.Sort(new ByIndexComparer<BUTRLauncherModuleVM>(x => sorted.TryGetValue(x.ModuleInfoExtended.Id, out var idx) ? idx : -1));
        }

        private static void SortByUserGameData(MBBindingList<BUTRLauncherModuleVM> moduleVMs, UserGameTypeData userData)
        {
            var sorted = userData.ModDatas.Select((x, i) => new { Item = x.Id, Index = i }).ToDictionary(x => x.Item, x => x.Index);
            moduleVMs.Sort(new ByIndexComparer<BUTRLauncherModuleVM>(x => sorted.TryGetValue(x.ModuleInfoExtended.Id, out var idx) ? idx : -1));
        }
    }
}