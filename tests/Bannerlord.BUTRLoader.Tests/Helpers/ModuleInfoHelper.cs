using Bannerlord.BUTR.Shared.ModuleInfoExtended;
using Bannerlord.BUTRLoader.Helpers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Tests.Helpers
{
    internal static class ModuleInfoHelper
    {
        public static ModuleInfo ModuleInfo(ModuleInfoModel model)
        {
            var moduleInfo = new ModuleInfo();
            Populate(model, moduleInfo);
            return moduleInfo;
        }
        public static ModuleInfo2 ModuleInfo2(ModuleInfoModel model)
        {
            var moduleInfo = new ModuleInfo2();
            Populate(model, moduleInfo);
            return moduleInfo;
        }

        public static void Populate(ModuleInfoModel model, ModuleInfo moduleInfo)
        {
            SymbolExtensions2.GetPropertyInfo((ModuleInfo mi) => mi.Id).SetValue(moduleInfo, model.Id);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo mi) => mi.Name).SetValue(moduleInfo, model.Name);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo mi) => mi.IsOfficial).SetValue(moduleInfo, model.IsOfficial);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo mi) => mi.Version).SetValue(moduleInfo, model.Version);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo mi) => mi.Alias).SetValue(moduleInfo, model.Alias);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo mi) => mi.IsSingleplayerModule).SetValue(moduleInfo, model.IsSingleplayerModule);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo mi) => mi.IsMultiplayerModule).SetValue(moduleInfo, model.IsMultiplayerModule);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo mi) => mi.IsSelected).SetValue(moduleInfo, model.IsSelected);
            SymbolExtensions2.GetFieldInfo((ModuleInfo mi) => mi.SubModules).SetValue(moduleInfo, model.SubModules);
            if (typeof(ModuleInfo).GetField("DependedModules")?.GetValue(moduleInfo) is IList originaNewList)
            {
                var dependedModuleType = typeof(ApplicationVersion).Assembly.GetType("TaleWorlds.Library.DependedModule");
                originaNewList.Clear();
                foreach (var dependedModule in model.DependedModules.Select(dm => Activator.CreateInstance(dependedModuleType, dm.ModuleId, dm.Version)))
                    originaNewList.Add(dependedModule);
            }
            if (typeof(ModuleInfo).GetField("DependedModuleIds")?.GetValue(moduleInfo) is List<string> originalList)
            {
                originalList.Clear();
                foreach (var dependedModule in model.DependedModules)
                    originalList.Add(dependedModule.ModuleId);
            }
        }

        public static void Populate(ModuleInfoModel model, ModuleInfo2 moduleInfo)
        {
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.Id).SetValue(moduleInfo, model.Id);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.Name).SetValue(moduleInfo, model.Name);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.IsOfficial).SetValue(moduleInfo, model.IsOfficial);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.Version).SetValue(moduleInfo, model.Version);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.Alias).SetValue(moduleInfo, model.Alias);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.IsSingleplayerModule).SetValue(moduleInfo, model.IsSingleplayerModule);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.IsMultiplayerModule).SetValue(moduleInfo, model.IsMultiplayerModule);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.IsSelected).SetValue(moduleInfo, model.IsSelected);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.SubModules).SetValue(moduleInfo, model.SubModules);
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.DependedModules).SetValue(moduleInfo, model.DependedModules
                .ConvertAll(dm => new BUTR.Shared.ModuleInfoExtended.DependedModule { ModuleId = dm.ModuleId, Version = dm.Version})
            );
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.DependedModuleMetadatas).SetValue(moduleInfo, model.DependedModuleMetadatas);
        }
    }
}