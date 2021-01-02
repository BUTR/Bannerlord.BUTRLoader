using Bannerlord.BUTRLoader.ModuleInfoExtended;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class ModuleSorter
    {
        public static void Visit<T>(T item, Func<T, IEnumerable<T>> getDependencies, List<T> sorted, Dictionary<T, bool> visited)
        {
            if (visited.TryGetValue(item, out _))
                return;

            visited[item] = true;
            if (getDependencies(item) is { } enumerable)
            {
                foreach (var item2 in enumerable)
                    Visit(item2, getDependencies, sorted, visited);
            }
            visited[item] = false;
            sorted.Add(item);
        }

        public static IEnumerable<ModuleInfo2> GetDependentModulesOf(IEnumerable<ModuleInfo2> source, ModuleInfo2 module)
        {
            var sourceList = source.ToList();

            foreach (var dependedModule in module.DependedModules)
            {
                if (sourceList.Find(i => i.Id == dependedModule.ModuleId) is { } moduleInfo)
                {
                    yield return moduleInfo;
                }
            }

            foreach (var dependedModuleMetadata in module.DependedModuleMetadatas)
            {
                if (dependedModuleMetadata.LoadType != LoadType.LoadBeforeThis)
                    continue;

                var moduleInfo = sourceList.Find(i => i.Id == dependedModuleMetadata.Id);
                if (!dependedModuleMetadata.IsOptional && moduleInfo is null)
                {
                    // We should not hit this place
                }
                else if (moduleInfo is not null)
                {
                    yield return moduleInfo;
                }
            }

            foreach (var moduleInfo in sourceList.Where(mi => mi.IsSelected))
            {
                foreach (var dependedModuleMetadata in moduleInfo.DependedModuleMetadatas)
                {
                    if (dependedModuleMetadata.LoadType != LoadType.LoadAfterThis)
                        continue;

                    if (dependedModuleMetadata.Id == module.Id)
                    {
                        yield return moduleInfo;
                    }
                }
            }
        }
    }
}