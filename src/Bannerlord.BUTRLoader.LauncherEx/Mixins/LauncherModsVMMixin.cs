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

namespace Bannerlord.BUTRLoader.Mixins
{
    internal enum ModuleType { Framework, Graphical, Standard, Patches }

    internal sealed partial class LauncherModsVMMixin : ViewModelMixin<LauncherModsVMMixin, LauncherModsVM>
    {
        private static readonly AccessTools.FieldRef<LauncherModsVM, UserDataManager>? _userDataManager =
            AccessTools2.FieldRefAccess<LauncherModsVM, UserDataManager>("_userDataManager");

        // All installed Modules
        private readonly Dictionary<string, ModuleInfoExtended> _extendedModuleInfoCache =
            // Not real modules, we declare this way our launcher capabilities
            new(FeatureIds.LauncherFeatures.ToDictionary(x => x, x => new ModuleInfoExtended { Id = x, IsSingleplayerModule = true }));

        private Func<BUTRLauncherSaveVM?>? _getSelectedSave;

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

        // Fast lookup for the ViewModels
        public Dictionary<string, BUTRLauncherModuleVM> Modules2Lookup { get; } = new();

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

        public string ModuleListCode => _getSelectedSave?.Invoke() is { ModuleListCode: { } } saveVM
                ? saveVM.ModuleListCode
                : $"_MODULES_*{string.Join("*", Modules2.Where(x => x.IsSelected).Select(x => x.ModuleInfoExtended.Id))}*_MODULES_";

        public LauncherModsVMMixin(LauncherModsVM launcherModsVM) : base(launcherModsVM)
        {
            _extendedModuleInfoCache.AddRange(ModuleInfoHelper.GetModules().Cast<ModuleInfoExtended>().ToDictionary(x => x.Id, x => x));
        }

        public ModuleInfoExtended? GetModuleById(string id) => _extendedModuleInfoCache.TryGetValue(id, out var mie) ? mie : null;
        public ModuleInfoExtended? GetModuleByName(string name) => _extendedModuleInfoCache.Values.FirstOrDefault(x => x.Name == name);
        public void SetGetSelectedSave(Func<BUTRLauncherSaveVM?> getSelectedSave)
        {
            _getSelectedSave = getSelectedSave;
        }

        public void Initialize(bool isMultiplayer)
        {
            if (_userDataManager is null || ViewModel is null) return;

            IsSingleplayer = !isMultiplayer;

            var userDataManager = _userDataManager(ViewModel);
            var userGameTypeData = isMultiplayer ? userDataManager.UserData.MultiplayerData : userDataManager.UserData.SingleplayerData;
            var modDatas = userGameTypeData.ModDatas.ToDictionary(x => x.Id, x => x.IsSelected);

            var issues = TryOrderByLoadOrder(modDatas.Keys, x => modDatas.TryGetValue(x, out var isSelected) && isSelected).ToList();
            if (issues.Count != 0)
            {
                IsForceSorted = true;
                ForceSortedHint = new LauncherHintVM($"The Load Order was re-sorted with the default algorithm!\nReasons:\n{string.Join("\n", issues)}");

                // Beta sorting algorithm will fail currently in some cases, use the TW fallback
                TryOrderByLoadOrderTW(Enumerable.Empty<string>(), x => modDatas.TryGetValue(x, out var isSelected) && isSelected);
                //TryOrderByLoadOrder(Enumerable.Empty<string>(), x => modDatas.TryGetValue(x, out var isSelected) && isSelected);
            }
            else
            {
                IsForceSorted = false;
            }
        }

        public IEnumerable<string> TryOrderByLoadOrder(IEnumerable<string> loadOrder, Func<string, bool> isModuleSelected) =>
            LauncherSettings.BetaSorting ? TryOrderByLoadOrderBeta(loadOrder, isModuleSelected) : TryOrderByLoadOrderTW(loadOrder, isModuleSelected);

        private void OverrideModuleVMs(IEnumerable<BUTRLauncherModuleVM> orderedModules)
        {
            Modules2Lookup.Clear();
            Modules2.Clear();
            foreach (var moduleVM in orderedModules)
            {
                Modules2.Add(moduleVM);
                Modules2Lookup[moduleVM.ModuleInfoExtended.Id] = moduleVM;
            }

            // Validate all VM's after they were selected and ordered
            foreach (var modules in Modules2)
                modules.Validate();
        }

        private IEnumerable<ModuleIssue> ValidateModule(BUTRLauncherModuleVM moduleVM) => ValidateModuleInternal(Modules2, Modules2Lookup, moduleVM);

        private void ToggleModuleSelection(BUTRLauncherModuleVM moduleVM) => ToggleModuleSelectionInternal(Modules2, Modules2Lookup, moduleVM);

        private bool ChangeModulePosition(BUTRLauncherModuleVM targetModuleVM, int insertIndex, Action<IReadOnlyCollection<ModuleIssue>>? onIssues = null) =>
            ChangeModulePositionInternal(Modules2, Modules2Lookup, targetModuleVM, insertIndex, onIssues);

        [BUTRDataSourceMethod]
        public void ExecuteRefresh() => SortByDefaultInternal(Modules2);

        [BUTRDataSourceMethod(OverrideName = "ChangeLoadingOrderOf")]
        public void ChangeModulePosition(BUTRLauncherModuleVM targetModuleVM, int insertIndex, string _)
        {
            ChangeModulePosition(targetModuleVM, insertIndex, (issues) =>
            {
                HintManager.ShowHint(@$"Failed to place the module to the desired position! Placing to the nearest available! Reason:\n{string.Join("\n", issues.Select(x => x.Reason))}");
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(5000);
                    HintManager.HideHint();
                }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
            });
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

        private static bool IsVisible(bool isSignleplayer, ModuleInfoExtended moduleInfo) =>
            moduleInfo.IsNative() || !isSignleplayer && moduleInfo.IsMultiplayerModule || isSignleplayer && moduleInfo.IsSingleplayerModule;
    }
}