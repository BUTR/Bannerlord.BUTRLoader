using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.BUTRLoader
{
    internal static class ModuleSorter
    {
        public static IEnumerable<ExtendedModuleInfo> GetDependentModulesOf(IEnumerable<ExtendedModuleInfo> source, ExtendedModuleInfo module)
        {
            var sourceList = source.ToList();

            foreach (var dependedModuleId in module.DependedModuleIds)
            {
                if (sourceList.Find(i => i.Id == dependedModuleId) is { } moduleInfo)
                {
                    yield return moduleInfo;
                }
            }

            foreach (var dependedModuleMetadata in module.DependedModuleMetadatas)
            {
                if (sourceList.Find(i => i.Id == dependedModuleMetadata.Id) is { } moduleInfo && dependedModuleMetadata.LoadType == LoadType.LoadBeforeThis)
                {
                    yield return moduleInfo;
                }
            }

            foreach (var moduleInfo in sourceList)
            {
                foreach (var dependedModuleMetadata in moduleInfo.DependedModuleMetadatas)
                {
                    if (dependedModuleMetadata.Id == module.Id && dependedModuleMetadata.LoadType == LoadType.LoadAfterThis)
                    {
                        yield return moduleInfo;
                    }
                }
            }
        }
    }
}