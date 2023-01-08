using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.ModuleManager;

using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;

namespace Bannerlord.BUTRLoader.Helpers
{
    public static class ReaderExtensions
    {
        public static IReader? OpenOrDefault(Stream stream, ReaderOptions options)
        {
            TryOpen(stream, options, out var archive);
            return archive;
        }

        public static bool TryOpen(Stream stream, ReaderOptions options, [NotNullWhen(true)] out IReader? archive)
        {
            try
            {
                archive = ReaderFactory.Open(stream, options);
                return true;
            }
            catch (Exception)
            {
                archive = null;
                return false;
            }
        }
    }

    public static class ArchiveExtensions
    {
        public static IArchive? OpenOrDefault(Stream stream, ReaderOptions options)
        {
            TryOpen(stream, options, out var archive);
            return archive;
        }

        public static bool TryOpen(Stream stream, ReaderOptions options, [NotNullWhen(true)] out IArchive? archive)
        {
            try
            {
                archive = ArchiveFactory.Open(stream, options);
                return true;
            }
            catch (Exception)
            {
                archive = null;
                return false;
            }
        }
    }

    public class ModuleInstaller
    {
        private static List<ModuleInfoExtendedWithMetadata> GetModules()
        {
            var list = new List<ModuleInfoExtendedWithMetadata>();
            var foundIds = new HashSet<string>();
            foreach (var moduleInfo in ModuleInfoHelper.GetPhysicalModules().Concat(ModuleInfoHelper.GetPlatformModules()))
            {
                if (!foundIds.Contains(moduleInfo.Id.ToLower()))
                {
                    foundIds.Add(moduleInfo.Id.ToLower());
                    list.Add(moduleInfo);
                }
            }
            return list;
        }

        public static void InstallArchive(string path)
        {
            if (!File.Exists(path)) return;

            using var fs = File.OpenRead(path);
            using var archive = ArchiveExtensions.OpenOrDefault(fs, new ReaderOptions { LeaveStreamOpen = true });
            if (archive is null) return;

            var installedModules = GetModules().ToDictionary(x => x.Id, x => x);

            ModuleInfoExtended? moduleInfo;
            IReader? reader;
            if (archive is { IsSolid: true, Type: ArchiveType.Rar })
            {
                fs.Seek(0, SeekOrigin.Begin);
                using var reader1 = ReaderExtensions.OpenOrDefault(fs, new ReaderOptions { LeaveStreamOpen = true });
                if (reader1 is null) return;
                if (GetSubModuleFile(archive) is not { } doc) return;
                moduleInfo = ModuleInfoExtended.FromXml(doc);
                if (moduleInfo is null) return;

                fs.Seek(0, SeekOrigin.Begin);
                using var reader2 = ReaderExtensions.OpenOrDefault(fs, new ReaderOptions { LeaveStreamOpen = true });
                if (reader2 is null) return;
                var hasRoot = HasRoot(reader2);

                fs.Seek(0, SeekOrigin.Begin);
                reader = ReaderExtensions.OpenOrDefault(fs, new ReaderOptions { LeaveStreamOpen = true });
                if (reader is null) return;
            }
            else
            {
                if (GetSubModuleFile(archive) is not { } doc) return;
                moduleInfo = ModuleInfoExtended.FromXml(doc);
                if (moduleInfo is null) return;
                reader = archive.ExtractAllEntries();
            }

            var installedModule = installedModules.TryGetValue(moduleInfo.Id, out var val) ? val : null;

            var modulesPath = Path.GetFullPath(Path.Combine(TaleWorlds.Library.BasePath.Name, ModuleInfoHelper.ModulesFolder));
            Install(moduleInfo, () =>
            {
                string BackupPath() => Path.GetFullPath(Path.Combine(modulesPath, "../", "ModulesBackup", Path.GetFileName(installedModule.Path)));

                try
                {
                    if (installedModule is not null)
                    {
                        var backup = BackupPath();
                        if (Directory.Exists(backup)) Directory.Delete(backup, true);
                        //Directory.CreateDirectory(backup);
                        Directory.Move(installedModule.Path, backup);
                    }

                    reader.MoveToNextEntry();
                    if (reader.Entry is {IsDirectory: true, Key: "Modules"})
                        reader.MoveToNextEntry();

                    reader.WriteAllToDirectory(modulesPath, new ExtractionOptions() { ExtractFullPath = true});

                    if (installedModule is not null)
                    {
                        var backup = BackupPath();
                        Directory.Delete(backup, true);
                    }
                }
                catch (Exception)
                {
                    if (installedModule is not null)
                    {
                        var backup = BackupPath();
                        if (Directory.Exists(backup))
                        {
                            Directory.Move(backup, installedModule.Path);
                        }
                    }
                }
                finally
                {
                    reader.Dispose();
                }
            });
        }

        private static void Install(ModuleInfoExtended moduleInfo, Action install)
        {
            var installed = ModuleInfoHelper.GetModules().ToDictionary(x => x.Id, x => x);
            if (installed.ContainsKey(moduleInfo.Id) && !ShowAlreadyExistsWarning(moduleInfo))
            {
                return;
            }
            install();
        }

        private static bool ShowAlreadyExistsWarning(ModuleInfoExtended moduleInfo)
        {
            /*
            var mismatched = new BUTRTextObject("{=BuMom4Jt}Mismatched Module Versions:{NL}{MODULEVERSIONS}")
                .SetTextVariable("NL", Environment.NewLine)
                .SetTextVariable("MODULEVERSIONS", string.Join(Environment.NewLine, mismatchedVersions)).ToString();
            var split = mismatched.Split('\n');
            using var okButton = new TaskDialogButton(ButtonType.Yes);
            using var cancelButton = new TaskDialogButton(ButtonType.No);
            using var dialog = new TaskDialog
            {
                MainIcon = TaskDialogIcon.Warning,
                WindowTitle = new BUTRTextObject("Import Warning").ToString(),
                MainInstruction = split[0],
                Content = $"{split[1]}{Environment.NewLine}{Environment.NewLine}{new BUTRTextObject("{=hgew15HH}Continue import?")}",
                Buttons = { okButton, cancelButton },
                CenterParent = true,
                AllowDialogCancellation = true,
            };
            return dialog.ShowDialog() == okButton;
            */
            return true;
        }


        private static bool HasRoot(IArchive archive) => archive.Entries.Any(x => x.IsDirectory && x.Key == "Modules");
        private static XmlDocument? GetSubModuleFile(IArchive archive)
        {
            if (archive.Entries.FirstOrDefault(x => !x.IsDirectory && Path.GetFileName(x.Key) == "SubModule.xml") is not { } entry) return null;
            using var stream = entry.OpenEntryStream();
            try
            {
                var doc = new XmlDocument();
                doc.Load(stream);
                return doc;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool HasRoot(IReader reader)
        {
            while (reader.MoveToNextEntry())
            {
                if (reader.Entry is { IsDirectory: true, Key: "Modules" })
                    return true;
            }
            return false;
        }
        private static XmlDocument? GetSubModuleFile(IReader reader)
        {
            while (reader.MoveToNextEntry())
            {
                if (reader.Entry is { IsDirectory: false } entry && Path.GetFileName(entry.Key) == "SubModule.xml")
                {
                    using var stream = reader.OpenEntryStream();
                    try
                    {
                        var doc = new XmlDocument();
                        doc.Load(stream);
                        return doc;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
            return null;
        }
    }
}