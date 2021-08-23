using System;
using System.IO;
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
                    var result = MessageBox.Show(
                        "BUTRLoader detected 0Harmony.dll inside the game's root bin folder!\nThis could lead to issues, remove it?",
                        "Warning!",
                        MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {
                        if (File.Exists(harmonyBakFile))
                            File.Delete(harmonyBakFile);
                        File.Move(harmonyFile, harmonyBakFile);
                    }
                }
            }
        }
    }
}