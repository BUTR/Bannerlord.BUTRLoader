﻿using UserDataOld = TaleWorlds.MountAndBlade.Launcher.UserDatas.UserData;

namespace Bannerlord.BUTRLoader.Options
{
    public class UserData : UserDataOld
    {
        public bool ExtendedSorting { get; set; } = true;
        public bool AutomaticallyCheckForUpdates { get; set; }
        public bool UnblockFiles { get; set; } = true;

        public UserData() { }

        public UserData(UserDataOld userData, bool extendedSorting, bool automaticallyCheckForUpdates, bool unblockFiles)
        {
            GameType = userData.GameType;
            SingleplayerData = userData.SingleplayerData;
            MultiplayerData = userData.MultiplayerData;
            ExtendedSorting = extendedSorting;
            AutomaticallyCheckForUpdates = automaticallyCheckForUpdates;
            UnblockFiles = unblockFiles;
        }
    }
}