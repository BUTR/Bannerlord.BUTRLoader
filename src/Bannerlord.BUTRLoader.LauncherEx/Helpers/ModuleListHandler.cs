using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Patches.Mixins;

using HarmonyLib.BUTR.Extensions;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using TaleWorlds.MountAndBlade.Launcher.Library;

using ApplicationVersion = Bannerlord.ModuleManager.ApplicationVersion;
using BinaryReader = System.IO.BinaryReader;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal class ModuleListHandler
    {
        private record SaveMetadata([property: JsonProperty("List")] Dictionary<string, string> List);
        private record ModuleListEntry(string Id, ApplicationVersion Version);
        private record ModuleMismatch(string Id, ApplicationVersion OriginalVersion, ApplicationVersion CurrentVersion)
        {
            public override string ToString() => $"{Id} - Expected: {OriginalVersion}, Installed: {CurrentVersion}";
        }

        private delegate void UpdateAndSaveUserModsDataDelegate(LauncherVM instance, bool isMultiplayer);
        private static readonly UpdateAndSaveUserModsDataDelegate? UpdateAndSaveUserModsDataMethod =
            AccessTools2.GetDelegate<UpdateAndSaveUserModsDataDelegate>(typeof(LauncherVM), "UpdateAndSaveUserModsData");

        private static readonly int DefaultChangeSet = typeof(TaleWorlds.Library.ApplicationVersion).GetField("DefaultChangeSet")?.GetValue(null) as int? ?? 0;

        private static string Serialize(IEnumerable<ModuleListEntry> modules)
        {
            var sb = new StringBuilder();
            foreach (var (id, version) in modules)
            {
                sb.AppendLine($"{id}: {version}");
            }
            return sb.ToString();
        }

        private readonly LauncherVM _launcherVM;

        public ModuleListHandler(LauncherVM launcherVM)
        {
            _launcherVM = launcherVM;
        }

        private static ModuleInfoExtendedWithMetadata[] ReadImportList(Stream stream, LauncherModsVMMixin mixin)
        {
            static IEnumerable<string> ReadAllLines(TextReader reader)
            {
                while (reader.ReadLine() is { } line)
                {
                    yield return line;
                }
            }
            static IEnumerable<ModuleListEntry> Deserialize(IEnumerable<string> content)
            {
                var list = new List<ModuleListEntry>();
                foreach (var line in content)
                {
                    if (line.Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries) is { Length: 2 } split)
                    {
                        var version = ApplicationVersion.TryParse(split[1], out var versionVar) ? versionVar : ApplicationVersion.Empty;
                        list.Add(new ModuleListEntry(split[0], version));
                    }
                }
                var nativeChangeset = list.Find(x => x.Id == "Native").Version.ChangeSet is var x and not 0 ? x : DefaultChangeSet;
                foreach (var entry in list)
                {
                    var version = entry.Version;
                    yield return version.ChangeSet == nativeChangeset
                        ? entry with { Version = new ApplicationVersion(version.ApplicationVersionType, version.Major, version.Minor, version.Revision, 0) }
                        : entry;
                }
            }

            using var reader = new StreamReader(stream);
            var importedModules = Deserialize(ReadAllLines(reader)).ToArray();

            var importedModuleIds = importedModules.Select(x => x.Id).ToHashSet();
            var currentModuleIds = mixin.Modules2.Select(x => x.ModuleInfoExtended.Id).ToHashSet();
            var mismatchedModuleIds = importedModuleIds.Except(currentModuleIds).ToList();
            if (mismatchedModuleIds.Count > 0)
            {
                HintManager.ShowHint(@$"Cancelled Import!

Missing modules:
{string.Join(Environment.NewLine, mismatchedModuleIds)}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            var mismatchedVersions = new List<ModuleMismatch>();
            foreach (var (id, version) in importedModules)
            {
                if (!mixin.Modules2Lookup.TryGetValue(id, out var moduleVM)) continue;

                var launcherModuleVersion = moduleVM.ModuleInfoExtended.Version;
                if (launcherModuleVersion != version)
                    mismatchedVersions.Add(new ModuleMismatch(id, version, launcherModuleVersion));
            }
            if (mismatchedVersions.Count > 0)
            {
                HintManager.ShowHint(@$"Cancelled Import!

Mismatched module versions:
{string.Join(Environment.NewLine, mismatchedVersions)}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            return importedModules.Select(x => mixin.Modules2Lookup[x.Id]).Select(x => x.ModuleInfoExtended).ToArray();
        }
        private static ModuleInfoExtendedWithMetadata[] ReadSaveFile(Stream stream, LauncherModsVMMixin mixin)
        {
            static string[] GetModules(SaveMetadata metadata)
            {
                if (!metadata.List.TryGetValue("Modules", out var text))
                {
                    return Array.Empty<string>();
                }
                return text.Split(';');
            }
            static ApplicationVersion GetModuleVersion(SaveMetadata metadata, string moduleName)
            {
                if (metadata.List.TryGetValue($"Module_{moduleName}", out var versionRaw))
                {
                    return ApplicationVersion.TryParse(versionRaw, out var versionVar) ? versionVar : ApplicationVersion.Empty;
                }
                return ApplicationVersion.Empty;
            }

            SaveMetadata? metadata;
            try
            {
                using var reader = new BinaryReader(stream);
                var utf8Length = reader.ReadInt32();
                var utf8Bytes = reader.ReadBytes(utf8Length);
                metadata = JsonConvert.DeserializeObject<SaveMetadata?>(Encoding.UTF8.GetString(utf8Bytes));
            }
            catch
            {
                metadata = null;
            }

            if (metadata is null)
            {
                HintManager.ShowHint(@$"Cancelled Import!

Failed to read the save file!");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            var nativeChangeset = GetModuleVersion(metadata, "Native").ChangeSet;
            var importedModules = GetModules(metadata).Select(x =>
            {
                var version = GetModuleVersion(metadata, x);
                if (version.ChangeSet == nativeChangeset)
                    version = new ApplicationVersion(version.ApplicationVersionType, version.Major, version.Minor, version.Revision, 0);
                return new ModuleListEntry(x, version);
            }).ToArray();

            var importedModuleNames = importedModules.Select(x => x.Id).ToHashSet();
            var currentModuleNames = mixin.Modules2.Select(x => x.ModuleInfoExtended.Name).ToHashSet();
            var mismatchedModuleNames = importedModuleNames.Except(currentModuleNames).ToList();
            if (mismatchedModuleNames.Count > 0)
            {
                HintManager.ShowHint(@$"Cancelled Import!

Missing modules:
{string.Join(Environment.NewLine, mismatchedModuleNames)}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            /*
            var mismatchedVersions = new List<ModuleMismatch>();
            foreach (var (name, version) in importedModules)
            {
                if (mixin.Modules2.FirstOrDefault(x => x.Name == name) is not { } moduleVM) continue;

                var launcherModuleVersion = moduleVM.ModuleInfoExtended.Version;
                if (launcherModuleVersion != version)
                    mismatchedVersions.Add(new ModuleMismatch(name, version, launcherModuleVersion));
            }
            if (mismatchedVersions.Count > 0)
            {
                HintManager.ShowHint(@$"Cancelled Import!

Mismatched module versions:
{string.Join(Environment.NewLine, mismatchedVersions)}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }
            */

            return importedModules.Select(x => mixin.Modules2.First(y => y.Name == x.Id)).Select(x => x.ModuleInfoExtended).ToArray();
        }

        public void Import()
        {
            var thread = new Thread(() =>
            {
                if (_launcherVM.ModsData.GetMixin<LauncherModsVMMixin, LauncherModsVM>() is not { } mixin)
                {
                    HintManager.ShowHint(@$"Cancelled Import!

Internal BUTRLoader error: GetMixin() null");
                    return;
                }
                if (UpdateAndSaveUserModsDataMethod is null)
                {
                    HintManager.ShowHint(@$"Cancelled Import!

Internal BUTRLoader error: UpdateAndSaveUserModsDataMethod null");
                    return;
                }

                var dialog = new OpenFileDialog
                {
                    FileName = "MyList.bmlist",
                    Filter = "Bannerlord Module List (*.bmlist)|*.bmlist|Bannerlord Save File (*.sav)|*.sav",
                    Title = "Open a Bannerlord Module List File",

                    CheckFileExists = true,
                    CheckPathExists = true,
                    ReadOnlyChecked = true,
                    Multiselect = false,
                    ValidateNames = true
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using var fs = dialog.OpenFile();
                        var modules = Path.GetExtension(dialog.FileName) switch
                        {
                            ".bmlist" => ReadImportList(fs, mixin),
                            ".sav" => ReadSaveFile(fs, mixin),
                            _ => Array.Empty<ModuleInfoExtendedWithMetadata>()
                        };
                        if (modules.Length == 0)
                            return;
                        
                        var loadOrderValidationIssues = LauncherModsVMMixin.IsLoadOrderCorrect(modules).ToList();
                        if (loadOrderValidationIssues.Count != 0)
                        {
                            HintManager.ShowHint(@$"Cancelled Import!

Load Order is not correct! Reason:
{string.Join(Environment.NewLine, loadOrderValidationIssues.Select(x => x.Reason))}");
                            return;
                        }

                        // Deselect all
                        foreach (var moduleVM in mixin.Modules2)
                        {
                            if (moduleVM.IsSelected)
                                moduleVM.ExecuteSelect();
                        }
                        
                        // Select all from load order
                        foreach (var module in modules)
                        {
                            if (mixin.Modules2Lookup[module.Id] is { IsSelected: false } moduleVM)
                                moduleVM.ExecuteSelect();
                        }

                        mixin.SortByDefault();

                        var orderIssues = mixin.OrderBy(modules.Select(x => x.Id).ToList()).ToList();
                        if (orderIssues.Count != 0)
                        {
                            HintManager.ShowHint(@$"Cancelled Import!

Load Order is not correct! Reason:
{string.Join(Environment.NewLine, orderIssues)}");
                            return;
                        }

                        UpdateAndSaveUserModsDataMethod(_launcherVM, false);
                        HintManager.ShowHint("Successfully imported list!");
                    }
                    catch (Exception) { }
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        public void Export()
        {
            var thread = new Thread(() =>
            {
                if (_launcherVM.ModsData.GetMixin<LauncherModsVMMixin, LauncherModsVM>() is not { } mixin)
                {
                    HintManager.ShowHint(@$"Cancelled Export!

Internal BUTRLoader error: GetMixin() null");
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    FileName = "MyList.bmlist",
                    Filter = "Bannerlord Module List (*.bmlist)|*.bmlist",
                    Title = "Save a Bannerlord Module List File",

                    CheckFileExists = false,
                    CheckPathExists = false,

                    ValidateNames = true,
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var modules = mixin.Modules2
                            .Where(x => x.IsSelected)
                            .Select(x => x.ModuleInfoExtended)
                            .Select(x => new ModuleListEntry(x.Id, x.Version))
                            .ToArray();

                        using var fs = dialog.OpenFile();
                        using var writer = new StreamWriter(fs);
                        var content = Serialize(modules);
                        writer.Write(content);
                    }
                    catch (Exception) { }
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}