using Bannerlord.BUTR.Shared.ModuleInfoExtended;
using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using static Bannerlord.BUTRLoader.Helpers.ModuleInfoHelper;

namespace Bannerlord.BUTRLoader.Tests.Helpers
{
    internal static class ModuleInfoHelper
    {
        public static object ModuleInfo(ModuleInfoModel model)
        {
            var moduleInfo = Activator.CreateInstance(OldModuleInfoType ?? NewModuleInfoType);
            Populate(model, moduleInfo);
            return moduleInfo;
        }
        public static ModuleInfo2 ModuleInfo2(ModuleInfoModel model)
        {
            var moduleInfo = new ModuleInfo2();
            Populate(model, moduleInfo);
            return moduleInfo;
        }

        public static void Populate(SubModuleInfo2 model, object subModule)
        {
            var type = OldSubModuleInfoType ?? NewSubModuleInfoType;

            AccessTools.Property(type, "Name")?.SetValue(subModule, model.Name);
            AccessTools.Property(type, "DLLName")?.SetValue(subModule, model.DLLName);
            AccessTools.Property(type, "DLLExists")?.SetValue(subModule, model.DLLExists);
            AccessTools.Property(type, "Assemblies")?.SetValue(subModule, model.Assemblies);
            AccessTools.Property(type, "SubModuleClassType")?.SetValue(subModule, model.SubModuleClassType);
            AccessTools.Property(type, "Tags")?.SetValue(subModule, model.Tags);
        }

        public static void Populate(ModuleInfoModel model, object moduleInfo)
        {
            var type = OldModuleInfoType ?? NewModuleInfoType;

            AccessTools.Property(type, "Id")?.SetValue(moduleInfo, model.Id);
            AccessTools.Property(type, "Name")?.SetValue(moduleInfo, model.Name);
            AccessTools.Property(type, "IsOfficial")?.SetValue(moduleInfo, model.IsOfficial);
            AccessTools.Property(type, "Version")?.SetValue(moduleInfo, model.Version);
            AccessTools.Property(type, "Alias")?.SetValue(moduleInfo, model.Alias);
            AccessTools.Property(type, "IsSingleplayerModule")?.SetValue(moduleInfo, model.IsSingleplayerModule);
            AccessTools.Property(type, "IsMultiplayerModule")?.SetValue(moduleInfo, model.IsMultiplayerModule);
            AccessTools.Property(type, "IsSelected")?.SetValue(moduleInfo, model.IsSelected);
            if (type.GetField("SubModules")?.GetValue(moduleInfo) is IList subModules)
            {
                var subModuleInfoType = OldSubModuleInfoType ?? NewSubModuleInfoType;
                subModules.Clear();
                foreach (var subModule in model.SubModules.Select(dm =>
                {
                    var sub = Activator.CreateInstance(subModuleInfoType);
                    Populate(dm, sub);
                    return sub;
                }))
                {
                    subModules.Add(subModule);
                }
            }
            if (type.GetField("DependedModules")?.GetValue(moduleInfo) is IList originalNewList)
            {
                var dependedModuleType = OldDependedModuleType ?? NewDependedModuleType;
                originalNewList.Clear();
                foreach (var dependedModule in model.DependedModules.Select(dm => Activator.CreateInstance(dependedModuleType, dm.ModuleId, dm.Version)))
                    originalNewList.Add(dependedModule);
            }
            if (type.GetField("DependedModuleIds")?.GetValue(moduleInfo) is List<string> originalList)
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