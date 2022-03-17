// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Options
{
    public sealed class LauncherExData
    {
        public bool ExtendedSorting { get; set; } = true;
        public bool AutomaticallyCheckForUpdates { get; set; }
        public bool UnblockFiles { get; set; } = true;
        public bool FixCommonIssues { get; set; }
        public bool CompactModuleList { get; set; }
        public bool ResetModuleList { get; set; }

        public LauncherExData() { }
        public LauncherExData(bool extendedSorting, bool automaticallyCheckForUpdates, bool unblockFiles, bool fixCommonIssues, bool compactModuleList, bool resetModuleList)
        {
            ExtendedSorting = extendedSorting;
            AutomaticallyCheckForUpdates = automaticallyCheckForUpdates;
            UnblockFiles = unblockFiles;
            FixCommonIssues = fixCommonIssues;
            CompactModuleList = compactModuleList;
            ResetModuleList = resetModuleList;
        }
    }
}