using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.ModuleManager;

using HarmonyLib.BUTR.Extensions;

using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
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

            var id = _launcherModuleVM.Info.Id ?? string.Empty;
            var moduleInfoExtended = ModuleInfoHelper.LoadFromId(id);
            if (moduleInfoExtended is not null && ModuleInfoHelper2.GetDependencyHint(moduleInfoExtended) is { } str)
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

            if (CheckModuleDangerous(moduleInfoExtended))
            {
                IsDangerous2 = true;
                _launcherModuleVM.DangerousHint = new LauncherHintVM(
                    "The DLL is obfuscated!\nThere is no guarantee that the code is safe!\nThe BUTR Team warns of consequences arising from running obfuscated code!");
            }
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

        private static bool CheckModuleDangerous(ModuleInfoExtended? moduleInfoExtended)
        {
            if (moduleInfoExtended is not ModuleInfoExtendedWithMetadata moduleInfoExtendedWithMetadata)
                return false;

            foreach (var subModule in moduleInfoExtended.SubModules.Where(x =>
                         ModuleInfoHelper.CheckIfSubModuleCanBeLoaded(x, ApplicationPlatform.CurrentPlatform, ApplicationPlatform.CurrentRuntimeLibrary, DedicatedServerType.None, false)))
            {
                var asm = Path.GetFullPath(Path.Combine(moduleInfoExtendedWithMetadata.Path, "bin", "Win64_Shipping_Client", subModule.DLLName));
                try
                {
                    using var stream = File.OpenRead(asm);
                    using var reader = new PEReader(stream);
                    var metadata = reader.GetMetadataReader();
                    var assembly = metadata.GetAssemblyDefinition();
                    var module = metadata.GetModuleDefinition();
                    var hasConfusedByAttributeUsed = module.GetCustomAttributes().Select(metadata.GetCustomAttribute).Any(x =>
                    {
                        if (x.Constructor.Kind == HandleKind.MemberReference)
                        {
                            var ctor = metadata.GetMemberReference((MemberReferenceHandle)x.Constructor);
                            var attrType = metadata.GetTypeReference((TypeReferenceHandle) ctor.Parent);
                            var name = metadata.GetString(attrType.Name);
                            return name == "ConfusedByAttribute";
                        }

                        return false;
                    });
                    var hasConfusedByAttributeDeclared = metadata.TypeDefinitions.Select(metadata.GetTypeDefinition).Any(x =>
                    {
                        var name = metadata.GetString(x.Name);
                        return name == "ConfusedByAttribute";
                    });

                    return hasConfusedByAttributeUsed || hasConfusedByAttributeDeclared;
                }
                catch (Exception)
                {
                    return true;
                }
            }

            return false;
        }
    }
}