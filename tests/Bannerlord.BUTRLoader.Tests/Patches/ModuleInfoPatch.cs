using Bannerlord.BUTRLoader.Tests.Helpers;

using Bannerlord.BUTRLoader.Patches;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using NUnit.Framework;

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

using static Bannerlord.BUTRLoader.Helpers.ModuleInfoWrapper;

using AccessTools2 = HarmonyLib.BUTR.Extensions.AccessTools2;

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
                AccessTools2.Method(OldModuleInfoType, "Load") ?? AccessTools2.Method(NewModuleInfoType, "LoadWithFullPath"),
                prefix: AccessTools2.Method(typeof(ModuleInfoPatch), nameof(LoadPrefix))))
            {
                Assert.Fail();
            }

            harmony.TryPatch(
                AccessTools2.Method(OldModuleInfoType, "GetModules"),
                prefix: AccessTools2.Method(typeof(ModuleInfoPatch), nameof(GetModulesPrefix)));
            harmony.TryPatch(
                AccessTools2.Method(ModuleHelperType, "GetModules"),
                prefix: AccessTools2.Method(typeof(ModuleInfoPatch), nameof(GetModulesPrefix)));
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
            var castedItems = LauncherModsVMPatch.CastMethod.Invoke(_currentModuleStorage!.GetModuleInfos());
            __result = LauncherModsVMPatch.ToListMethod.Invoke(castedItems);
            return false;
        }

        public void Dispose()
        {
            _harmony.Unpatch(
                AccessTools2.Method(OldModuleInfoType, "Load") ?? AccessTools2.Method(NewModuleInfoType, "LoadWithFullPath"),
                AccessTools2.Method(typeof(ModuleInfoPatch), nameof(LoadPrefix)));

            try
            {
                _harmony.Unpatch(
                    AccessTools2.Method(OldModuleInfoType, "GetModules"),
                    AccessTools2.Method(typeof(ModuleInfoPatch), nameof(GetModulesPrefix)));
                _harmony.Unpatch(
                    AccessTools2.Method(ModuleHelperType, "GetModules"),
                    AccessTools2.Method(typeof(ModuleInfoPatch), nameof(GetModulesPrefix)));
            }
            catch { }


            _currentModuleStorage = null;
        }
    }
}