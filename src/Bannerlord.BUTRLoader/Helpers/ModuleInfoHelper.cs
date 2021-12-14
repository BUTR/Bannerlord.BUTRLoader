using Bannerlord.ModuleManager;

using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using TaleWorlds.MountAndBlade.Launcher;

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
        public static ModuleInfoWrapper Create(object? @object) => new(@object);

        public string Id => _id ??= Object is null ? string.Empty : LauncherModuleVMExtensions.GetId?.Invoke(Object, Array.Empty<object>()) as string ?? string.Empty;
        private string? _id;
        public string Alias => _alias ??= Object is null ? string.Empty : LauncherModuleVMExtensions.GetAlias?.Invoke(Object, Array.Empty<object>()) as string ?? string.Empty;
        private string? _alias;
        public bool IsSelected => Object is null ? false : LauncherModuleVMExtensions.GetIsSelected?.Invoke(Object, Array.Empty<object>()) as bool? ?? false;

        public object? Object { get; }

        private ModuleInfoWrapper(object? @object)
        {
            Object = @object;
        }
    }

    internal sealed class LauncherModuleVMWrapper
    {
        public static LauncherModuleVMWrapper Create(object @object) => new(@object);

        public ModuleInfoWrapper Info => _info ??= ModuleInfoWrapper.Create(LauncherModuleVMExtensions.GetInfo?.GetValue(Object));
        private ModuleInfoWrapper? _info;

        public object Object { get; }

        private LauncherModuleVMWrapper(object @object)
        {
            Object = @object;
        }
    }

    internal static class LauncherModuleVMExtensions
    {
        public static readonly Type? OldDependedModuleType = Type.GetType("TaleWorlds.Library.DependedModule, TaleWorlds.Library", false);
        public static readonly Type? NewDependedModuleType = Type.GetType("TaleWorlds.ModuleManager.DependedModule, TaleWorlds.ModuleManager", false);

        public static readonly Type? ModuleHelperType = Type.GetType("TaleWorlds.ModuleManager.ModuleHelper, TaleWorlds.ModuleManager", false);

        public static readonly Type? OldModuleInfoType = Type.GetType("TaleWorlds.Library.ModuleInfo, TaleWorlds.Library", false);
        public static readonly Type? NewModuleInfoType = Type.GetType("TaleWorlds.ModuleManager.ModuleInfo, TaleWorlds.ModuleManager", false);

        public static readonly Type? OldSubModuleInfoType = Type.GetType("TaleWorlds.Library.SubModuleInfo, TaleWorlds.Library", false);
        public static readonly Type? NewSubModuleInfoType = Type.GetType("TaleWorlds.ModuleManager.SubModuleInfo, TaleWorlds.ModuleManager", false);

        public static readonly MethodInfo? GetId;
        public static readonly MethodInfo? GetAlias;
        public static readonly MethodInfo? GetIsSelected;

        public static readonly FieldInfo? GetInfo;

        public static readonly MethodInfo? CastMethod;
        public static readonly MethodInfo? ToListMethod;

        //private delegate string GetIdDelegate(object instance);
        //private delegate string GetAliasDelegate(object instance);

        static LauncherModuleVMExtensions()
        {
            GetInfo = AccessTools2.Field(typeof(LauncherModuleVM), "Info");

            if (OldModuleInfoType is { })
            {
                GetId = AccessTools2.PropertyGetter(OldModuleInfoType, "Id");
                GetAlias = AccessTools2.PropertyGetter(OldModuleInfoType, "Alias");
                GetIsSelected = AccessTools2.PropertyGetter(OldModuleInfoType, "IsSelected");
                CastMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))?.MakeGenericMethod(OldModuleInfoType);
                ToListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))?.MakeGenericMethod(OldModuleInfoType);
            }
            if (NewModuleInfoType is { })
            {
                GetId = AccessTools2.PropertyGetter(NewModuleInfoType, "Id");
                GetAlias = AccessTools2.PropertyGetter(NewModuleInfoType, "Alias");
                GetIsSelected = AccessTools2.PropertyGetter(NewModuleInfoType, "IsSelected");
                CastMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))?.MakeGenericMethod(NewModuleInfoType);
                ToListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))?.MakeGenericMethod(NewModuleInfoType);
            }
        }
    }
}