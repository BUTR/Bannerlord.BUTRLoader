using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.ModuleManager;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.SaveSystem;

using ApplicationVersion = Bannerlord.ModuleManager.ApplicationVersion;

namespace Bannerlord.BUTRLoader.ViewModels
{
    internal sealed class BUTRLauncherSaveVM : BUTRViewModel
    {
        private record ModuleListEntry(string Name, ApplicationVersion Version);

        [BUTRDataSourceProperty]
        public string Name { get => _name; set => SetField(ref _name, value, nameof(Name)); }
        private string _name;

        [BUTRDataSourceProperty]
        public string Version { get => _version; set => SetField(ref _version, value, nameof(Version)); }
        private string _version;

        [BUTRDataSourceProperty]
        public string CharacterName { get => _characterName; set => SetField(ref _characterName, value, nameof(CharacterName)); }
        private string _characterName;

        [BUTRDataSourceProperty]
        public string Level { get => _level; set => SetField(ref _level, value, nameof(Level)); }
        private string _level;

        [BUTRDataSourceProperty]
        public string Days { get => _days; set => SetField(ref _days, value, nameof(Days)); }
        private string _days;

        [BUTRDataSourceProperty]
        public string CreatedAt { get => _createdAt; set => SetField(ref _createdAt, value, nameof(CreatedAt)); }
        private string _createdAt;

        [BUTRDataSourceProperty]
        public bool IsSelected { get => _isSelected; set => SetField(ref _isSelected, value, nameof(IsSelected)); }
        private bool _isSelected;

        [BUTRDataSourceProperty]
        public LauncherHintVM? LoadOrderHint { get => _loadOrderHint; set => SetField(ref _loadOrderHint, value, nameof(LoadOrderHint)); }
        private LauncherHintVM? _loadOrderHint;

        [BUTRDataSourceProperty]
        public bool HasWarning { get => _hasWarning; set => SetField(ref _hasWarning, value, nameof(HasWarning)); }
        private bool _hasWarning;

        [BUTRDataSourceProperty]
        public LauncherHintVM? WarningHint { get => _warningHint; set => SetField(ref _warningHint, value, nameof(WarningHint)); }
        private LauncherHintVM? _warningHint;

        [BUTRDataSourceProperty]
        public bool HasError { get => _hasError; set => SetField(ref _hasError, value, nameof(HasError)); }
        private bool _hasError;

        [BUTRDataSourceProperty]
        public LauncherHintVM? ErrorHint { get => _errorHint; set => SetField(ref _errorHint, value, nameof(ErrorHint)); }
        private LauncherHintVM? _errorHint;

        [BUTRDataSourceProperty]
        public bool IsVisible { get => _isVisible; set => SetField(ref _isVisible, value, nameof(IsVisible)); }
        private bool _isVisible = true;

        public string? ModuleListCode { get; private set; }

        private readonly SaveGameFileInfo _saveGameFileInfo;
        private readonly Action<BUTRLauncherSaveVM> _select;
        private readonly Func<string, ModuleInfoExtended?> _getModuleById;
        private readonly Func<string, ModuleInfoExtended?> _getModuleByName;

