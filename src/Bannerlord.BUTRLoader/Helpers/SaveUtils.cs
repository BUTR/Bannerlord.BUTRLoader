#if CONTINUE

using System.IO;
using System.Linq;

using TaleWorlds.Core;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class SaveUtils
    {
        public static FileInfo? GetLatestSave()
        {
            var directory = new DirectoryInfo(FilePaths.SavePath);
            if (directory.Exists)
            {
                var lastEditedFile = directory.GetFiles("*.sav", SearchOption.AllDirectories).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                return lastEditedFile;
            }

            return null;
        }
    }
}
#endif