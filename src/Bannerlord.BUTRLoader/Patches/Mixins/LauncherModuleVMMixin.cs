using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

using static Bannerlord.BUTRLoader.Helpers.ModuleInfoHelper;

namespace Bannerlord.BUTRLoader.Patches.Mixins
{
    internal class LauncherModuleVMMixin
    {
        public string Expander => IsExpanded ? "[-]" : "[X]";

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    _launcherModuleVM.OnPropertyChangedWithValue(value, nameof(IsExpanded));
                    _launcherModuleVM.OnPropertyChanged(nameof(Expander));
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
        private string _issuesText;

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

        private readonly LauncherModuleVM _launcherModuleVM;
        private readonly string _moduleId;

        public LauncherModuleVMMixin(LauncherModuleVM launcherModuleVM)
        {
            _launcherModuleVM = launcherModuleVM;

            var propsObject = AccessTools.Field(typeof(ViewModel), "_propertyInfos")?.GetValue(_launcherModuleVM) as Dictionary<string, PropertyInfo>
                              ?? new Dictionary<string, PropertyInfo>();

            void SetVMProperty(string property)
            {
                propsObject[property] = new WrappedPropertyInfo(
                    AccessTools.Property(typeof(LauncherModuleVMMixin), property),
                    this,
                    () => _launcherModuleVM.OnPropertyChanged(property));
            }

            SetVMProperty(nameof(Expander));
            SetVMProperty(nameof(IsExpanded));
            SetVMProperty(nameof(IssuesText));
            SetVMProperty(nameof(HasIssues));
            SetVMProperty(nameof(HasNoIssues));
            SetVMProperty(nameof(IsNoUpdateAvailable));

            _moduleId = ((string) GetId.Invoke(GetInfo.GetValue(launcherModuleVM), Array.Empty<object>()));
            UpdateIssues();
        }

        public void ExecuteExpander()
        {
            IsExpanded = !IsExpanded;
        }

        public void UpdateIssues()
        {
            IssuesText = LauncherModsVMPatch.Issues.TryGetValue(_moduleId, out var issues) && issues.Count > 0
                ? string.Join("\n", issues)
                : string.Empty;
        }
    }
}