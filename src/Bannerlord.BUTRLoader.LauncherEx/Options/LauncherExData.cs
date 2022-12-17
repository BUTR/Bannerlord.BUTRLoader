namespace Bannerlord.BUTRLoader.Options
{
    public sealed class LauncherExData
    {
        public bool ExtendedSorting { get; set; } = true;
        public bool AutomaticallyCheckForUpdates { get; set; }
        public bool UnblockFiles { get; set; } = true;
        public bool FixCommonIssues { get; set; }
        public bool CompactModuleList { get; set; }
        public bool DisableBinaryCheck { get; set; }
        public bool HideRandomImage { get; set; }

        public LauncherExData() { }
        public LauncherExData(
            bool extendedSorting, bool automaticallyCheckForUpdates,
            bool unblockFiles, bool fixCommonIssues, bool compactModuleList,
            bool hideRandomImage, bool disableBinaryCheck)
        {
            ExtendedSorting = extendedSorting;
            AutomaticallyCheckForUpdates = automaticallyCheckForUpdates;
            UnblockFiles = unblockFiles;
            FixCommonIssues = fixCommonIssues;
            CompactModuleList = compactModuleList;
            DisableBinaryCheck = disableBinaryCheck;
            HideRandomImage = hideRandomImage;
        }
    }
}