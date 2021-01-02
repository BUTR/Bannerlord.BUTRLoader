using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.ModuleInfoExtended;
using Bannerlord.BUTRLoader.Tests.Helpers;

using HarmonyLib;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Bannerlord.BUTRLoader.Tests.Patches
{
    internal class ModuleInfo2Patch : IDisposable
    {
        private readonly Harmony _harmony;
        private static ModuleStorage? _currentModuleStorage;

        public ModuleInfo2Patch(Harmony harmony, ModuleStorage moduleStorage)
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
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool LoadPrefix(ModuleInfo2 __instance, string alias)
        {
            var model = _currentModuleStorage!.ModuleInfoModels.Find(mi => mi.Alias == alias);
            ModuleInfoHelper.Populate(model, __instance);
            return false;
        }

        public void Dispose()
        {
            _harmony.Unpatch(
                SymbolExtensions.GetMethodInfo((ModuleInfo2 mi2) => mi2.Load(null!)),
                AccessTools.Method(typeof(ModuleInfo2Patch), nameof(LoadPrefix)));

            _currentModuleStorage = null;
        }
    }
}