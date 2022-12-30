using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Localization;
using Bannerlord.ModuleManager;

using System.Collections.Generic;
using System.Text;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class ModuleDependencyConstructor
    {
        public static string GetDependencyHint(ModuleInfoExtended moduleInfoExtended)
        {
            static string GetOptional(bool isOptional) => isOptional ? new BUTRTextObject("{=8ldMJPhQ} (optional)").ToString() : string.Empty;

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
                directDeps[dependentModule.Id] = $"{dependentModule.Id}{GetOptional(dependentModule.IsOptional)}{GetVersionV(dependentModule)}";
            }

            foreach (var dependentModule in moduleInfoExtended.IncompatibleModules)
            {
                incompatibleDeps[dependentModule.Id] = $"{dependentModule.Id}{GetVersionV(dependentModule)}";
            }

            foreach (var dependentModule in moduleInfoExtended.ModulesToLoadAfterThis)
            {
                loadAfterDeps[dependentModule.Id] = $"{dependentModule.Id}{GetVersionV(dependentModule)}";
            }

            foreach (var dependentModule in moduleInfoExtended.DependentModuleMetadatas)
            {
                if (dependentModule.IsIncompatible)
                {
                    incompatibleDeps[dependentModule.Id] = $"{dependentModule.Id}{GetVersion(dependentModule)}";
                }
                else if (dependentModule.LoadType == LoadType.LoadAfterThis)
                {
                    loadAfterDeps[dependentModule.Id] = $"{dependentModule.Id}{GetOptional(dependentModule.IsOptional)}{GetVersion(dependentModule)}";
                }
                else
                {
                    directDeps[dependentModule.Id] = $"{dependentModule.Id}{GetOptional(dependentModule.IsOptional)}{GetVersion(dependentModule)}";
                }
            }


            var sb = new StringBuilder();

            if (directDeps.Count > 0)
            {
                sb.Append(new BUTRTextObject("{=f9hYP7mk}Depends on:")).Append("\n");
            }
            foreach (var metadata in directDeps.Values.WithMetadata())
            {
                sb.Append(metadata.Value);
                if (!metadata.IsLast)
                    sb.Append("\n");
            }

            if (incompatibleDeps.Count > 0)
            {
                if (directDeps.Count > 0)
                {
                    sb.Append("\n----\n");
                }
                sb.Append(new BUTRTextObject("{=eGJ387f7}Incompatible with:")).Append("\n");
            }
            foreach (var metadata in incompatibleDeps.Values.WithMetadata())
            {
                sb.Append(metadata.Value);
                if (!metadata.IsLast)
                    sb.Append("\n");
            }

            if (loadAfterDeps.Count > 0)
            {
                if (directDeps.Count > 0 || incompatibleDeps.Count > 0)
                {
                    sb.Append("\n----\n");
                }
                sb.Append(new BUTRTextObject("{=eW76jyd7}Needs to load before:")).Append("\n");
            }
            foreach (var metadata in loadAfterDeps.Values.WithMetadata())
            {
                sb.Append(metadata.Value);
                if (!metadata.IsLast)
                    sb.Append("\n");
            }

            return sb.ToString();
        }
    }
}