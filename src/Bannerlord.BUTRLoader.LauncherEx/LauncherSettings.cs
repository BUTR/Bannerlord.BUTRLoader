using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Bannerlord.BUTRLoader.Tests")]

namespace Bannerlord.BUTRLoader
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal sealed class LauncherSettings
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

        public static bool CompactModuleList
        {
            get => _compactModuleList;
            set => _compactModuleList = value;
        }
        private static bool _compactModuleList;

        public static bool ResetModuleList
        {
            get => _resetModuleList;
            set => _resetModuleList = value;
        }
        private static bool _resetModuleList;
    }
}