using UserDataOld = TaleWorlds.MountAndBlade.Launcher.UserDatas.UserData;

namespace Bannerlord.BUTRLoader.Options
{
    public class UserData : UserDataOld
    {
        public bool ExtendedSorting { get; set; }
        public bool AutomaticallyCheckForUpdates { get; set; }

        public UserData() { }

        public UserData(UserDataOld userData, bool extendedSorting, bool automaticallyCheckForUpdates)
        {
            GameType = userData.GameType;
            SingleplayerData = userData.SingleplayerData;
            MultiplayerData = userData.MultiplayerData;
            ExtendedSorting = extendedSorting;
            AutomaticallyCheckForUpdates = automaticallyCheckForUpdates;
        }
    }
}