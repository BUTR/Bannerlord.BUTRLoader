using System;

namespace Bannerlord.BUTRLoader
{
    internal sealed class BUTRLoaderAppDomainManager : AppDomainManager
    {
        public static bool ExtendedSorting
        {
            get => _extendedSorting;
            set => _extendedSorting = value;
        }
        private static bool _extendedSorting = true;

        public static bool AutomaticallyCheckForUpdates
        {
            get => _automaticallyCheckForUpdates;
            set => _automaticallyCheckForUpdates = value;
        }
        private static bool _automaticallyCheckForUpdates;

        public static bool UnblockFiles
        {
            get => _unblockFiles;
            set => _unblockFiles = value;
        }
        private static bool _unblockFiles;

        public static bool FixCommonIssues
        {
            get => _fixCommonIssues;
            set => _fixCommonIssues = value;
        }
        private static bool _fixCommonIssues;
    }
}