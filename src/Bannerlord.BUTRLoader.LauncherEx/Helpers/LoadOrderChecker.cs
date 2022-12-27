using Bannerlord.ModuleManager;

using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class LoadOrderChecker
    {
        public static IEnumerable<ModuleIssue> IsLoadOrderCorrect(IReadOnlyList<ModuleInfoExtended> modules)
        {
            var loadOrder = FeatureIds.LauncherFeatures.Select(x => new ModuleInfoExtended { Id = x }).Concat(modules).ToList();
            foreach (var module in modules)
            {
                var issues = ModuleUtilities.ValidateLoadOrder(loadOrder, module).ToList();
                if (issues.Any())
                    return issues;
            }
            return Enumerable.Empty<ModuleIssue>();
        }
    }
}