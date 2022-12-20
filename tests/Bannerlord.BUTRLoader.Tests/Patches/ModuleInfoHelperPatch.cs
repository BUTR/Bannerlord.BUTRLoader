using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.ModuleManager;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Bannerlord.BUTRLoader.Tests.Patches
{
    internal class ModuleInfoHelperPatch : IDisposable
    {
        private readonly Harmony _harmony;
        private static ModuleStorage? _currentModuleStorage;

        public ModuleInfoHelperPatch(Harmony harmony, ModuleStorage moduleStorage)
        {
            if (_currentModuleStorage != null)
                throw new Exception();

            _harmony = harmony;
            _currentModuleStorage = moduleStorage;

            if (!harmony.TryPatch(
                original: AccessTools2.Method(typeof(ModuleInfoHelper), nameof(ModuleInfoHelper.GetModules)),
                prefix: AccessTools2.Method(typeof(ModuleInfoHelperPatch), nameof(GetModulesPrefix))))
            {
                Assert.Fail();
            }
            if (!harmony.TryPatch(
                    original: AccessTools2.Method(typeof(ModuleInfoHelper), nameof(ModuleInfoHelper.GetPhysicalModules)),
                    prefix: AccessTools2.Method(typeof(ModuleInfoHelperPatch), nameof(GetModulesEmptyPrefix))))
            {
                Assert.Fail();
            }
            if (!harmony.TryPatch(
                    original: AccessTools2.Method(typeof(ModuleInfoHelper), nameof(ModuleInfoHelper.GetPlatformModules)),
                    prefix: AccessTools2.Method(typeof(ModuleInfoHelperPatch), nameof(GetModulesEmptyPrefix))))
            {
                Assert.Fail();
            }
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetModulesPrefix(ref IEnumerable<ModuleInfoExtended> __result)
        {
            __result = _currentModuleStorage!.GetModuleInfos();
            return false;
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetModulesEmptyPrefix(ref IEnumerable<ModuleInfoExtended> __result)
        {
            __result = Enumerable.Empty<ModuleInfoExtended>();
            return false;
        }

        public void Dispose()
        {
            _harmony.Unpatch(
                AccessTools2.Method(typeof(ModuleInfoHelper), nameof(ModuleInfoHelper.GetModules)),
                AccessTools2.Method(typeof(ModuleInfoHelperPatch), nameof(GetModulesPrefix)));
            _harmony.Unpatch(
                AccessTools2.Method(typeof(ModuleInfoHelper), nameof(ModuleInfoHelper.GetPhysicalModules)),
                AccessTools2.Method(typeof(ModuleInfoHelperPatch), nameof(GetModulesEmptyPrefix)));
            _harmony.Unpatch(
                AccessTools2.Method(typeof(ModuleInfoHelper), nameof(ModuleInfoHelper.GetPlatformModules)),
                AccessTools2.Method(typeof(ModuleInfoHelperPatch), nameof(GetModulesEmptyPrefix)));

            _currentModuleStorage = null;
        }
    }
}