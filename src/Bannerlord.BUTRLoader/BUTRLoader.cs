using Bannerlord.BUTRLoader;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;

[SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
[SuppressMessage("ReSharper", "CheckNamespace")]
internal sealed class BUTRLoader : AppDomainManager
{
    public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
    {
        base.InitializeNewDomain(appDomainInfo);

        AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
        {
            // Wait for the Launcher assembly to load
            if (args.LoadedAssembly.GetName().Name == "TaleWorlds.MountAndBlade.Launcher")
            {
                var modulesDir = Path.Combine(BasePath.Name, "Modules");
                var harmonyDir = Path.Combine(modulesDir, "Bannerlord.Harmony");

                if (Directory.Exists(harmonyDir))
                {
                    var harmonyAssemblyPath = Path.Combine(harmonyDir, "bin", Common.ConfigName, "0Harmony.dll");
                    if (File.Exists(harmonyAssemblyPath))
                    {
                        Assembly.LoadFile(harmonyAssemblyPath);
                        HarmonyFlow();
                    }
                    else
                    {
                        NoHarmonyFlow();
                    }
                }
                else
                {
                    NoHarmonyFlow();
                }
            }
        };
    }

    private static readonly Dictionary<string, ExtendedModuleInfo> ExtendedModuleInfos = new Dictionary<string, ExtendedModuleInfo>();
    private static ExtendedModuleInfo GetExtendedModuleInfo(ModuleInfo moduleInfo)
    {
        if (ExtendedModuleInfos.ContainsKey(moduleInfo.Id))
            return ExtendedModuleInfos[moduleInfo.Id];

        var extendedModuleInfo = new ExtendedModuleInfo();
        extendedModuleInfo.Load(moduleInfo.Alias);
        ExtendedModuleInfos[moduleInfo.Id] = extendedModuleInfo;
        return extendedModuleInfo;
    }
    private static bool GetDependentModulesOfPrefix(IEnumerable<ModuleInfo> source, ModuleInfo module, ref IEnumerable<ModuleInfo> __result)
    {
        var sourceList = source.ToList();
        var extendedSourceList = sourceList.ConvertAll(GetExtendedModuleInfo);

        var extendedModuleInfo = GetExtendedModuleInfo(module);

        var extendedDependencies = ModuleSorter.GetDependentModulesOf(extendedSourceList, extendedModuleInfo);
        var dependencies = extendedDependencies.Select(em => sourceList.First(m => m.Id == em.Id));

        __result = dependencies;
        return false;
    }

    // We don't directly depend on Harmony to avoid runtime linking issues
    private static bool HarmonyFlow()
    {
        var harmonyAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .FirstOrDefault(a => Path.GetFileNameWithoutExtension(a.CodeBase) == "0Harmony");
        if (harmonyAssembly == null)
            return false;

        var harmonyType = harmonyAssembly.GetType("HarmonyLib.Harmony");
        var harmonyMethodType = harmonyAssembly.GetType("HarmonyLib.HarmonyMethod");
        var harmonyPatchMethod = harmonyType.GetMethod("Patch");
        if (harmonyType == null || harmonyMethodType == null || harmonyPatchMethod == null)
            return false;

        var prefixMethod = typeof(BUTRLoader).GetMethod(nameof(GetDependentModulesOfPrefix), BindingFlags.Static | BindingFlags.NonPublic);
        var toPatchMethod = typeof(LauncherModsVM).GetMethod("GetDependentModulesOf", BindingFlags.Instance | BindingFlags.NonPublic);

        var harmonyInstance = Activator.CreateInstance(harmonyType, "Bannerlord.BUTRLoader");
        var harmonyMethodInstance = Activator.CreateInstance(harmonyMethodType, prefixMethod);

        harmonyPatchMethod.Invoke(harmonyInstance, new[] { toPatchMethod, harmonyMethodInstance, null, null, null });

        return true;
    }

    private static bool NoHarmonyFlow()
    {
        return true;
    }
}