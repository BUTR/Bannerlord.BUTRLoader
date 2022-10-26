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

        public LauncherHintVM DependencyHint2 { get; }
        public bool AnyDependencyAvailable2 { get; }

        public bool IsDangerous2 { get; }

        public bool IsDisabled2 { get; }


        private readonly LauncherModuleVM _launcherModuleVM;
        private readonly string _moduleId;

        public LauncherModuleVMMixin(LauncherModuleVM launcherModuleVM)
        {
            _launcherModuleVM = launcherModuleVM;

            void SetVMProperty(string property)
            {
                var propertyInfo = new WrappedPropertyInfo(AccessTools2.Property($"Bannerlord.BUTRLoader.Patches.Mixins.LauncherModuleVMMixin:{property}")!, this);
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

            var id = _launcherModuleVM.Info.Id ?? string.Empty;
            if (ModuleInfoHelper.LoadFromId(id) is { } moduleInfo && ModuleInfoHelper2.GetDependencyHint(moduleInfo) is { } str)
            {
                DependencyHint2 = new LauncherHintVM(str);
                AnyDependencyAvailable2 = !string.IsNullOrEmpty(str);
                SetVMProperty(nameof(DependencyHint2));
                SetVMProperty(nameof(AnyDependencyAvailable2));
            }

            _moduleId = _launcherModuleVM.Info.Id ?? string.Empty;

            IsDisabled2 = LauncherModuleVMPatch.AreAllDepenenciesPresentReferences.TryGetValue(launcherModuleVM, out var del)
                ? !(bool) del.DynamicInvoke(_launcherModuleVM.Info)
                : true;

            UpdateIssues();

            // Remove danger warnings
            IsDangerous2 = false;

            _launcherModuleVM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == "Refresh_Command")
                    UpdateIssues();
            };
        }

        public void UpdateIssues()
        {
            IssuesText = IssueStorage.Issues.TryGetValue(_moduleId, out var issues) && issues.Count > 0
                ? string.Join("\n", issues)
                : string.Empty;
        }
    }
}