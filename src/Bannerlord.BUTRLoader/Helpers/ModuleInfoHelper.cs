using Bannerlord.ModuleManager;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Text;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class ModuleInfoHelper2
    {
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
                directDeps[dependentModule.Id]= $"{dependentModule.Id}{GetOptional(dependentModule.IsOptional)}{GetVersionV(dependentModule)}\n";
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

    internal sealed class ModuleInfoWrapper
    {
        private delegate string GetIdDelegate(object instance);
        private delegate string GetAliasDelegate(object instance);
        private delegate bool GetIsSelectedDelegate(object instance);

        private static readonly Type? OldModuleInfoType = AccessTools2.TypeByName("TaleWorlds.Library.ModuleInfo");
        private static readonly Type? NewModuleInfoType = AccessTools2.TypeByName("TaleWorlds.ModuleManager.ModuleInfo");
        public static readonly Type? ModuleInfoType = OldModuleInfoType ?? NewModuleInfoType;

        private static readonly GetIdDelegate? GetId = AccessTools2.GetPropertyGetterDelegate<GetIdDelegate>(ModuleInfoType, "Id");
        private static readonly GetAliasDelegate? GetAlias = AccessTools2.GetPropertyGetterDelegate<GetAliasDelegate>(ModuleInfoType, "Alias");
        private static readonly GetIsSelectedDelegate? GetIsSelected = AccessTools2.GetPropertyGetterDelegate<GetIsSelectedDelegate>(ModuleInfoType, "IsSelected");

        public static ModuleInfoWrapper Create(object? @object) => new(@object);

        public string Id => _id ??= Object is null ? string.Empty : GetId?.Invoke(Object) ?? string.Empty;
        private string? _id;
        public string Alias => _alias ??= Object is null ? string.Empty : GetAlias?.Invoke(Object) ?? string.Empty;
        private string? _alias;
        public bool IsSelected => Object is null ? false : GetIsSelected?.Invoke(Object) ?? false;

        public object? Object { get; }

        private ModuleInfoWrapper(object? @object)
        {
            Object = @object;
        }
    }

    internal sealed class LauncherModuleVMWrapper
    {
        private static readonly Type? LauncherModuleVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.LauncherModuleVM");
        private static readonly AccessTools.FieldRef<object, object>? GetInfo = AccessTools2.FieldRefAccess<object>(LauncherModuleVMType!, "Info");

        public static LauncherModuleVMWrapper Create(object @object) => new(@object);

        public ModuleInfoWrapper Info => _info ??= ModuleInfoWrapper.Create(GetInfo?.Invoke(Object));
        private ModuleInfoWrapper? _info;

        public object Object { get; }

        private LauncherModuleVMWrapper(object @object)
        {
            Object = @object;
        }
    }
}