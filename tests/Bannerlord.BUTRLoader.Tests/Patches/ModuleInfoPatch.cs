using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.ModuleInfoExtended;
using Bannerlord.BUTRLoader.Tests.Helpers;

using HarmonyLib;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Tests.Patches
{
    internal class ModuleInfoPatch : IDisposable
    {
        private readonly Harmony _harmony;
        private static ModuleStorage? _currentModuleStorage;

        public ModuleInfoPatch(Harmony harmony, ModuleStorage moduleStorage)
        {
            if (_currentModuleStorage != null)
                throw new Exception();

            _harmony = harmony;
            _currentModuleStorage = moduleStorage;

            if (!harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((ModuleInfo2 mi2) => mi2.Load(null!)),
                prefix: AccessTools.Method(typeof(ModuleInfo2Patch), nameof(LoadPrefix))))
            {
                Assert.Fail();
            }

            if (!harmony.TryPatch(
                SymbolExtensions.GetMethodInfo(() => ModuleInfo.GetModules()),
                prefix: AccessTools.Method(typeof(ModuleInfoPatch), nameof(GetModulesPrefix))))
            {
                Assert.Fail();
            }
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool LoadPrefix(ref ModuleInfo __instance, string alias)
        {
            var model = _currentModuleStorage!.ModuleInfoModels.Find(mi => mi.Alias == alias);
            ModuleInfoHelper.Populate(model, __instance);
            return false;
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetModulesPrefix(ref IEnumerable<ModuleInfo> __result)
        {
            __result = _currentModuleStorage!.GetModuleInfos();
            return false;
        }

        public void Dispose()
        {
            _harmony.Unpatch(
                SymbolExtensions.GetMethodInfo((ModuleInfo2 mi2) => mi2.Load(null!)),
                AccessTools.Method(typeof(ModuleInfo2Patch), nameof(LoadPrefix)));

            _harmony.Unpatch(
                SymbolExtensions.GetMethodInfo(() => ModuleInfo.GetModules()),
                AccessTools.Method(typeof(ModuleInfoPatch), nameof(GetModulesPrefix)));

            _currentModuleStorage = null;
        }
    }
}