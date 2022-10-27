using Bannerlord.BUTR.Shared.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Features.AssemblyResolver.Patches
{
    internal static class AssemblyLoaderPatch
    {
        private static readonly Type? EngineApplicationInterfaceType =
            AccessTools2.TypeByName("TaleWorlds.Engine.EngineApplicationInterface");

        private static readonly AccessTools.FieldRef<object>? IUtil =
            AccessTools2.StaticFieldRefAccess<object>(EngineApplicationInterfaceType, "IUtil");

        private delegate string GetModulesCodeDelegate(object instance);

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
                var iUtil = IUtil?.Invoke();
                var getModulesCode = AccessTools2.GetDelegate<GetModulesCodeDelegate>(iUtil, "GetModulesCode");
                var isInGame = getModulesCode is not null;

                var name = args.Name.Contains(',') ? $"{args.Name.Split(',')[0]}.dll" : args.Name;

                var assemblies = (isInGame
                        ? ModuleInfoHelper.GetLoadedModules().OfType<ModuleInfoExtendedWithMetadata>()
                        : ModuleInfoHelper.GetModules().OfType<ModuleInfoExtendedWithMetadata>())
                    .Select(x => Directory.GetFiles(Path.Combine(x.Path, "bin", Common.ConfigName), "*.dll")).ToArray();

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