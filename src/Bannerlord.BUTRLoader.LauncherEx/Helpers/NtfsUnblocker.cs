using System.IO;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Helpers
{
    /// <summary>
    /// https://stackoverflow.com/a/21266072
    /// </summary>
    internal static class NtfsUnblocker
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFile(string name);

        public static void UnblockPath(string path, string wildcard = "*")
        {
            foreach (string file in Directory.GetFiles(path, wildcard))
                UnblockFile(file);

            foreach (string dir in Directory.GetDirectories(path))
                UnblockPath(dir);
        }

        public static bool UnblockFile(string fileName) => DeleteFile(fileName + ":Zone.Identifier");
    }
}