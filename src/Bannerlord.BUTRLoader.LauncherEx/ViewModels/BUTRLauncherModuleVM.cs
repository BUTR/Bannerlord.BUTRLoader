using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.ModuleManager;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.ViewModels
{
    internal sealed class BUTRLauncherModuleVM : BUTRViewModel
    {
        public readonly ModuleInfoExtendedWithMetadata ModuleInfoExtended;
        private readonly Action<BUTRLauncherModuleVM> _select;
        private readonly Func<BUTRLauncherModuleVM, IEnumerable<ModuleIssue>> _validate;

        [BUTRDataSourceProperty]
        public string Name => ModuleInfoExtended.Name;

        [BUTRDataSourceProperty]
        public string VersionText => ModuleInfoExtended.Version.ToString();

        [BUTRDataSourceProperty]
        public bool IsOfficial => ModuleInfoExtended.IsOfficial;

        [BUTRDataSourceProperty]
        public bool IsDangerous { get => _isDangerous; set => SetField(ref _isDangerous, value, nameof(IsDangerous)); }
        private bool _isDangerous;

        [BUTRDataSourceProperty]
        public LauncherHintVM? DangerousHint { get => _dangerousHint; set => SetField(ref _dangerousHint, value, nameof(DangerousHint)); }
        private LauncherHintVM? _dangerousHint;

        [BUTRDataSourceProperty]
        public LauncherHintVM? DependencyHint { get => _dependencyHint; set => SetField(ref _dependencyHint, value, nameof(DependencyHint)); }
        private LauncherHintVM? _dependencyHint;

        [BUTRDataSourceProperty]
        public bool AnyDependencyAvailable { get => _anyDependencyAvailable; set => SetField(ref _anyDependencyAvailable, value, nameof(AnyDependencyAvailable)); }
        private bool _anyDependencyAvailable;

        [BUTRDataSourceProperty]
        public bool IsSelected { get => _isSelected; set => SetField(ref _isSelected, value, nameof(IsSelected)); }
        private bool _isSelected;

        [BUTRDataSourceProperty]
        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                if (value != _isDisabled)
                {
                    _isDisabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNotSelectable));
                }
            }
        }
        private bool _isDisabled;

        [BUTRDataSourceProperty]
        public bool IsExpanded { get => _isExpanded; set => SetField(ref _isExpanded, value, nameof(IsExpanded)); }
        private bool _isExpanded;

        [BUTRDataSourceProperty]
        public string IssuesText
        {
            get => _issuesText;
            set
            {
                if (value != _issuesText)
                {
                    _issuesText = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNotSelectable));
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }
        private string _issuesText = string.Empty;


        [BUTRDataSourceProperty]
        public bool IsNotSelectable => !IsValid || IsDisabled;

        [BUTRDataSourceProperty]
        public bool IsValid => string.IsNullOrWhiteSpace(IssuesText);

        [BUTRDataSourceProperty]
        public bool IsVisible { get => _isVisible; set => SetField(ref _isVisible, value, nameof(IsVisible)); }
        private bool _isVisible = true;

        public BUTRLauncherModuleVM(ModuleInfoExtendedWithMetadata moduleInfoExtended, Action<BUTRLauncherModuleVM> select, Func<BUTRLauncherModuleVM, IEnumerable<ModuleIssue>> validate)
        {
            ModuleInfoExtended = moduleInfoExtended;
            _select = select;
            _validate = validate;

            if (ModuleDependencyConstructor.GetDependencyHint(moduleInfoExtended) is { } str)
            {
                DependencyHint = new LauncherHintVM(str);
                AnyDependencyAvailable = !string.IsNullOrEmpty(str);
            }

            var dangerous = string.Empty;
            if (ModuleChecker.IsInstalledInMainAndExternalModuleDirectory(moduleInfoExtended))
                dangerous += "The Module is installed in the game's /Modules folder and on Steam Workshop!\nThe /Modules version will be used!";
            if (ModuleChecker.IsObfuscated(moduleInfoExtended))
            {
                if (dangerous.Length != 0)
                    dangerous += "\n";
                dangerous += "The DLL is obfuscated!\nThere is no guarantee that the code is safe!\nThe BUTR Team warns of consequences arising from running obfuscated code!";
            }
            if (!string.IsNullOrEmpty(dangerous))
            {
                IsDangerous = true;
                DangerousHint = new LauncherHintVM(dangerous);
            }
            else
            {
                IsDangerous = false;
                DangerousHint = new LauncherHintVM(dangerous);
            }
        }

        public void Validate()
        {
            var validationIssues = _validate(this).ToList();

            IssuesText = validationIssues.Count > 0
                ? string.Join("\n", validationIssues.Select(x => x.Reason))
                : string.Empty;
        }

        [BUTRDataSourceMethod]
        public void ExecuteSelect()
        {
            if (IsNotSelectable)
                return;

            _select(this);
        }

        [BUTRDataSourceMethod]
        public void ExecuteOpen()
        {
            if (Integrations.IsModOrganizer2)
            {
                var explorer = Path.Combine(Integrations.ModOrganizer2Path!, "explorer++", "Explorer++.exe");
                if (!File.Exists(explorer)) return;
                Process.Start(explorer, $"\"{ModuleInfoExtended.Path}\"");
                return;
            }

            if (!Directory.Exists(ModuleInfoExtended.Path)) return;
            Process.Start(ModuleInfoExtended.Path);
        }

        public override string ToString() => $"{ModuleInfoExtended}, IsSelected: {IsSelected}, IsValid: {IsValid}";
    }
}