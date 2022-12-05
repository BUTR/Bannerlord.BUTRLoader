using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib.BUTR.Extensions;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal sealed class LauncherModuleVMMixin
    {
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    _launcherModuleVM.OnPropertyChangedWithValue(value, nameof(IsExpanded));
                }
            }
        }
        private bool _isExpanded;

        public string IssuesText
        {
            get => _issuesText;
            set
            {
                if (value != _issuesText)
                {
                    _issuesText = value;
                    _launcherModuleVM.OnPropertyChangedWithValue(value, nameof(IssuesText));
                    _launcherModuleVM.OnPropertyChanged(nameof(HasNoIssues));
                }
            }
        }
        private string _issuesText = string.Empty;

        public bool HasIssues => !string.IsNullOrWhiteSpace(IssuesText);
        public bool HasNoIssues => !HasIssues;

        public bool IsNoUpdateAvailable
        {
            get => _isNoUpdateAvailable;
            set
            {
                if (value != _isNoUpdateAvailable)
                {
                    _isNoUpdateAvailable = value;
                    _launcherModuleVM.OnPropertyChangedWithValue(value, nameof(IsNoUpdateAvailable));
                }
            }
        }
        private bool _isNoUpdateAvailable;

        public LauncherHintVM? DependencyHint2 { get; }
        public bool AnyDependencyAvailable2 { get; }

        public bool IsDangerous2 { get; }


        public bool IsDisabled2 { get; }


        private readonly LauncherModuleVM _launcherModuleVM;

        public LauncherModuleVMMixin(LauncherModuleVM launcherModuleVM)
        {
            _launcherModuleVM = launcherModuleVM;

            void SetVMProperty(string property)
            {
                var propertyInfo = new WrappedPropertyInfo(AccessTools2.Property(typeof(LauncherModuleVMMixin), property)!, this);
                _launcherModuleVM.AddProperty(property, propertyInfo);
                propertyInfo.PropertyChanged += (_, e) => _launcherModuleVM.OnPropertyChanged(e.PropertyName);
            }

            SetVMProperty(nameof(IsExpanded));
            SetVMProperty(nameof(IssuesText));
            SetVMProperty(nameof(HasIssues));
            SetVMProperty(nameof(HasNoIssues));
            SetVMProperty(nameof(IsNoUpdateAvailable));
            SetVMProperty(nameof(IsDisabled2));
            SetVMProperty(nameof(IsDangerous2));

            if (ModuleInfoHelper.LoadFromId(_launcherModuleVM.Info.Id) is { } moduleInfoExtended)
            {
                if (ModuleInfoHelper2.GetDependencyHint(moduleInfoExtended) is { } str)
                {
                    DependencyHint2 = new LauncherHintVM(str);
                    AnyDependencyAvailable2 = !string.IsNullOrEmpty(str);
                    SetVMProperty(nameof(DependencyHint2));
                    SetVMProperty(nameof(AnyDependencyAvailable2));
                }

                var dangerous = string.Empty;
                if (ModuleChecker.IsInstalledInMainAndExternalModuleDirectory(moduleInfoExtended))
                    dangerous += "The Module is installed in the game's /Modules folder and on Steam Workshop!\nThe /Modules version will be used!\n";
                if (ModuleChecker.IsObfuscated(moduleInfoExtended))
                    dangerous += "The DLL is obfuscated!\nThere is no guarantee that the code is safe!\nThe BUTR Team warns of consequences arising from running obfuscated code!\n";
                if (!string.IsNullOrEmpty(dangerous))
                {
                    IsDangerous2 = true;
                    _launcherModuleVM.DangerousHint = new LauncherHintVM(dangerous);
                }
            }

            IsDisabled2 = LauncherModuleVMPatch.AreAllDepenenciesPresentReferences.TryGetValue(launcherModuleVM, out var del)
                ? !(bool) del.DynamicInvoke(_launcherModuleVM.Info)
                : true;

            UpdateIssues(_launcherModuleVM.Info.Id);

            _launcherModuleVM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == "Refresh_Command")
                    UpdateIssues(_launcherModuleVM.Info.Id);
            };
        }

        private void UpdateIssues(string id)
        {
            IssuesText = IssueStorage.Issues.TryGetValue(id, out var issues) && issues.Count > 0
                ? string.Join("\n", issues)
                : string.Empty;
        }
    }
}