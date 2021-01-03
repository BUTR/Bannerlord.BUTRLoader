using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using TaleWorlds.MountAndBlade.Launcher.UserDatas;

namespace Bannerlord.BUTRLoader.Tests.Patches
{
    internal class UserDataManagerPatch : IDisposable
    {
        private readonly Harmony _harmony;
        private static ModuleStorage? _currentModuleStorage;

        public UserDataManagerPatch(Harmony harmony, ModuleStorage moduleStorage)
        {
            if (_currentModuleStorage != null)
                throw new Exception();

            _harmony = harmony;
            _currentModuleStorage = moduleStorage;

            if (!harmony.TryPatch(
                SymbolExtensions2.GetPropertyInfo((UserDataManager udm) => udm.UserData).GetMethod,
                prefix: AccessTools.Method(typeof(UserDataManagerPatch), nameof(GetUserDataPrefix))))
            {
                Assert.Fail();
            }

            if (!harmony.TryPatch(
                SymbolExtensions2.GetConstructorInfo(() => new UserDataManager()),
                prefix: AccessTools.Method(typeof(UserDataManagerPatch), nameof(UserDataManagerConstructorPrefix))))
            {
                Assert.Fail();
            }

            if (!harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((UserDataManager udm) => udm.SaveUserData()),
                prefix: AccessTools.Method(typeof(UserDataManagerPatch), nameof(SaveUserDataPrefix))))
            {
                Assert.Fail();
            }

            if (!harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((UserDataManager udm) => udm.LoadUserData()),
                prefix: AccessTools.Method(typeof(UserDataManagerPatch), nameof(LoadUserDataPrefix))))
            {
                Assert.Fail();
            }

            if (!harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((UserDataManager udm) => udm.HasUserData()),
                prefix: AccessTools.Method(typeof(UserDataManagerPatch), nameof(HasUserDataPrefix))))
            {
                Assert.Fail();
            }
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetUserDataPrefix(ref UserData __result)
        {
            __result = new UserData
            {
                GameType = GameType.Singleplayer,
                SingleplayerData = new UserGameTypeData
                {
                    ModDatas = _currentModuleStorage!.GetModuleInfos().ConvertAll(mi => new UserModData(mi.Id, mi.IsSelected))
                }
            };

            return false;
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool UserDataManagerConstructorPrefix() => false;

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool SaveUserDataPrefix() => false;

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool LoadUserDataPrefix() => false;

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool HasUserDataPrefix() => false;

        public void Dispose()
        {
            _harmony.Unpatch(
                SymbolExtensions2.GetPropertyInfo((UserDataManager udm) => udm.UserData).GetMethod,
                AccessTools.Method(typeof(UserDataManagerPatch), nameof(GetUserDataPrefix)));

            _harmony.Unpatch(
                SymbolExtensions2.GetConstructorInfo(() => new UserDataManager()),
                AccessTools.Method(typeof(UserDataManagerPatch), nameof(UserDataManagerConstructorPrefix)));

            _harmony.Unpatch(
                SymbolExtensions.GetMethodInfo((UserDataManager udm) => udm.SaveUserData()),
                AccessTools.Method(typeof(UserDataManagerPatch), nameof(SaveUserDataPrefix)));

            _harmony.Unpatch(
                SymbolExtensions.GetMethodInfo((UserDataManager udm) => udm.LoadUserData()),
                AccessTools.Method(typeof(UserDataManagerPatch), nameof(LoadUserDataPrefix)));

            _harmony.Unpatch(
                SymbolExtensions.GetMethodInfo((UserDataManager udm) => udm.HasUserData()),
                AccessTools.Method(typeof(UserDataManagerPatch), nameof(HasUserDataPrefix)));

            _currentModuleStorage = null;
        }
    }
}