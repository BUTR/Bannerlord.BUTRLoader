using Bannerlord.ModuleManager;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class IssueStorage
    {
        public static readonly Dictionary<string, HashSet<string>> Issues = new();
        private static void SetIssue(string moduleId, IEnumerable<string> issues)
        {
            if (Issues.TryGetValue(moduleId, out var list))
            {
                foreach (var issue in issues)
                {
                    if (!list.Contains(issue))
                        list.Add(issue);
                }
            }
            else
            {
                Issues.Add(moduleId, new HashSet<string>(issues));
            }
        }

        public static void AppendIssue(LauncherModsVM viewModelWrapper, ModuleInfoExtended ModuleInfoExtended, string issue)
        {
            //viewModelWrapper.ExecuteCommand("SetIssue", new object[] { ModuleInfoExtended.Id,  new string[] { issue } });
            SetIssue(ModuleInfoExtended.Id, new[] { issue });

            var moduleVM = viewModelWrapper.Modules.FirstOrDefault(m => m.Info?.Id == ModuleInfoExtended.Id);
            moduleVM?.ExecuteCommand("UpdateIssues", Array.Empty<object>());
        }
        public static void AppendIssue(LauncherModuleVM viewModelWrapper, string issue)
        {
            var id = viewModelWrapper.Info?.Id ?? string.Empty;
            SetIssue(id, new[] { issue });

            viewModelWrapper.ExecuteCommand("UpdateIssues", Array.Empty<object>());
        }
        public static void ClearIssues(LauncherModsVM viewModelWrapper, ModuleInfoExtended ModuleInfoExtended)
        {
            //Issues.Remove(ModuleInfoExtended.Id);
            if (Issues.TryGetValue(ModuleInfoExtended.Id, out var list))
                list.Clear();

            var moduleVM = viewModelWrapper.Modules.FirstOrDefault(m => m.Info?.Id == ModuleInfoExtended.Id);
            moduleVM?.ExecuteCommand("UpdateIssues", Array.Empty<object>());
        }
        public static void ClearIssues(LauncherModuleVM viewModelWrapper)
        {
            var id = viewModelWrapper.Info?.Id ?? string.Empty;

            //Issues.Remove(ModuleInfoExtended.Id);
            if (Issues.TryGetValue(id, out var list))
                list.Clear();

            viewModelWrapper.ExecuteCommand("UpdateIssues", Array.Empty<object>());
        }
    }
}