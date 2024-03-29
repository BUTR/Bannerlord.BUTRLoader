﻿using System.Runtime.InteropServices;

using Windows.Win32;

namespace Bannerlord.BUTRLoader.AssemblyManager;

internal static class NtfsUnblocker
{
    public static void UnblockFile(string fileName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            PInvoke.DeleteFile($"{fileName}:Zone.Identifier");
    }
}