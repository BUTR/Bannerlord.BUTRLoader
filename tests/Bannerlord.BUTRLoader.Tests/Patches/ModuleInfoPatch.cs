using Bannerlord.BUTRLoader.Tests.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using NUnit.Framework;

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

using static Bannerlord.BUTRLoader.Helpers.LauncherModuleVMExtensions;

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
                AccessTools.Method(OldModuleInfoType, "Load") ?? AccessTools.Method(NewModuleInfoType, "LoadWithFullPath"),
                prefix: AccessTools.Method(typeof(ModuleInfo2Patch), nameof(LoadPrefix))))
            {
                Assert.Fail();
            }

            harmony.TryPatch(
                AccessTools.Method(OldModuleInfoType, "GetModules"),
                prefix: AccessTools.Method(typeof(ModuleInfoPatch), nameof(GetModulesPrefix)));
            harmony.TryPatch(
                AccessTools.Method(ModuleHelperType, "GetModules"),
                prefix: AccessTools.Method(typeof(ModuleInfoPatch), nameof(GetModulesPrefix)));
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool LoadPrefix(ref object __instance, string __0)
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

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetModulesPrefix(ref IEnumerable __result)
        {
            var castedItems = CastMethod.Invoke(null, new object[] { _currentModuleStorage!.GetModuleInfos() });
            __result = (IEnumerable) ToListMethod.Invoke(null, new object[] { castedItems });
            return false;
        }

        public void Dispose()
        {
            _harmony.Unpatch(
                AccessTools.Method(OldModuleInfoType, "Load") ?? AccessTools.Method(NewModuleInfoType, "LoadWithFullPath"),
                AccessTools.Method(typeof(ModuleInfoPatch), nameof(LoadPrefix)));

            try
            {
                _harmony.Unpatch(
                    AccessTools.Method(OldModuleInfoType, "GetModules"),
                    AccessTools.Method(typeof(ModuleInfoPatch), nameof(GetModulesPrefix)));
                _harmony.Unpatch(
                    AccessTools.Method(ModuleHelperType, "GetModules"),
                    AccessTools.Method(typeof(ModuleInfoPatch), nameof(GetModulesPrefix)));
            }
            catch { }


            _currentModuleStorage = null;
        }
    }
}