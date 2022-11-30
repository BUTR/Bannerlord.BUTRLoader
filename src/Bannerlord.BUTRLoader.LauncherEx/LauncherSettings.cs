﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Bannerlord.BUTRLoader.Tests")]

namespace Bannerlord.BUTRLoader
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal sealed class LauncherSettings
    {
        public static bool ExtendedSorting { get; set; } = true;
        public static bool AutomaticallyCheckForUpdates { get; set; }
        public static bool UnblockFiles { get; set; }
        public static bool FixCommonIssues { get; set; }
        public static bool CompactModuleList { get; set; }
        public static bool ResetModuleList { get; set; }
        public static bool DisableBinaryCheck { get; set; }
    }
}