using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Helpers;

using HarmonyLib;

using Ikriv.Xml;

using System;
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
    internal class UserDataManagerPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((UserDataManager udm) => udm.LoadUserData()),
                prefix: AccessTools.Method(typeof(UserDataManagerPatch), nameof(LoadUserDataPrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                SymbolExtensions.GetMethodInfo((UserDataManager udm) => udm.SaveUserData()),
                prefix: AccessTools.Method(typeof(UserDataManagerPatch), nameof(SaveUserDataPrefix)));
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
                var setMethod = SymbolExtensions2.GetPropertyInfo((UserDataManager ud) => ud.UserData).SetMethod;
                setMethod.Invoke(__instance, new object?[] { userDataOptions as UserDataOld });
                BUTRLoaderAppDomainManager.ExtendedSorting = userDataOptions.ExtendedSorting;
                BUTRLoaderAppDomainManager.AutomaticallyCheckForUpdates = userDataOptions.AutomaticallyCheckForUpdates;
                BUTRLoaderAppDomainManager.UnblockFiles = userDataOptions.UnblockFiles;
                BUTRLoaderAppDomainManager.FixCommonIssues = userDataOptions.FixCommonIssues;
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
                    BUTRLoaderAppDomainManager.ExtendedSorting,
                    BUTRLoaderAppDomainManager.AutomaticallyCheckForUpdates,
                    BUTRLoaderAppDomainManager.UnblockFiles,
                    BUTRLoaderAppDomainManager.FixCommonIssues));
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
            }

            return false;
        }
    }
}