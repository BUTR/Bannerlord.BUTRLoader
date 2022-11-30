using System.IO;
using System.Runtime.InteropServices;

namespace Bannerlord.BUTRLoader.Helpers
{
    /// <summary>
    /// https://stackoverflow.com/a/21266072
    /// </summary>
    internal static class NtfsUnblocker
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        public static void UnblockPath(string path, string wildcard = "*")
        {
            foreach (var file in Directory.GetFiles(path, wildcard))
                UnblockFile(file);

            foreach (var dir in Directory.GetDirectories(path))
                UnblockPath(dir);
        }

        public static bool UnblockFile(string fileName) => DeleteFile($"{fileName}:Zone.Identifier");
    }
}