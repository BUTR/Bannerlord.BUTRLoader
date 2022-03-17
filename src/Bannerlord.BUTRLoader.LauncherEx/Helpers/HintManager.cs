using HarmonyLib.BUTR.Extensions;

using System;
using System.Windows.Forms;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Helpers;

internal static class HintManager
{
    private const int WM_CLOSE = 0x0010;
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

    private static readonly string _caption = "Message!";

    private delegate void AddHintInformationDelegate(string message);
    private static readonly AddHintInformationDelegate? AddHintInformation =
        AccessTools2.GetDelegate<AddHintInformationDelegate>(LauncherUIWrapper.LauncherUIType!, "AddHintInformation");

    private delegate void HideHintInformationDelegate();
    private static readonly HideHintInformationDelegate? HideHintInformation =
        AccessTools2.GetDelegate<HideHintInformationDelegate>(LauncherUIWrapper.LauncherUIType!, "HideHintInformation");


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