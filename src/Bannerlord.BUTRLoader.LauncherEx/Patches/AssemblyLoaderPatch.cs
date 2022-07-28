﻿using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Shared.Utils;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class AssemblyLoaderPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(typeof(AssemblyLoader), "LoadFrom"),
                prefix: AccessTools2.Method(typeof(AssemblyLoaderPatch), nameof(LoadFromPrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                AccessTools2.Method(typeof(AssemblyLoader), "OnAssemblyResolve"),
                prefix: AccessTools2.Method(typeof(AssemblyLoaderPatch), nameof(OnAssemblyResolvePrefix)));
            if (!res2) return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool LoadFromPrefix(ref Assembly? __result, string assemblyFile)
        {
            if (assemblyFile.Contains("Modules"))
            {
                var module = ModuleInfoHelper.GetModuleByType(new TypeWrapper(Path.GetFullPath(assemblyFile)));
                if (module is not null)
                {
                    var filename = Path.GetFileName(assemblyFile);
                    var subModule = module.SubModules.FirstOrDefault(sm => sm.DLLName == filename);
                    if (subModule is not null)
                    {
                        if (subModule.Tags.TryGetValue("LoadReferencesOnLoad", out var list) && string.Equals(list.FirstOrDefault(), "false", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                __result = Assembly.LoadFrom(assemblyFile);
                            }
                            catch (Exception)
                            {
                                __result = null;
                            }
                            return false;
                        }
                    }

                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool OnAssemblyResolvePrefix(ref Assembly? __result, object sender, ResolveEventArgs args)
        {
            if (sender is AppDomain { FriendlyName: "Compatibility Checker" } domain)
            {
                foreach (var assembly in domain.GetAssemblies())
                {
                    if (assembly.FullName == args.Name)
                    {
                        __result = assembly;
                        return false;
                    }
                }

                var name = args.Name.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                __result = Assembly.LoadFrom(name[0]);
                return false;
            }

            return true;
        }
    }
}