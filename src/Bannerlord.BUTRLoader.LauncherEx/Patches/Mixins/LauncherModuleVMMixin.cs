using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Wrappers;

using HarmonyLib.BUTR.Extensions;

using TaleWorlds.Library;

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

        public object? DependencyHint2 { get; }
        public bool AnyDependencyAvailable2 { get; }

        public bool IsDangerous2 { get; }

        public bool IsDisabled2 { get; }


        private readonly ViewModel _launcherModuleVM;
        private readonly string _moduleId;

        public LauncherModuleVMMixin(ViewModel launcherModuleVM)
        {
            _launcherModuleVM = launcherModuleVM;

            var moduleInfoWrapper = LauncherModuleVMWrapper.Create(launcherModuleVM);

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

            if (ApplicationVersionHelper.GameVersion() is { Major: 1, Minor: >= 7 })
            {
                var id = moduleInfoWrapper.Info?.Id ?? string.Empty;
                if (ModuleInfoHelper.LoadFromId(id) is { } moduleInfo && ModuleInfoHelper2.GetDependencyHint(moduleInfo) is { } str && LauncherHintVMWrapper.Create(str) is { } hint)
                {
                    DependencyHint2 = hint.Object;
                    AnyDependencyAvailable2 = !string.IsNullOrEmpty(str);
                    SetVMProperty(nameof(DependencyHint2));
                    SetVMProperty(nameof(AnyDependencyAvailable2));
                }
            }

            _moduleId = moduleInfoWrapper.Info?.Id ?? string.Empty;

            IsDisabled2 = LauncherModuleVMPatch.AreAllDepenenciesPresentReferences.TryGetValue(launcherModuleVM, out var del)
                ? !(bool) del.DynamicInvoke(moduleInfoWrapper.Info?.Object)
                : true;

            UpdateIssues();

            // Remove danger warnings
            IsDangerous2 = false;
        }

        public void UpdateIssues()
        {
            IssuesText = LauncherModsVMPatch.Issues.TryGetValue(_moduleId, out var issues) && issues.Count > 0
                ? string.Join("\n", issues)
                : string.Empty;
        }
    }
}