using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Localization;
using Bannerlord.BUTRLoader.Mixins;

using HarmonyLib.BUTR.Extensions;

using Ookii.Dialogs.WinForms;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.SaveSystem;

using ApplicationVersion = Bannerlord.ModuleManager.ApplicationVersion;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal class ModuleListHandler
    {
        private record ModuleListEntry(string Id, ApplicationVersion Version, string? Url = null);
        private record ModuleMismatch(string Id, ApplicationVersion OriginalVersion, ApplicationVersion CurrentVersion)
        {
            public override string ToString() => $"{Id} - Expected: {OriginalVersion}, Installed: {CurrentVersion}";
        }

        private delegate void UpdateAndSaveUserModsDataDelegate(LauncherVM instance, bool isMultiplayer);
        private static readonly UpdateAndSaveUserModsDataDelegate? UpdateAndSaveUserModsDataMethod =
            AccessTools2.GetDelegate<UpdateAndSaveUserModsDataDelegate>(typeof(LauncherVM), "UpdateAndSaveUserModsData");

        private static readonly int DefaultChangeSet = typeof(TaleWorlds.Library.ApplicationVersion).GetField("DefaultChangeSet")?.GetValue(null) as int? ?? 0;

        private readonly LauncherVM _launcherVM;

        public ModuleListHandler(LauncherVM launcherVM)
        {
            _launcherVM = launcherVM;
        }

        private static bool ShowVersionWarning(IEnumerable<string> mismatchedVersions)
        {
            using var okButton = new TaskDialogButton(ButtonType.Yes);
            using var cancelButton = new TaskDialogButton(ButtonType.No);
            using var dialog = new TaskDialog
            {
                MainIcon = TaskDialogIcon.Warning,
                WindowTitle = "Import Warning",
                MainInstruction = "Mismatched module versions:",
                Content = $"{string.Join(Environment.NewLine, mismatchedVersions)}{Environment.NewLine}{Environment.NewLine}Continue import?",
                Buttons = { okButton, cancelButton },
                CenterParent = true,
                AllowDialogCancellation = true,
            };
            return dialog.ShowDialog() == okButton;
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

            var idDuplicates = mixin.Modules2.Select(x => x.ModuleInfoExtended.Id).GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (idDuplicates.Count > 0)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\n{new BUTRTextObject("{=izSm5f85}Duplicate Module Ids:{NL}{MODULEIDS}").SetTextVariable("MODULEIDS", string.Join("\n", idDuplicates))}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            using var reader = new StreamReader(stream);
            var importedModules = Deserialize(ReadAllLines(reader)).ToArray();

            var importedModuleIds = importedModules.Select(x => x.Id).ToHashSet();
            var currentModuleIds = mixin.Modules2.Select(x => x.ModuleInfoExtended.Id).ToHashSet();
            var mismatchedModuleIds = importedModuleIds.Except(currentModuleIds).ToList();
            if (mismatchedModuleIds.Count > 0)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\n{new BUTRTextObject("{=GtDRbC3m}Missing Modules:{NL}{MODULES}").SetTextVariable("MODULES", string.Join("\n", mismatchedModuleIds))}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            var mismatchedVersions = new List<ModuleMismatch>();
            foreach (var (id, version, _) in importedModules)
            {
                if (!mixin.Modules2Lookup.TryGetValue(id, out var moduleVM)) continue;

                var launcherModuleVersion = moduleVM.ModuleInfoExtended.Version;
                if (launcherModuleVersion != version)
                    mismatchedVersions.Add(new ModuleMismatch(id, version, launcherModuleVersion));
            }
            if (mismatchedVersions.Count > 0 && !ShowVersionWarning(mismatchedVersions.Select(x => x.ToString())))
                return Array.Empty<ModuleInfoExtendedWithMetadata>();

            return importedModules.Select(x => mixin.Modules2Lookup[x.Id]).Select(x => x.ModuleInfoExtended).ToArray();
        }
        private static ModuleInfoExtendedWithMetadata[] ReadSaveFile(Stream stream, LauncherModsVMMixin mixin)
        {
            var nameDuplicates = mixin.Modules2.Select(x => x.ModuleInfoExtended.Name).GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (nameDuplicates.Count > 0)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\n{new BUTRTextObject("{=vCwH9226}Duplicate Module Names:{NL}{MODULENAMES}").SetTextVariable("{MODULENAMES}", string.Join("\n", nameDuplicates))}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            if (MetaData.Deserialize(stream) is not { } metadata)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\n{new BUTRTextObject("{=epU06HID}Failed to read the save file!")}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            var changeset = SaveHelper.GetChangeSet(metadata);
            var importedModules = SaveHelper.GetModules(metadata).Select(x =>
            {
                var version = SaveHelper.GetModuleVersion(metadata, x);
                if (version.ChangeSet == changeset)
                    version = new ApplicationVersion(version.ApplicationVersionType, version.Major, version.Minor, version.Revision, 0);
                return new ModuleListEntry(x, version);
            }).ToArray();

            var importedModuleNames = importedModules.Select(x => x.Id).ToHashSet();
            var currentModuleNames = mixin.Modules2.Select(x => x.ModuleInfoExtended.Name).ToHashSet();
            var mismatchedModuleNames = importedModuleNames.Except(currentModuleNames).ToList();
            if (mismatchedModuleNames.Count > 0)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\n{new BUTRTextObject("{=GtDRbC3m}issing Modules:{NL}{MODULENAMES}").SetTextVariable("MODULENAMES", string.Join("\n", mismatchedModuleNames))}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            var mismatchedVersions = new List<ModuleMismatch>();
            foreach (var (name, version, _) in importedModules)
            {
                if (mixin.Modules2.FirstOrDefault(x => x.Name == name) is not { } moduleVM) continue;

                var launcherModuleVersion = moduleVM.ModuleInfoExtended.Version;
                if (launcherModuleVersion != version)
                    mismatchedVersions.Add(new ModuleMismatch(name, version, launcherModuleVersion));
            }
            if (mismatchedVersions.Count > 0 && !ShowVersionWarning(mismatchedVersions.Select(x => x.ToString())))
                return Array.Empty<ModuleInfoExtendedWithMetadata>();

            return importedModules.Select(x => mixin.Modules2.First(y => y.Name == x.Id)).Select(x => x.ModuleInfoExtended).ToArray();
        }
        private static ModuleInfoExtendedWithMetadata[] ReadNovusPreset(Stream stream, LauncherModsVMMixin mixin)
        {
            var idDuplicates = mixin.Modules2.Select(x => x.ModuleInfoExtended.Id).GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (idDuplicates.Count > 0)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\n{new BUTRTextObject("{=izSm5f85}Duplicate Module Ids:{NL}{MODULEIDS}").SetTextVariable("MODULEIDS", string.Join("\n", idDuplicates))}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            var document = new XmlDocument();
            document.Load(stream);

            var importedModules = new List<ModuleListEntry>();
            foreach (var xmlNode in document.DocumentElement?.SelectNodes("PresetModule")?.OfType<XmlNode>() ?? Enumerable.Empty<XmlNode>())
            {
                if (xmlNode.NodeType == XmlNodeType.Comment)
                    continue;

                if (xmlNode.Attributes is null)
                    continue;

                if (xmlNode.Attributes["Id"] == null)
                    continue;

                var id = xmlNode.Attributes["Id"].InnerText;
                var version = xmlNode.Attributes["RequiredVersion"].InnerText;
                if (!string.IsNullOrEmpty(version) && char.IsNumber(version[0]))
                    version = $"v{version}";
                importedModules.Add(new ModuleListEntry(id, ApplicationVersion.TryParse(version, out var versionVar) ? versionVar : ApplicationVersion.Empty));

            }

            var importedModuleIds = importedModules.Select(x => x.Id).ToHashSet();
            var currentModuleIds = mixin.Modules2.Select(x => x.ModuleInfoExtended.Id).ToHashSet();
            var mismatchedModuleIds = importedModuleIds.Except(currentModuleIds).ToList();
            if (mismatchedModuleIds.Count > 0)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\n{new BUTRTextObject("{=GtDRbC3m}Missing Modules:{NL}{MODULES}").SetTextVariable("MODULES", string.Join("\n", mismatchedModuleIds))}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            var mismatchedVersions = new List<ModuleMismatch>();
            foreach (var (id, version, _) in importedModules)
            {
                if (!mixin.Modules2Lookup.TryGetValue(id, out var moduleVM)) continue;

                var launcherModuleVersion = moduleVM.ModuleInfoExtended.Version;
                if (launcherModuleVersion != version)
                    mismatchedVersions.Add(new ModuleMismatch(id, version, launcherModuleVersion));
            }
            if (mismatchedVersions.Count > 0 && !ShowVersionWarning(mismatchedVersions.Select(x => x.ToString())))
                return Array.Empty<ModuleInfoExtendedWithMetadata>();

            return importedModules.Select(x => mixin.Modules2Lookup[x.Id]).Select(x => x.ModuleInfoExtended).ToArray();
        }
        public void Import()
        {
            if (_launcherVM.ModsData.GetMixin<LauncherModsVMMixin, LauncherModsVM>() is not { } mixin)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\nInternal BUTRLoader error: GetMixin() null");
                return;
            }
            if (UpdateAndSaveUserModsDataMethod is null)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\nInternal BUTRLoader error: UpdateAndSaveUserModsDataMethod null");
                return;
            }

            var thread = new Thread(() =>
            {
                var dialog = new VistaOpenFileDialog
                {
                    Filter = "Bannerlord Module List (*.bmlist)|*.bmlist|Bannerlord Save File (*.sav)|*.sav|Novus Preset (*.xml)|*.xml|All files (*.*)|*.*",
                    Title = new BUTRTextObject("{=DKRNkst2}Open a File with a Load Order").ToString(),

                    CheckFileExists = true,
                    CheckPathExists = true,
                    ReadOnlyChecked = true,
                    Multiselect = false,
                    ValidateNames = true,
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
                            ".xml" => ReadNovusPreset(fs, mixin),
                            _ => Array.Empty<ModuleInfoExtendedWithMetadata>()
                        };
                        ImportInternal(modules, mixin);
                    }
                    catch (Exception e)
                    {
                        HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\nException:\n{e}");
                    }
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
        public void ImportSaveFile(string saveFile)
        {
            if (_launcherVM.ModsData.GetMixin<LauncherModsVMMixin, LauncherModsVM>() is not { } mixin)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\nInternal BUTRLoader error: GetMixin() null");
                return;
            }

            if (MBSaveLoad.GetSaveFileWithName(saveFile) is not { } si || SaveHelper.GetSaveFilePath(si) is not { } saveFilePath || !File.Exists(saveFilePath))
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\n{new BUTRTextObject("{=B64DbmWp}Save File not found!")}");
                return;
            }

            try
            {
                using var fs = File.OpenRead(saveFilePath);
                var modules = ReadSaveFile(fs, mixin);
                ImportInternal(modules, mixin);
            }
            catch (Exception e)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\nException:\n{e}");
            }
        }
        private void ImportInternal(ModuleInfoExtendedWithMetadata[] modules, LauncherModsVMMixin mixin)
        {
            if (modules.Length == 0)
                return;

            if (UpdateAndSaveUserModsDataMethod is null)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\nInternal BUTRLoader error: UpdateAndSaveUserModsDataMethod null");
                return;
            }

            var loadOrderValidationIssues = LoadOrderChecker.IsLoadOrderCorrect(modules).ToList();
            if (loadOrderValidationIssues.Count != 0)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\n{new BUTRTextObject("{=HvvA78sZ}Load Order Issues:{NL}{LOADORDERISSUES}").SetTextVariable("LOADORDERISSUES", string.Join("\n", loadOrderValidationIssues))}");
                return;
            }

            var moduleIds = modules.Select(x => x.Id).ToHashSet();
            var orderIssues = mixin.TryOrderByLoadOrder(modules.Select(x => x.Id), x => moduleIds.Contains(x)).ToList();
            if (orderIssues.Count != 0)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=WJnTxf3v}Cancelled Import!")}\n\n{new BUTRTextObject("{=HvvA78sZ}Load Order Issues:{NL}{LOADORDERISSUES}").SetTextVariable("LOADORDERISSUES", string.Join("\n", orderIssues))}");
                return;
            }

            UpdateAndSaveUserModsDataMethod(_launcherVM, false);
            HintManager.ShowHint(new BUTRTextObject("{=eohqbvHU}Successfully imported list!"));
        }

        private static ModuleListEntry[] ReadSaveFileModuleList(Stream stream, LauncherModsVMMixin mixin)
        {
            var nameDuplicates = mixin.Modules2.Select(x => x.ModuleInfoExtended.Name).GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (nameDuplicates.Count > 0)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=BjtJ4Lxw}Cancelled Export!")}\n\n{new BUTRTextObject("{=vCwH9226}Duplicate Module Names:{NL}{MODULENAMES}").SetTextVariable("MODULENAMES", string.Join("\n", nameDuplicates))}");
                return Array.Empty<ModuleListEntry>();
            }

            if (MetaData.Deserialize(stream) is not { } metadata)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=BjtJ4Lxw}Cancelled Export!")}\n\n{new BUTRTextObject("{=epU06HID}Failed to read the save file!")}");
                return Array.Empty<ModuleListEntry>();
            }

            var changeset = SaveHelper.GetChangeSet(metadata);
            var importedModules = SaveHelper.GetModules(metadata).Select(x =>
            {
                var version = SaveHelper.GetModuleVersion(metadata, x);
                if (version.ChangeSet == changeset)
                    version = new ApplicationVersion(version.ApplicationVersionType, version.Major, version.Minor, version.Revision, 0);
                return new ModuleListEntry(x, version);
            }).ToArray();

            var importedModuleNames = importedModules.Select(x => x.Id).ToHashSet();
            var currentModuleNames = mixin.Modules2.Select(x => x.ModuleInfoExtended.Name).ToHashSet();
            var mismatchedModuleNames = importedModuleNames.Except(currentModuleNames).ToList();
            if (mismatchedModuleNames.Count > 0)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=BjtJ4Lxw}Cancelled Export!")}\n\n{new BUTRTextObject("{=GtDRbC3m}Missing Modules:{NL}{MODULES}").SetTextVariable("MODULES", string.Join("\n", mismatchedModuleNames))}");
                return Array.Empty<ModuleListEntry>();
            }

            return importedModules
                .Select(x => mixin.Modules2.First(y => y.Name == x.Id))
                .Select(x => x.ModuleInfoExtended)
                .Select(x => new ModuleListEntry(x.Id, x.Version, x.Url))
                .ToArray();
        }
        private static void SaveBMList(Stream stream, IEnumerable<ModuleListEntry> modules)
        {
            static string Serialize(IEnumerable<ModuleListEntry> modules)
            {
                var sb = new StringBuilder();
                foreach (var (id, version, _) in modules)
                {
                    sb.AppendLine($"{id}: {version}");
                }
                return sb.ToString();
            }

            using var writer = new StreamWriter(stream);
            var content = Serialize(modules.Select(x => new ModuleListEntry(x.Id, x.Version)).ToArray());
            writer.Write(content);
        }
        private static void SaveNovusPreset(Stream stream, IEnumerable<ModuleListEntry> modules)
        {
            var document = new XmlDocument();

            var root = document.CreateElement("Preset");
            var nameAttribute = document.CreateAttribute("Name");
            nameAttribute.Value = "BUTRLoader Exported Load Order";
            var createdByAttribute = document.CreateAttribute("CreatedBy");
            createdByAttribute.Value = "BUTRLoader";
            var lastUpdatedAttribute = document.CreateAttribute("LastUpdated");
            lastUpdatedAttribute.Value = DateTime.Now.ToString("dd/MM/yyyy");
            root.Attributes.Append(nameAttribute);
            root.Attributes.Append(createdByAttribute);
            root.Attributes.Append(lastUpdatedAttribute);
            foreach (var module in modules)
            {
                var entryNode = document.CreateElement("PresetModule");

                var idAttribute = document.CreateAttribute("Id");
                idAttribute.Value = module.Id;
                var versionAttribute = document.CreateAttribute("RequiredVersion");
                versionAttribute.Value = module.Version.ToString();
                var urlAttribute = document.CreateAttribute("URL");
                urlAttribute.Value = module.Url;

                entryNode.Attributes.Append(idAttribute);
                entryNode.Attributes.Append(versionAttribute);
                entryNode.Attributes.Append(urlAttribute);

                root.AppendChild(entryNode);
            }
            document.AppendChild(root);

            using var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            });
            document.Save(writer);
        }
        public void Export()
        {
            if (_launcherVM.ModsData.GetMixin<LauncherModsVMMixin, LauncherModsVM>() is not { } mixin)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=BjtJ4Lxw}Cancelled Export!")}\n\nInternal BUTRLoader error: GetMixin() null");
                return;
            }

            var thread = new Thread(() =>
            {
                var dialog = new VistaSaveFileDialog
                {
                    FileName = "MyList.bmlist",
                    Filter = "Bannerlord Module List (*.bmlist)|*.bmlist|Novus Preset (*.xml)|*.xml",
                    Title = new BUTRTextObject("{=XSxlKweM}Save a Bannerlord Module List File").ToString(),

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
                            .Select(x => new ModuleListEntry(x.Id, x.Version, x.Url));

                        using var fs = dialog.OpenFile();
                        switch (Path.GetExtension(dialog.FileName))
                        {
                            case ".bmlist":
                                SaveBMList(fs, modules);
                                break;
                            case ".xml":
                                SaveNovusPreset(fs, modules);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        HintManager.ShowHint($"{new BUTRTextObject("{=BjtJ4Lxw}Cancelled Export!")}\n\nException:\n{e}");
                    }
                    HintManager.ShowHint(new BUTRTextObject("{=VwFQTk5z}Successfully exported list!"));
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
        public void ExportSaveFile(string saveFile)
        {
            if (_launcherVM.ModsData.GetMixin<LauncherModsVMMixin, LauncherModsVM>() is not { } mixin)
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=BjtJ4Lxw}Cancelled Export!")}\n\nInternal BUTRLoader error: GetMixin() null");
                return;
            }

            if (MBSaveLoad.GetSaveFileWithName(saveFile) is not { } si || SaveHelper.GetSaveFilePath(si) is not { } saveFilePath || !File.Exists(saveFilePath))
            {
                HintManager.ShowHint($"{new BUTRTextObject("{=BjtJ4Lxw}Cancelled Export!")}\n\n{new BUTRTextObject("{=B64DbmWp}Save File not found!")}");
                return;
            }

            using var fsSave = File.OpenRead(saveFilePath);
            var modules = ReadSaveFileModuleList(fsSave, mixin);
            if (modules.Length == 0)
                return;

            var thread = new Thread(() =>
            {
                var dialog = new VistaSaveFileDialog
                {
                    FileName = $"{saveFile}.bmlist",
                    Filter = "Bannerlord Module List (*.bmlist)|*.bmlist|Novus Preset (*.xml)|*.xml",
                    Title = new BUTRTextObject("{=XSxlKweM}Save a Bannerlord Module List File").ToString(),

                    CheckFileExists = false,
                    CheckPathExists = false,

                    ValidateNames = true,
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using var fs = dialog.OpenFile();
                        switch (Path.GetExtension(dialog.FileName))
                        {
                            case ".bmlist":
                                SaveBMList(fs, modules);
                                break;
                            case ".xml":
                                SaveNovusPreset(fs, modules);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        HintManager.ShowHint($"{new BUTRTextObject("{=BjtJ4Lxw}Cancelled Export!")}\n\nException:\n{e}");
                    }
                    HintManager.ShowHint(new BUTRTextObject("{=VwFQTk5z}Successfully exported list!"));
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}