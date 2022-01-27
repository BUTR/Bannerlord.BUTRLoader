using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using TaleWorlds.MountAndBlade.Launcher.UserDatas;

using static Bannerlord.BUTRLoader.Helpers.ModuleInfoWrapper;

namespace Bannerlord.BUTRLoader.Tests.Patches
{
    internal class UserDataManagerPatch2 : IDisposable
    {
        private readonly Harmony _harmony;
        private static ModuleStorage? _currentModuleStorage;

        public UserDataManagerPatch2(Harmony harmony, ModuleStorage moduleStorage)
        {
            if (_currentModuleStorage != null)
                throw new Exception();

            _harmony = harmony;
            _currentModuleStorage = moduleStorage;

            if (!harmony.TryPatch(
                SymbolExtensions2.GetPropertyInfo((UserDataManager udm) => udm.UserData).GetMethod,
                prefix: AccessTools2.Method(typeof(UserDataManagerPatch2), nameof(GetUserDataPrefix))))
            {
                Assert.Fail();
            }

            if (!harmony.TryPatch(
                SymbolExtensions2.GetConstructorInfo(() => new UserDataManager()),
                prefix: AccessTools2.Method(typeof(UserDataManagerPatch2), nameof(UserDataManagerConstructorPrefix))))
            {
                Assert.Fail();
            }

            if (!harmony.TryPatch(
                SymbolExtensions2.GetMethodInfo((UserDataManager udm) => udm.SaveUserData()),
                prefix: AccessTools2.Method(typeof(UserDataManagerPatch2), nameof(SaveUserDataPrefix))))
            {
                Assert.Fail();
            }

            if (!harmony.TryPatch(
                SymbolExtensions2.GetMethodInfo((UserDataManager udm) => udm.LoadUserData()),
                prefix: AccessTools2.Method(typeof(UserDataManagerPatch2), nameof(LoadUserDataPrefix))))
            {
                Assert.Fail();
            }

            if (!harmony.TryPatch(
                SymbolExtensions2.GetMethodInfo((UserDataManager udm) => udm.HasUserData()),
                prefix: AccessTools2.Method(typeof(UserDataManagerPatch2), nameof(HasUserDataPrefix))))
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
                    ModDatas = _currentModuleStorage!.GetModuleInfos().ConvertAll(mi => new UserModData(GetId.Invoke(mi), GetIsSelected.Invoke(mi)))
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
                AccessTools2.Method(typeof(UserDataManagerPatch2), nameof(GetUserDataPrefix)));

            _harmony.Unpatch(
                SymbolExtensions2.GetConstructorInfo(() => new UserDataManager()),
                AccessTools2.Method(typeof(UserDataManagerPatch2), nameof(UserDataManagerConstructorPrefix)));

            _harmony.Unpatch(
                SymbolExtensions2.GetMethodInfo((UserDataManager udm) => udm.SaveUserData()),
                AccessTools2.Method(typeof(UserDataManagerPatch2), nameof(SaveUserDataPrefix)));

            _harmony.Unpatch(
                SymbolExtensions2.GetMethodInfo((UserDataManager udm) => udm.LoadUserData()),
                AccessTools2.Method(typeof(UserDataManagerPatch2), nameof(LoadUserDataPrefix)));

            _harmony.Unpatch(
                SymbolExtensions2.GetMethodInfo((UserDataManager udm) => udm.HasUserData()),
                AccessTools2.Method(typeof(UserDataManagerPatch2), nameof(HasUserDataPrefix)));

            _currentModuleStorage = null;
        }
    }
}