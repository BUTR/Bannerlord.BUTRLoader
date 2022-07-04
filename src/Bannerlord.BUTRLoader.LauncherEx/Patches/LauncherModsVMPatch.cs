using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.LauncherEx;
using Bannerlord.BUTRLoader.Wrappers;
using Bannerlord.ModuleManager;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;

using static Bannerlord.BUTRLoader.Helpers.ModuleInfoHelper2;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class LauncherModsVMPatch
    {
        private static readonly MethodInfo? CastModuleInfoMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))?.MakeGenericMethod(
                AccessTools2.TypeByName("TaleWorlds.Library.ModuleInfo") ??
                AccessTools2.TypeByName("TaleWorlds.ModuleManager.ModuleInfo"));

        private static readonly MethodInfo? ToListModuleInfoMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))?.MakeGenericMethod(
            AccessTools2.TypeByName("TaleWorlds.Library.ModuleInfo") ??
            AccessTools2.TypeByName("TaleWorlds.ModuleManager.ModuleInfo"));

        private delegate IEnumerable CastDelegate(IEnumerable instance);
        private static readonly CastDelegate? CastMethod =
            AccessTools2.GetDelegate<CastDelegate>(CastModuleInfoMethodInfo!);

        private delegate IEnumerable ToListDelegate(IEnumerable instance);
        private static readonly ToListDelegate? ToListMethod =
            AccessTools2.GetDelegate<ToListDelegate>(ToListModuleInfoMethodInfo!);

        private static readonly MethodInfo? GetDependentModulesOfMethodInfo =
            AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.LauncherModsVM:GetDependentModulesOf") ??
            AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModsVM:GetDependentModulesOf") ??
            AccessTools2.DeclaredMethod("TaleWorlds.ModuleManager.ModuleHelper:GetDependentModulesOf");

        private static readonly MethodInfo? IsAllDependenciesOfModulePresentMethodInfo =
            AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.LauncherModsVM:IsAllDependenciesOfModulePresent") ??
            AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModsVM:IsAllDependenciesOfModulePresent");

        private static readonly MethodInfo? ChangeIsSelectedOfMethodInfo =
            AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.LauncherModsVM:ChangeIsSelectedOf") ??
            AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModsVM:ChangeIsSelectedOf");

        private static readonly MethodInfo? LoadSubModulesMethodInfo =
            AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.LauncherModsVM:LoadSubModules") ??
            AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModsVM:LoadSubModules");


        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                GetDependentModulesOfMethodInfo,
                prefix: AccessTools2.DeclaredMethod("Bannerlord.BUTRLoader.Patches.LauncherModsVMPatch:GetDependentModulesOfPrefix"));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                IsAllDependenciesOfModulePresentMethodInfo,
                prefix: AccessTools2.DeclaredMethod("Bannerlord.BUTRLoader.Patches.LauncherModsVMPatch:AreAllDependenciesOfModulePresentPrefix"));
            if (!res2) return false;

            var res3 = harmony.TryPatch(
                ChangeIsSelectedOfMethodInfo,
                prefix: AccessTools2.DeclaredMethod("Bannerlord.BUTRLoader.Patches.LauncherModsVMPatch:ChangeIsSelectedOfPrefix"));
            if (!res3) return false;

            var res4 = harmony.TryPatch(
                LoadSubModulesMethodInfo,
                prefix: AccessTools2.DeclaredMethod("Bannerlord.BUTRLoader.Patches.LauncherModsVMPatch:LoadSubModulesPrefix"));
            if (!res4) return false;

            return true;
        }

        public static void Disable(Harmony harmony)
        {
            harmony.Unpatch(
                GetDependentModulesOfMethodInfo,
                AccessTools2.DeclaredMethod("Bannerlord.BUTRLoader.Patches.LauncherModsVMPatch:GetDependentModulesOfPrefix"));

            harmony.Unpatch(
                IsAllDependenciesOfModulePresentMethodInfo,
                AccessTools2.DeclaredMethod("Bannerlord.BUTRLoader.Patches.LauncherModsVMPatch:AreAllDependenciesOfModulePresentPrefix"));

            harmony.Unpatch(
                ChangeIsSelectedOfMethodInfo,
                AccessTools2.DeclaredMethod("Bannerlord.BUTRLoader.Patches.LauncherModsVMPatch:ChangeIsSelectedOfPrefix"));

            harmony.Unpatch(
                LoadSubModulesMethodInfo,
                AccessTools2.DeclaredMethod("Bannerlord.BUTRLoader.Patches.LauncherModsVMPatch:LoadSubModulesPrefix"));
        }

        private static Func<ModuleInfoExtended?, bool> GetIsSelected(LauncherModsVMWrapper instance) => module =>
        {
            if (instance.Modules.FirstOrDefault(m => m.Info?.Id == module?.Id) is { } wrapper)
                return wrapper.IsSelected;
            return false;
        };
        private static Action<ModuleInfoExtended?, bool> SetIsSelected(LauncherModsVMWrapper instance) => (module, value) =>
        {
            if (instance.Modules.FirstOrDefault(m => m.Info?.Id == module?.Id) is { } wrapper)
                wrapper.IsSelected = value;
        };
        private static Func<ModuleInfoExtended?, bool> GetIsDisabled(LauncherModsVMWrapper instance) => module =>
        {
            if (instance.Modules.FirstOrDefault(m => m.Info?.Id == module?.Id) is { } wrapper)
                return wrapper.IsDisabled;
            return false;
        };
        private static Action<ModuleInfoExtended?, bool> SetIsDisabled(LauncherModsVMWrapper instance) => (module, value) =>
        {
            if (instance.Modules.FirstOrDefault(m => m.Info?.Id == module?.Id) is { } wrapper)
                wrapper.IsDisabled = value;
        };

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetDependentModulesOfPrefix(IEnumerable<object> source, object module, ref IEnumerable __result)
        {
            if (!LauncherSettings.ExtendedSorting || CastMethod is null || ToListMethod is null)
                return true;

            var extendedModuleInfo = GetExtendedModuleInfo(module);
            if (extendedModuleInfo is null) return true;

            var sourceList = source.ToList();
            var extendedSourceList = sourceList.ConvertAll(GetExtendedModuleInfo);
            // Any incorrect module will be null. Filter it out
            while (extendedSourceList.IndexOf(null) is var idx && idx != -1)
            {
                sourceList.RemoveAt(idx);
                extendedSourceList.RemoveAt(idx);
            }

            var validModules = ValidModules.Where(kv => kv.Value).Select(x => x.Key).ToArray();
            var extendedDependencies = ModuleUtilities.GetDependencies(validModules, extendedModuleInfo).Except(new[] { extendedModuleInfo });
            var dependencies = extendedDependencies.Select(em => sourceList.Find(m => ModuleInfoWrapper.Create(m).Id == em.Id)).Where(x => x is not null);

            var castedItems = CastMethod.Invoke(dependencies);
            __result = ToListMethod.Invoke(castedItems);
            return false;
        }

        /// <summary>
        /// Decides if the Module is valid.
        /// We replace the original method with our implementation.
        /// </summary>
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool AreAllDependenciesOfModulePresentPrefix(object __instance, object info, ref bool __result)
        {
            if (!LauncherSettings.ExtendedSorting)
                return true;

            var wrapped = LauncherModsVMWrapper.Create(__instance);
            var info2 = GetExtendedModuleInfo(info);
            if (info2 is null) // Any incorrect module will be null. Filter it out
            {
                __result = false;
                return false;
            }

            // External dependencies should not disable a mod. Instead, they itself should be disabled
            __result = true;
            var visited = new HashSet<ModuleInfoExtended>();
            var opt = new ModuleSorterOptions { SkipOptionals = true, SkipExternalDependencies = true };
            foreach (var dependency in ModuleUtilities.GetDependencies(ExtendedModuleInfoCache.Values, info2, visited, opt))
            {
                if (!ModuleIsCorrect(wrapped, dependency, visited))
                {
                    if (dependency != info2)
                        IssueStorage.AppendIssue(wrapped, info2, $"'{dependency.Name}' has unresolved issues!");

                    __result = false;
                    return false;
                }
            }
            return false;
        }
        private static bool AreAllDependenciesOfModulePresent(object launcherModsVM, object moduleInfo)
        {
            var result = false;
            AreAllDependenciesOfModulePresentPrefix(launcherModsVM, moduleInfo, ref result);
            return result;
        }

        private static bool CheckModuleCompatibility(LauncherModsVMWrapper instance, ModuleInfoExtended moduleInfoExtended)
        {
            foreach (var subModule in moduleInfoExtended.SubModules.Where(CheckIfSubModuleCanBeLoaded))
            {
                var asm = Path.GetFullPath(Path.Combine(BasePath.Name, "Modules", moduleInfoExtended.Id, "bin", "Win64_Shipping_Client", subModule.DLLName));
                switch (Manager._compatibilityChecker.CheckAssembly(asm))
                {
                    case CheckResult.TypeLoadException:
                        IssueStorage.AppendIssue(instance, moduleInfoExtended, "Not binary compatible with the current game version!");
                        return false;
                    case CheckResult.ReflectionTypeLoadException:
                        IssueStorage.AppendIssue(instance, moduleInfoExtended, "Not binary compatible with the current game version!");
                        return false;
                    case CheckResult.GenericException:
                        IssueStorage.AppendIssue(instance, moduleInfoExtended, "There was an error checking for binary compatibility with the current game version");
                        return false;
                }
            }

            return true;
        }
        private static bool ModuleIsCorrect(LauncherModsVMWrapper instance, ModuleInfoExtended moduleInfoExtended, HashSet<ModuleInfoExtended> visited)
        {
            if (!CheckModuleCompatibility(instance, moduleInfoExtended))
            {
                ValidModules[moduleInfoExtended] = false;
                SetIsDisabled(instance)(moduleInfoExtended, false);
                return false;
            }

            var validationResult = ModuleUtilities.ValidateModule(ExtendedModuleInfoCache.Values, moduleInfoExtended, visited, GetIsSelected(instance)).ToList();

            if (validationResult.Count > 0)
            {
                ValidModules[moduleInfoExtended] = false;
                SetIsDisabled(instance)(moduleInfoExtended, false);
                foreach (var issue in validationResult)
                {
                    IssueStorage.AppendIssue(instance, issue.Target, issue.Reason);
                }
                return false;
            }
            else
            {
                ValidModules[moduleInfoExtended] = true;
                IssueStorage.ClearIssues(instance, moduleInfoExtended);
                return true;
            }
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool ChangeIsSelectedOfPrefix(object __instance, object targetModule)
        {
            if (!LauncherSettings.ExtendedSorting)
                return true;

            var targetModuleWrapped = LauncherModuleVMWrapper.Create(targetModule);
            if (LauncherModuleVMWrapper.Create(targetModule) is not { Info.Object: { } obj } || !AreAllDependenciesOfModulePresent(__instance, obj))
            {
                // Direct and current External Dependencies are not valid, do nothing
                return false;
            }

            var wrapped = LauncherModsVMWrapper.Create(__instance);
            ChangeIsSelectedOf(wrapped, targetModuleWrapped);

            return false;
        }

        private static void ChangeIsSelectedOf(LauncherModsVMWrapper instance, LauncherModuleVMWrapper targetModule)
        {
            if (targetModule.Info is not { } info || GetExtendedModuleInfo(info) is not { } targetModuleInfoExtended) return;

            var validModules = ValidModules.Where(kv => kv.Value).Select(x => x.Key).ToArray();
            var result = ToggleModuleSelection(validModules, targetModuleInfoExtended, GetIsSelected(instance), SetIsSelected(instance), GetIsDisabled(instance), SetIsDisabled(instance)).ToList();
            foreach (var issue in result)
            {
                IssueStorage.AppendIssue(instance, issue.Target, issue.Reason);
            }
        }
        private static IEnumerable<ModuleIssue> ToggleModuleSelection(IReadOnlyCollection<ModuleInfoExtended> modules, ModuleInfoExtended targetModule, Func<ModuleInfoExtended, bool> getSelected, Action<ModuleInfoExtended, bool> setSelected, Func<ModuleInfoExtended, bool> getDisabled, Action<ModuleInfoExtended, bool> setDisabled)
        {
            if (getSelected(targetModule))
            {
                foreach (var issue in ModuleUtilities.DisableModule(modules, targetModule, getSelected, setSelected, getDisabled, setDisabled))
                    yield return issue;
            }
            else
            {
                foreach (var issue in ModuleUtilities.EnableModule(modules, targetModule, getSelected, setSelected, getDisabled, setDisabled))
                    yield return issue;
            }
        }

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadSubModulesPrefix(object __instance)
        {
            var wrapped = LauncherModsVMWrapper.Create(__instance);
            foreach (var module in wrapped.Modules.Select(x => x.Object).OfType<ViewModel>())
            {
                module.OnPropertyChanged("Refresh_Command");
            }
        }
    }
}