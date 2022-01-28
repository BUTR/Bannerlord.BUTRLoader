using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Patches;
using Bannerlord.BUTRLoader.Tests.Patches;

using HarmonyLib;

using NUnit.Framework;

using System;
using System.Linq;
using System.Reflection;

using TaleWorlds.MountAndBlade.Launcher;
using TaleWorlds.MountAndBlade.Launcher.UserDatas;

namespace Bannerlord.BUTRLoader.Tests
{
    public class LauncherModsVMTests
    {
        private readonly Harmony _harmony = new("Bannerlord.BUTRLoader.Tests.LauncherModsVMTests");

        public static Array GetModuleListTemplates() => Enum.GetValues(typeof(ModuleListTemplates));

        [OneTimeSetUp]
        public void Setup()
        {
            Assembly.Load("TaleWorlds.Library");
            Assembly.Load("TaleWorlds.ModuleManager");

            LauncherModsVMPatch.Enable(_harmony);
        }

        [Test]
        [TestCaseSource(nameof(GetModuleListTemplates))]
        public void Test_Refresh(ModuleListTemplates moduleListTemplate)
        {
            ModuleInfoHelper2.ExtendedModuleInfoCache.Clear();

            var storage = new ModuleStorage(moduleListTemplate);
            using var _ = new ModuleInfoPatch(_harmony, storage);
            using var __ = new ModuleInfo2Patch(_harmony, storage);
            using var ___ = new UserDataManagerPatch2(_harmony, storage);

            var userDataManager = new UserDataManager();
            var viewModel = new LauncherModsVM(userDataManager);

            viewModel.Refresh(false);

            var sorted = viewModel.Modules.Where(m => m.IsSelected).Select(m => m.Info.Id).ToList();
            CollectionAssert.AreEqual(storage.ExpectedIdOrder, sorted);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            LauncherModsVMPatch.Disable(_harmony);
        }
    }
}