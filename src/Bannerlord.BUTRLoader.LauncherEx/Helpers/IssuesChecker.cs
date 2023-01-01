using Bannerlord.BUTRLoader.Localization;

using Ookii.Dialogs.WinForms;

using System;
using System.IO;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class IssuesChecker
    {
        public static void CheckForRootHarmony()
        {
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrWhiteSpace(folder)) return;

            var harmonyFile = Path.Combine(folder!, "0Harmony.dll");
            var harmonyBakFile = Path.Combine(folder!, "0Harmony.dll.bak");
            if (!File.Exists(harmonyFile)) return;

            using var okButton = new TaskDialogButton(ButtonType.Yes);
            using var cancelButton = new TaskDialogButton(ButtonType.No);
            using var dialog = new TaskDialog
            {
                MainIcon = TaskDialogIcon.Warning,
                WindowTitle = new BUTRTextObject("{=dDprK7Mz}WARNING!").ToString(),
                Content = new BUTRTextObject("{=tqjPGPtP}BUTRLoader detected 0Harmony.dll inside the game's root bin folder!{NL}This could lead to issues, remove it?").ToString(),
                Buttons = { okButton, cancelButton },
                CenterParent = true,
                AllowDialogCancellation = true,
            };

            if (dialog.ShowDialog() != okButton) return;

            if (File.Exists(harmonyBakFile))
                File.Delete(harmonyBakFile);
            File.Move(harmonyFile, harmonyBakFile);
        }
    }
}