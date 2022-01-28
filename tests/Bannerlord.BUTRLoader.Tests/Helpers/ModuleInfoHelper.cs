using Bannerlord.ModuleManager;

using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Library;

using static Bannerlord.BUTRLoader.Helpers.ModuleInfoWrapper;

namespace Bannerlord.BUTRLoader.Tests.Helpers
{
    internal static class ModuleInfoHelper
    {
        private static TaleWorlds.Library.ApplicationVersion Convert(Bannerlord.ModuleManager.ApplicationVersion version) => new(
            (TaleWorlds.Library.ApplicationVersionType) version.ApplicationVersionType,
            version.Major,
            version.Minor,
            version.Revision,
            version.ChangeSet,
            ApplicationVersionGameType.Singleplayer);

        public static object ModuleInfo(ModuleInfoModel model)
        {
            var moduleInfo = Activator.CreateInstance(OldModuleInfoType ?? NewModuleInfoType);
            Populate(model, moduleInfo);
            return moduleInfo;
        }
        public static ModuleInfoExtended ModuleInfo2(ModuleInfoModel model)
        {
            var moduleInfo = new ModuleInfoExtended();
            Populate(model, moduleInfo);
            return moduleInfo;
        }

        public static void Populate(SubModuleInfoExtended model, object subModule)
        {
            var type = OldSubModuleInfoType ?? NewSubModuleInfoType;

            AccessTools2.Property(type, "Name")?.SetValue(subModule, model.Name);
            AccessTools2.Property(type, "DLLName")?.SetValue(subModule, model.DLLName);
            AccessTools2.Property(type, "DLLExists")?.SetValue(subModule, true);
            AccessTools2.Property(type, "Assemblies")?.SetValue(subModule, model.Assemblies);
            AccessTools2.Property(type, "SubModuleClassType")?.SetValue(subModule, model.SubModuleClassType);
            AccessTools2.Property(type, "Tags")?.SetValue(subModule, model.Tags);
        }

        public static void Populate(ModuleInfoModel model, object moduleInfo)
        {
            var type = OldModuleInfoType ?? NewModuleInfoType;

            AccessTools2.Property(type, "Id")?.SetValue(moduleInfo, model.Id);
            AccessTools2.Property(type, "Name")?.SetValue(moduleInfo, model.Name);
            AccessTools2.Property(type, "IsOfficial")?.SetValue(moduleInfo, model.IsOfficial);
            AccessTools2.Property(type, "Version")?.SetValue(moduleInfo, Convert(model.Version));
            AccessTools2.Property(type, "Alias")?.SetValue(moduleInfo, model.Alias);
            AccessTools2.Property(type, "IsSingleplayerModule")?.SetValue(moduleInfo, model.IsSingleplayerModule);
            AccessTools2.Property(type, "IsMultiplayerModule")?.SetValue(moduleInfo, model.IsMultiplayerModule);
            AccessTools2.Property(type, "IsSelected")?.SetValue(moduleInfo, model.IsSelected);
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
                foreach (var dependedModule in model.DependedModules.Select(dm => Activator.CreateInstance(dependedModuleType, dm.Id, Convert(dm.Version), false)))
                    originalNewList.Add(dependedModule);
            }
            if (type.GetField("DependedModuleIds")?.GetValue(moduleInfo) is List<string> originalList)
            {
                originalList.Clear();
                foreach (var dependedModule in model.DependedModules)
                    originalList.Add(dependedModule.Id);
            }
        }

        public static void Populate(ModuleInfoModel model, ModuleInfoExtended moduleInfo)
        {
            SymbolExtensions2.GetPropertyInfo((ModuleInfoExtended mi) => mi.Id).SetValue(moduleInfo, model.Id);
            SymbolExtensions2.GetPropertyInfo((ModuleInfoExtended mi) => mi.Name).SetValue(moduleInfo, model.Name);
            SymbolExtensions2.GetPropertyInfo((ModuleInfoExtended mi) => mi.IsOfficial).SetValue(moduleInfo, model.IsOfficial);
            SymbolExtensions2.GetPropertyInfo((ModuleInfoExtended mi) => mi.Version).SetValue(moduleInfo, model.Version);
            SymbolExtensions2.GetPropertyInfo((ModuleInfoExtended mi) => mi.IsSingleplayerModule).SetValue(moduleInfo, model.IsSingleplayerModule);
            SymbolExtensions2.GetPropertyInfo((ModuleInfoExtended mi) => mi.IsMultiplayerModule).SetValue(moduleInfo, model.IsMultiplayerModule);
            //SymbolExtensions2.GetPropertyInfo((ModuleInfoExtended mi) => mi.IsSelected).SetValue(moduleInfo, model.IsSelected);
            SymbolExtensions2.GetPropertyInfo((ModuleInfoExtended mi) => mi.SubModules).SetValue(moduleInfo, model.SubModules);
            SymbolExtensions2.GetPropertyInfo((ModuleInfoExtended mi) => mi.DependentModules).SetValue(moduleInfo, model.DependedModules
                .ConvertAll(dm => new DependentModule { Id = dm.Id, Version = dm.Version })
            );
            SymbolExtensions2.GetPropertyInfo((ModuleInfoExtended mi) => mi.DependentModuleMetadatas).SetValue(moduleInfo, model.DependedModuleMetadatas);
        }
    }
}