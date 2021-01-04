using Bannerlord.BUTRLoader.ModuleInfoExtended;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class ModuleSorter
    {
        private static void Visit<T>(T item, Func<T, IEnumerable<T>> getDependencies, ICollection<T> sorted, HashSet<T> visited)
        {
            if (visited.TryGetValue(item, out _))
                return;

            visited.Add(item);
            if (getDependencies(item) is { } enumerable)
            {
                foreach (var item2 in enumerable)
                    Visit(item2, getDependencies, sorted, visited);
            }
            sorted.Add(item);
        }

        private static IEnumerable<ModuleInfo2> GetDependentModulesOfInternal(IEnumerable<ModuleInfo2> source, ModuleInfo2 module, bool skipExternalDependencies = false)
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

            if (!skipExternalDependencies)
            {
                foreach (var moduleInfo in sourceList)
                {
                    foreach (var dependedModuleMetadata in moduleInfo.DependedModuleMetadatas)
                    {
                        if (dependedModuleMetadata.LoadType != LoadType.LoadAfterThis)
                            continue;

                        if (dependedModuleMetadata.Id != module.Id)
                            continue;

                        yield return moduleInfo;
                    }
                }
            }
        }

        public static IEnumerable<ModuleInfo2> GetDependentModulesOf(IEnumerable<ModuleInfo2> source, ModuleInfo2 module, HashSet<ModuleInfo2> visited, bool skipExternalDependencies = false)
        {
            var dependencies = new List<ModuleInfo2>();
            Visit(module, x => GetDependentModulesOfInternal(source, x, skipExternalDependencies), dependencies, visited);
            return dependencies;
        }

        public static IEnumerable<ModuleInfo2> GetDependentModulesOf(IEnumerable<ModuleInfo2> source, ModuleInfo2 module, bool skipExternalDependencies = false)
        {
            var visited = new HashSet<ModuleInfo2>();
            return GetDependentModulesOf(source, module, visited, skipExternalDependencies);
        }
    }
}