using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.ModuleInfoExtended;

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
            SymbolExtensions2.GetFieldInfo((ModuleInfo mi) => mi.DependedModules).SetValue(moduleInfo, model.DependedModules);
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
                .ConvertAll(dm => new ModuleInfoExtended.DependedModule { ModuleId = dm.ModuleId, Version = dm.Version})
            );
            SymbolExtensions2.GetPropertyInfo((ModuleInfo2 mi) => mi.DependedModuleMetadatas).SetValue(moduleInfo, model.DependedModuleMetadatas);
        }
    }
}