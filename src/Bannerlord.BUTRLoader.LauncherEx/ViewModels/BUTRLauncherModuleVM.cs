using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.ModuleManager;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TaleWorlds.MountAndBlade.Launcher.Library;

using ApplicationVersion = Bannerlord.ModuleManager.ApplicationVersion;

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
                    OnPropertyChanged(nameof(IsNotValid));
                }
            }
        }
        private string _issuesText = string.Empty;


        [BUTRDataSourceProperty]
        public bool IsNotSelectable => !IsValid || IsDisabled;

        [BUTRDataSourceProperty]
        public bool IsValid => string.IsNullOrWhiteSpace(IssuesText);

        [BUTRDataSourceProperty]
        public bool IsNotValid => !IsValid;

        //[BUTRDataSourceProperty]
        //public bool IsNoUpdateAvailable { get => _isNoUpdateAvailable; set => SetField(ref _isNoUpdateAvailable, value, nameof(IsNoUpdateAvailable)); }
        //private bool _isNoUpdateAvailable;


        public BUTRLauncherModuleVM(
            ModuleInfoExtendedWithMetadata moduleInfoExtended,
            Action<BUTRLauncherModuleVM> select,
            Func<BUTRLauncherModuleVM, IEnumerable<ModuleIssue>> validate)
        {
            ModuleInfoExtended = moduleInfoExtended;
            _select = select;
            _validate = validate;

            if (GetDependencyHint(moduleInfoExtended) is { } str)
            {
                DependencyHint = new LauncherHintVM(str);
                AnyDependencyAvailable = !string.IsNullOrEmpty(str);
            }

            var dangerous = string.Empty;
            if (ModuleChecker.IsInstalledInMainAndExternalModuleDirectory(moduleInfoExtended))
                dangerous += "The Module is installed in the game's /Modules folder and on Steam Workshop!\nThe /Modules version will be used!\n";
            if (ModuleChecker.IsObfuscated(moduleInfoExtended))
                dangerous += "The DLL is obfuscated!\nThere is no guarantee that the code is safe!\nThe BUTR Team warns of consequences arising from running obfuscated code!\n";
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

        public override string ToString() => $"{ModuleInfoExtended}, IsSelected: {IsSelected}, IsValid: {IsValid}";

        private static string GetDependencyHint(ModuleInfoExtended moduleInfoExtended)
        {
            static string GetOptional(bool isOptional) => isOptional ? " (optional)" : string.Empty;

            static string GetVersionV(DependentModule metadata)
            {
                if (metadata.Version != ApplicationVersion.Empty)
                {
                    return $" >= {metadata.Version}";
                }
                return string.Empty;
            }
            static string GetVersion(DependentModuleMetadata metadata)
            {
                if (metadata.Version != ApplicationVersion.Empty)
                {
                    return $" >= {metadata.Version}";
                }
                if (metadata.VersionRange != ApplicationVersionRange.Empty)
                {
                    return $" >= {metadata.VersionRange.Min} <= {metadata.VersionRange.Max}";
                }
                return string.Empty;
            }

            var directDeps = new Dictionary<string, string>();
            var incompatibleDeps = new Dictionary<string, string>();
            var loadAfterDeps = new Dictionary<string, string>();

            foreach (var dependentModule in moduleInfoExtended.DependentModules)
            {
                directDeps[dependentModule.Id] = $"{dependentModule.Id}{GetOptional(dependentModule.IsOptional)}{GetVersionV(dependentModule)}\n";
            }

            foreach (var dependentModule in moduleInfoExtended.IncompatibleModules)
            {
                incompatibleDeps[dependentModule.Id] = $"{dependentModule.Id}{GetVersionV(dependentModule)}\n";
            }

            foreach (var dependentModule in moduleInfoExtended.ModulesToLoadAfterThis)
            {
                loadAfterDeps[dependentModule.Id] = $"{dependentModule.Id}{GetVersionV(dependentModule)}\n";
            }

            foreach (var dependentModule in moduleInfoExtended.DependentModuleMetadatas)
            {
                if (dependentModule.IsIncompatible)
                {
                    incompatibleDeps[dependentModule.Id] = $"{dependentModule.Id}{GetVersion(dependentModule)}\n";
                }
                else if (dependentModule.LoadType == LoadType.LoadAfterThis)
                {
                    loadAfterDeps[dependentModule.Id] = $"{dependentModule.Id}{GetOptional(dependentModule.IsOptional)}{GetVersion(dependentModule)}\n";
                }
                else
                {
                    directDeps[dependentModule.Id] = $"{dependentModule.Id}{GetOptional(dependentModule.IsOptional)}{GetVersion(dependentModule)}\n";
                }
            }


            var sb = new StringBuilder();

            if (directDeps.Count > 0)
            {
                sb.Append("Depends on: \n");
            }
            foreach (var str in directDeps.Values)
            {
                sb.Append(str);
            }

            if (incompatibleDeps.Count > 0)
            {
                if (directDeps.Count > 0)
                {
                    sb.Append("\n----\n");
                }
                sb.Append("Incompatible with: \n");
            }
            foreach (var str in incompatibleDeps.Values)
            {
                sb.Append(str);
            }

            if (loadAfterDeps.Count > 0)
            {
                if (directDeps.Count > 0 || incompatibleDeps.Count > 0)
                {
                    sb.Append("\n----\n");
                }
                sb.Append("Needs to load before: \n");
            }
            foreach (var str in loadAfterDeps.Values)
            {
                sb.Append(str);
            }

            return sb.ToString();
        }
    }
}