        public BUTRLauncherSaveVM(SaveGameFileInfo saveGameFileInfo, Action<BUTRLauncherSaveVM> select, Func<string, ModuleInfoExtended?> getModuleById, Func<string, ModuleInfoExtended?> getModuleByName)
        {
            _saveGameFileInfo = saveGameFileInfo;
            _select = select;
            _getModuleById = getModuleById;
            _getModuleByName = getModuleByName;

            _name = _saveGameFileInfo.Name;
            _version = _saveGameFileInfo.MetaData.TryGetValue("ApplicationVersion", out var appVersion) && !string.IsNullOrEmpty(appVersion) ? string.Join(".", appVersion.Split('.').Take(3)) : "Save Old";
            _characterName = _saveGameFileInfo.MetaData.TryGetValue("CharacterName", out var characterName) && !string.IsNullOrEmpty(characterName) ? characterName : "Save Old";
            _level = _saveGameFileInfo.MetaData.TryGetValue("MainHeroLevel", out var level) && !string.IsNullOrEmpty(level) ? level : "Save Old";
            _days = _saveGameFileInfo.MetaData.TryGetValue("DayLong", out var daysr) && !string.IsNullOrEmpty(daysr) && float.TryParse(daysr, out var days) ? days.ToString("0") : "Save Old";
            _createdAt = _saveGameFileInfo.MetaData.TryGetValue("CreationTime", out var ctr) && !string.IsNullOrEmpty(ctr) && long.TryParse(ctr, out var ticks) ? new DateTime(ticks).ToString("d") : "Save Old";

            ValidateSave();
        }

        private void ValidateSave()
        {
            var changeset = SaveHelper.GetChangeSet(_saveGameFileInfo.MetaData);
            var modules = SaveHelper.GetModules(_saveGameFileInfo.MetaData).Select(x =>
            {
                var version = SaveHelper.GetModuleVersion(_saveGameFileInfo.MetaData, x);
                if (version.ChangeSet == changeset)
                    version = new ApplicationVersion(version.ApplicationVersionType, version.Major, version.Minor, version.Revision, 0);
                return new ModuleListEntry(x, version);
            }).ToArray();

            var existingModules = modules.Select(x => _getModuleByName(x.Name)).OfType<ModuleInfoExtended>().ToArray();
            var existingModulesByName = existingModules.ToDictionary(x => x.Name, x => x);

            ModuleListCode = $"_MODULES_*{string.Join("*", existingModules.Select(x => x.Id))}*_MODULES_";

            var missingNames = modules.Select(x => x.Name).Except(existingModulesByName.Keys).ToArray();
            var loadOrderIssues = LoadOrderChecker.IsLoadOrderCorrect(existingModules).Select(x => x.Reason).ToList();

            //LoadOrderHint = new LauncherHintVM($"Load Order:\n{string.Join("\n", existingModules.Select(x => x.Id))}\n\nUnknown Mod Names:{string.Join("\n", missingNames)}");
            LoadOrderHint = new LauncherHintVM($"Load Order:\n{string.Join("\n", modules.Select(x => _getModuleByName(x.Name)?.Id ?? $"{x.Name} (Unknown Id)"))}");

            if (missingNames.Length > 0 || loadOrderIssues.Count > 0)
            {
                var text = string.Empty;
                text += loadOrderIssues.Count > 0 ? $"Load Order Issues:\n{string.Join("\n\n", loadOrderIssues)}{(missingNames.Length > 0 ? "\n\n\n" : string.Empty)}" : string.Empty;
                text += missingNames.Length > 0 ? $"Missing modules:\n{string.Join("\n", missingNames)}" : string.Empty;

                HasError = true;
                ErrorHint = new LauncherHintVM(text);
                return;
            }

            var issues = new List<string>();
            foreach (var module in modules)
            {
                var existingModule = existingModulesByName[module.Name];
                if (module.Version != existingModule.Version)
                {
                    issues.Add($"{existingModule.Id}. Required {module.Version}. Actual {existingModule.Version}");
                }
            }
            if (issues.Count > 0)
            {
                HasWarning = true;
                WarningHint = new LauncherHintVM($"Mismatched module versions:\n{string.Join("\n\n", issues)}");
            }
        }

        [BUTRDataSourceMethod]
        public void ExecuteSelect()
        {
            _select(this);
        }

        [BUTRDataSourceMethod]
        public void ExecuteOpen()
        {
            var saveFilePath = SaveHelper.GetSaveFilePath(_saveGameFileInfo);
            if (string.IsNullOrEmpty(saveFilePath) || !File.Exists(saveFilePath)) return;

            Process.Start("explorer.exe", $"/select,\"{saveFilePath}\"");
        }
    }
}