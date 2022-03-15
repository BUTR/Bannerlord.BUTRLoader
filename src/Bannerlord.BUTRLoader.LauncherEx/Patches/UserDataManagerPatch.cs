using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using Ikriv.Xml;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;

using TaleWorlds.MountAndBlade.Launcher.UserDatas;

using UserDataOld = TaleWorlds.MountAndBlade.Launcher.UserDatas.UserData;
using UserDataOptions = Bannerlord.BUTRLoader.Options.UserData;

namespace Bannerlord.BUTRLoader.Patches
{
    internal static class UserDataManagerPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                SymbolExtensions2.GetMethodInfo((UserDataManager udm) => udm.LoadUserData()),
                prefix: AccessTools2.Method(typeof(UserDataManagerPatch), nameof(LoadUserDataPrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                SymbolExtensions2.GetMethodInfo((UserDataManager udm) => udm.SaveUserData()),
                prefix: AccessTools2.Method(typeof(UserDataManagerPatch), nameof(SaveUserDataPrefix)));
            if (!res2) return false;

            return true;
        }

        private static XmlAttributeOverrides GetOverrides()
        {
            return new OverrideXml()
                .Override<UserDataOld>()
                .XmlType("UserDataOld")
                .Commit();
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool LoadUserDataPrefix(UserDataManager __instance, string ____filePath)
        {
            if (!File.Exists(____filePath))
            {
                return true;
            }
            var xmlSerializer = new XmlSerializer(typeof(UserDataOptions), GetOverrides());
            try
            {
                using var xmlReader = XmlReader.Create(____filePath);
                var userDataOptions = (UserDataOptions) xmlSerializer.Deserialize(xmlReader);
                LauncherSettings.ExtendedSorting = userDataOptions.ExtendedSorting;
                LauncherSettings.AutomaticallyCheckForUpdates = userDataOptions.AutomaticallyCheckForUpdates;
                LauncherSettings.UnblockFiles = userDataOptions.UnblockFiles;
                LauncherSettings.FixCommonIssues = userDataOptions.FixCommonIssues;
                LauncherSettings.CompactModuleList = userDataOptions.CompactModuleList;
                LauncherSettings.ResetModuleList = userDataOptions.ResetModuleList;
                if (LauncherSettings.ResetModuleList)
                {
                    userDataOptions.SingleplayerData.ModDatas = new List<UserModData>();
                    LauncherSettings.ResetModuleList = false;
                }
                var setMethod = SymbolExtensions2.GetPropertyInfo((UserDataManager ud) => ud.UserData)?.SetMethod;
                setMethod?.Invoke(__instance, new object?[] { userDataOptions as UserDataOld });
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
            }

            return false;
        }

        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool SaveUserDataPrefix(UserDataManager __instance, string ____filePath)
        {
            var xmlSerializer = new XmlSerializer(typeof(UserDataOptions), GetOverrides());
            try
            {
                using XmlWriter xmlWriter = XmlWriter.Create(____filePath, new XmlWriterSettings { Indent = true });
                xmlSerializer.Serialize(xmlWriter, new UserDataOptions(
                    __instance.UserData,
                    LauncherSettings.ExtendedSorting,
                    LauncherSettings.AutomaticallyCheckForUpdates,
                    LauncherSettings.UnblockFiles,
                    LauncherSettings.FixCommonIssues,
                    LauncherSettings.CompactModuleList,
                    LauncherSettings.ResetModuleList));
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
            }

            return false;
        }
    }
}