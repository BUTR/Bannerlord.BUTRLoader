using Bannerlord.BUTRLoader.Patches;
using Bannerlord.BUTRLoader.Patches.Mixins;
using Bannerlord.BUTRLoader.Tests.Patches;

using HarmonyLib;

using NUnit.Framework;

using System;
using System.Linq;

using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.BUTRLoader.Tests
{
    public class LauncherModsVMTests
    {
        private readonly Harmony _harmony = new("Bannerlord.BUTRLoader.Tests.LauncherModsVMTests");

        public static Array GetModuleListTemplates() => Enum.GetValues(typeof(ModuleListTemplates));

        [OneTimeSetUp]
        public void Setup()
        {
            LauncherModsVMPatch.Enable(_harmony);
        }

        [Test]
        [TestCaseSource(nameof(GetModuleListTemplates))]
        public void Test_Refresh(ModuleListTemplates moduleListTemplate)
        {
            var storage = new ModuleStorage(moduleListTemplate);
            using var _ = new ModuleInfoHelperPatch(_harmony, storage);
            using var __ = new ModuleHelperPatch(_harmony, storage);

            var userDataManager = new UserDataManager();
            var viewModel = new LauncherModsVM(userDataManager);
            var mixin = new LauncherModsVMMixin(viewModel);

            mixin.Initialize(false);
            foreach (var moduleVM in mixin.Modules2)
            {
                if (storage.ModuleInfoModels.Find(x => x.Id == moduleVM.ModuleInfoExtended.Id).IsSelected != moduleVM.IsSelected)
                    moduleVM.ExecuteSelect();
            }
            mixin.ExecuteRefresh();

            var sorted = mixin.Modules2.Where(m => m.IsSelected).Select(m => m.ModuleInfoExtended.Id).ToList();
            CollectionAssert.AreEqual(storage.ExpectedIdOrder, sorted);
        }

        [Test]
        [TestCaseSource(nameof(GetModuleListTemplates))]
        public void Test_LoadLoadOrder(ModuleListTemplates moduleListTemplate)
        {
            var storage = new ModuleStorage(moduleListTemplate);
            using var _ = new ModuleInfoHelperPatch(_harmony, storage);
            using var __ = new ModuleHelperPatch(_harmony, storage);

            var userDataManager = new UserDataManager
            {
                UserData =
                {
                    GameType = GameType.Singleplayer,
                    SingleplayerData = new UserGameTypeData
                    {
                        ModDatas = new(storage.ExpectedIdOrder.Select(x =>
                        {
                            var module = storage.ModuleInfoModels.Find(y => y.Id == x);
                            return new UserModData(module.Id, module.Version.ToString(), module.IsSelected);
                        }))
                    }
                }
            };
            var viewModel = new LauncherModsVM(userDataManager);
            var mixin = new LauncherModsVMMixin(viewModel);

            mixin.Initialize(false);

            var sorted = mixin.Modules2.Where(m => m.IsSelected).Select(m => m.ModuleInfoExtended.Id).ToList();
            CollectionAssert.AreEqual(storage.ExpectedIdOrder, sorted);
            Assert.False(mixin.IsForceSorted);
        }
    }
}