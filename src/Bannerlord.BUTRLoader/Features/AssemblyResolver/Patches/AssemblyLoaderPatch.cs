using Bannerlord.BUTR.Shared.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using TaleWorlds.Engine;
using TaleWorlds.Library;

using Path = System.IO.Path;

namespace Bannerlord.BUTRLoader.Features.AssemblyResolver.Patches
{
    internal static class AssemblyLoaderPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(typeof(AssemblyLoader), "OnAssemblyResolve"),
                prefix: AccessTools2.Method(typeof(AssemblyLoaderPatch), nameof(OnAssemblyResolvePrefix)));
            if (!res1) return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool OnAssemblyResolvePrefix(ref Assembly? __result, ResolveEventArgs args)
        {
            try
            {
                var basePath = Utilities.GetBasePath();
                var configName = Common.ConfigName;
                var modulePath = Path.GetFullPath(Path.Combine(basePath, "Modules"));

                var name = args.Name.Contains(',') ? $"{args.Name.Split(',')[0]}.dll" : args.Name;

                var assemblies = ModuleInfoHelper.GetLoadedModules()
                    .Select(x => Path.Combine(modulePath, x.Id, "bin", configName))
                    .Where(Directory.Exists)
                    .Select(x => Directory.GetFiles(x, "*.dll")).ToArray();

                var assembly = assemblies
                    .SelectMany(x => x)
                    .FirstOrDefault(x => x.Contains(name));

                if (assembly is not null)
                {
                    __result = Assembly.LoadFrom(assembly);
                    return false;
                }
            }
            catch (Exception) { }

            return true;
        }
    }
}