using UserDataOld = TaleWorlds.MountAndBlade.Launcher.UserDatas.UserData;

namespace Bannerlord.BUTRLoader.Options
{
    public sealed class UserData : UserDataOld
    {
        public bool ExtendedSorting { get; set; } = true;
        public bool AutomaticallyCheckForUpdates { get; set; }
        public bool UnblockFiles { get; set; } = true;
        public bool FixCommonIssues { get; set; }
        public bool CompactModuleList { get; set; }
        public bool ResetModuleList { get; set; }

        public UserData() { }

        public UserData(UserDataOld userData,
            bool extendedSorting, bool automaticallyCheckForUpdates, bool unblockFiles,
            bool fixCommonIssues, bool compactModuleList, bool resetModuleList)
        {
            GameType = userData.GameType;
            SingleplayerData = userData.SingleplayerData;
            MultiplayerData = userData.MultiplayerData;
            ExtendedSorting = extendedSorting;
            AutomaticallyCheckForUpdates = automaticallyCheckForUpdates;
            UnblockFiles = unblockFiles;
            FixCommonIssues = fixCommonIssues;
            CompactModuleList = compactModuleList;
            ResetModuleList = resetModuleList;
        }
    }
}