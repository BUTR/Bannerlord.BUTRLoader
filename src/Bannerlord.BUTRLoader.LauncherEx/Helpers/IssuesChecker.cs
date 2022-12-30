using Bannerlord.BUTRLoader.Localization;

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class IssuesChecker
    {
        public static void CheckForRootHarmony()
        {
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrWhiteSpace(folder))
            {
                var harmonyFile = Path.Combine(folder!, "0Harmony.dll");
                var harmonyBakFile = Path.Combine(folder!, "0Harmony.dll.bak");
                if (File.Exists(harmonyFile))
                {
                    var thread = new Thread(() =>
                    {
                        var result = MessageBox.Show(
                            new BUTRTextObject("{=tqjPGPtP}BUTRLoader detected 0Harmony.dll inside the game's root bin folder!{NL}This could lead to issues, remove it?").ToString(),
                            new BUTRTextObject("{=dDprK7Mz}WARNING!").ToString(),
                            MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                        {
                            if (File.Exists(harmonyBakFile))
                                File.Delete(harmonyBakFile);
                            File.Move(harmonyFile, harmonyBakFile);
                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                }
            }
        }
    }
}