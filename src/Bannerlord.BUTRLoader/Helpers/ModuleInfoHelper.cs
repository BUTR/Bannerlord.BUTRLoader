using HarmonyLib;

using System;
using System.Linq;
using System.Reflection;

using TaleWorlds.MountAndBlade.Launcher;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class ModuleInfoHelper
    {
        public static readonly Type? OldDependedModuleType = Type.GetType("TaleWorlds.Library.DependedModule, TaleWorlds.Library", false);
        public static readonly Type? NewDependedModuleType = Type.GetType("TaleWorlds.ModuleManager.DependedModule, TaleWorlds.ModuleManager", false);

        public static readonly Type? ModuleHelperType = Type.GetType("TaleWorlds.ModuleManager.ModuleHelper, TaleWorlds.ModuleManager", false);

        public static readonly Type? OldModuleInfoType = Type.GetType("TaleWorlds.Library.ModuleInfo, TaleWorlds.Library", false);
        public static readonly Type? NewModuleInfoType = Type.GetType("TaleWorlds.ModuleManager.ModuleInfo, TaleWorlds.ModuleManager", false);

        public static readonly Type? OldSubModuleInfoType = Type.GetType("TaleWorlds.Library.SubModuleInfo, TaleWorlds.Library", false);
        public static readonly Type? NewSubModuleInfoType = Type.GetType("TaleWorlds.ModuleManager.SubModuleInfo, TaleWorlds.ModuleManager", false);

        public static readonly MethodInfo? GetId;
        public static readonly MethodInfo? GetAlias;
        public static readonly MethodInfo? GetIsSelected;

        public static readonly FieldInfo? GetInfo;

        public static readonly MethodInfo? CastMethod;
        public static readonly MethodInfo? ToListMethod;

        private delegate string GetIdDelegate(object instance);
        private delegate string GetAliasDelegate(object instance);

        static ModuleInfoHelper()
        {
            GetInfo = AccessTools.Field(typeof(LauncherModuleVM), "Info");

            if (OldModuleInfoType is { })
            {
                GetId = AccessTools.PropertyGetter(OldModuleInfoType, "Id");
                GetAlias = AccessTools.PropertyGetter(OldModuleInfoType, "Alias");
                GetIsSelected = AccessTools.PropertyGetter(OldModuleInfoType, "IsSelected");
                CastMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))?.MakeGenericMethod(OldModuleInfoType);
                ToListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))?.MakeGenericMethod(OldModuleInfoType);
            }
            if (NewModuleInfoType is { })
            {
                GetId = AccessTools.PropertyGetter(NewModuleInfoType, "Id");
                GetAlias = AccessTools.PropertyGetter(NewModuleInfoType, "Alias");
                GetIsSelected = AccessTools.PropertyGetter(NewModuleInfoType, "IsSelected");
                CastMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))?.MakeGenericMethod(NewModuleInfoType);
                ToListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))?.MakeGenericMethod(NewModuleInfoType);
            }
        }
    }
}
