using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Wrappers;
using Bannerlord.ModuleManager;

using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationVersion = Bannerlord.ModuleManager.ApplicationVersion;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class ModuleInfoHelper2
    {
        internal static readonly Dictionary<string, ModuleInfoExtended> ExtendedModuleInfoCache =
            // Not a real module, we declare this way our launcher capabilities
            new(FeatureIds.Features.ToDictionary(x => x, x => new ModuleInfoExtended { Id = x }));

        internal static readonly Dictionary<ModuleInfoExtended, bool> ValidModules = new();

        public static ModuleInfoExtended? GetExtendedModuleInfo(object moduleInfo) => GetExtendedModuleInfo(ModuleInfoWrapper.Create(moduleInfo));
        public static ModuleInfoExtended? GetExtendedModuleInfo(ModuleInfoWrapper moduleInfoWrapper)
        {
            if (ExtendedModuleInfoCache.ContainsKey(moduleInfoWrapper.Id))
                return ExtendedModuleInfoCache[moduleInfoWrapper.Id];

            var extendedModuleInfo = ModuleInfoHelper.LoadFromId(string.IsNullOrEmpty(moduleInfoWrapper.Alias) ? moduleInfoWrapper.Id : moduleInfoWrapper.Alias);
            if (extendedModuleInfo is null) return null; // Special case
            ExtendedModuleInfoCache[moduleInfoWrapper.Id] = extendedModuleInfo;
            var validModules = ValidModules.Where(x => x.Value).Select(x => x.Key).ToList();
            ValidModules[extendedModuleInfo] = !ModuleUtilities.ValidateModule(validModules, extendedModuleInfo, _ => false).Any();
            return extendedModuleInfo;
        }

        public static string GetDependencyHint(ModuleInfoExtended moduleInfoExtended)
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