﻿using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class HintManager
    {
        public static void ShowHint(string message)
        {
            LauncherUI.AddHintInformation(message);
        }

        public static void HideHint()
        {
            LauncherUI.HideHintInformation();
        }
    }
}