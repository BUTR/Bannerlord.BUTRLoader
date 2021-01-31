using Bannerlord.BUTR.Shared.ModuleInfoExtended;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Tests.Helpers;

using HarmonyLib;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

using static Bannerlord.BUTRLoader.Helpers.LauncherModuleVMExtensions;

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
                SymbolExtensions.GetMethodInfo((ModuleInfo2 mi2) => mi2.Load((string) null!)),
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
        private static bool LoadPrefix(ModuleInfo2 __instance, string __0)
        {
            if (OldModuleInfoType is not null)
            {
                var model = _currentModuleStorage!.ModuleInfoModels.Find(mi => mi.Alias == __0);
                ModuleInfoHelper.Populate(model, __instance);
                return false;
            }

            if (NewModuleInfoType is not null)
            {
                var moduleId = Path.GetFileNameWithoutExtension(__0);
                var model = _currentModuleStorage!.ModuleInfoModels.Find(mi => mi.Alias == moduleId);
                ModuleInfoHelper.Populate(model, __instance);
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            _harmony.Unpatch(
                SymbolExtensions.GetMethodInfo((ModuleInfo2 mi2) => mi2.Load((string) null!)),
                AccessTools.Method(typeof(ModuleInfo2Patch), nameof(LoadPrefix)));

            _currentModuleStorage = null;
        }
    }
}