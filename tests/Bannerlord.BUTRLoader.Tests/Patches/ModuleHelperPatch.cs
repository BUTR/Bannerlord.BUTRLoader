using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

using TaleWorlds.ModuleManager;

namespace Bannerlord.BUTRLoader.Tests.Patches
{
    internal class ModuleHelperPatch : IDisposable
    {
        private readonly Harmony _harmony;
        private static ModuleStorage? _currentModuleStorage;

        public ModuleHelperPatch(Harmony harmony, ModuleStorage moduleStorage)
        {
            if (_currentModuleStorage != null)
                throw new Exception();

            _harmony = harmony;
            _currentModuleStorage = moduleStorage;

            if (!harmony.TryPatch(
                original: AccessTools2.Method(typeof(ModuleHelper), "GetModules"),
                prefix: AccessTools2.Method(typeof(ModuleHelperPatch), nameof(GetModulesPrefix))))
            {
                Assert.Fail();
            }
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetModulesPrefix(ref IEnumerable<ModuleInfo> __result)
        {
            __result = Enumerable.Empty<ModuleInfo>();
            return false;
        }

        public void Dispose()
        {
            _harmony.Unpatch(
                AccessTools2.Method(typeof(ModuleHelper), "GetModules"),
                AccessTools2.Method(typeof(ModuleHelperPatch), nameof(GetModulesPrefix)));

            _currentModuleStorage = null;
        }
    }
}