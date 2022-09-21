using HarmonyLib.BUTR.Extensions;

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class HintManager
    {
        private const int WM_CLOSE = 0x0010;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private static readonly string _caption = "Message!";

        private delegate void AddHintInformationDelegate(string message);
        private static readonly AddHintInformationDelegate? AddHintInformation =
            AccessTools2.GetDeclaredDelegate<AddHintInformationDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherUI:AddHintInformation") ??
            AccessTools2.GetDeclaredDelegate<AddHintInformationDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherUI:AddHintInformation");

        private delegate void HideHintInformationDelegate();
        private static readonly HideHintInformationDelegate? HideHintInformation =
            AccessTools2.GetDeclaredDelegate<HideHintInformationDelegate>("TaleWorlds.MountAndBlade.Launcher.LauncherUI:HideHintInformation") ??
            AccessTools2.GetDeclaredDelegate<HideHintInformationDelegate>("TaleWorlds.MountAndBlade.Launcher.Library.LauncherUI:HideHintInformation");


        public static void ShowHint(string message)
        {
            if (AddHintInformation is not null)
            {
                AddHintInformation(message);
            }
            else
            {
                MessageBox.Show(message, _caption);
            }
        }

        public static void HideHint()
        {
            if (HideHintInformation is not null)
            {
                HideHintInformation();
            }
            else
            {
                var mbWnd = FindWindow(null, _caption);
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}