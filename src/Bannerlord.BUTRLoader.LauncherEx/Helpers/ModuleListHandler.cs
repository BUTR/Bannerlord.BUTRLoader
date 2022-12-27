﻿using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Mixins;

using HarmonyLib.BUTR.Extensions;

using Newtonsoft.Json;

using Ookii.Dialogs.WinForms;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.SaveSystem;

using ApplicationVersion = Bannerlord.ModuleManager.ApplicationVersion;

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

            using var reader = new StreamReader(stream);
            var importedModules = Deserialize(ReadAllLines(reader)).ToArray();

            var importedModuleIds = importedModules.Select(x => x.Id).ToHashSet();
            var currentModuleIds = mixin.Modules2.Select(x => x.ModuleInfoExtended.Id).ToHashSet();
            var mismatchedModuleIds = importedModuleIds.Except(currentModuleIds).ToList();
            if (mismatchedModuleIds.Count > 0)
            {
                HintManager.ShowHint($"Cancelled Import!\n\nMissing modules:\n{string.Join("\n", mismatchedModuleIds)}");
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
            if (mismatchedVersions.Count > 0 && !ShowVersionWarning(mismatchedVersions.Select(x => x.ToString())))
                return Array.Empty<ModuleInfoExtendedWithMetadata>();

            return importedModules.Select(x => mixin.Modules2Lookup[x.Id]).Select(x => x.ModuleInfoExtended).ToArray();
        }
        private static ModuleInfoExtendedWithMetadata[] ReadSaveFile(Stream stream, LauncherModsVMMixin mixin)
        {
            if (MetaData.Deserialize(stream) is not { } metadata)
            {
                HintManager.ShowHint(@"Cancelled Import!

Failed to read the save file!");
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
                HintManager.ShowHint($"Cancelled Import!\n\nMissing modules:\n{string.Join("\n", mismatchedModuleNames)}");
                return Array.Empty<ModuleInfoExtendedWithMetadata>();
            }

            var mismatchedVersions = new List<ModuleMismatch>();
            foreach (var (name, version) in importedModules)
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
                HintManager.ShowHint($"Cancelled Import!\n\nMissing modules:\n{string.Join("\n", mismatchedModuleIds)}");
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
            if (mismatchedVersions.Count > 0 && !ShowVersionWarning(mismatchedVersions.Select(x => x.ToString())))
                return Array.Empty<ModuleInfoExtendedWithMetadata>();

            return importedModules.Select(x => mixin.Modules2Lookup[x.Id]).Select(x => x.ModuleInfoExtended).ToArray();
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


                var dialog = new VistaOpenFileDialog
                {
                    Filter = "Bannerlord Module List (*.bmlist)|*.bmlist|Bannerlord Save File (*.sav)|*.sav|Novus Preset (*.xml)|*.xml|All files (*.*)|*.*",
                    Title = "Open a File with a Load Order",

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
                        if (modules.Length == 0)
                            return;

                        var loadOrderValidationIssues = LoadOrderChecker.IsLoadOrderCorrect(modules).ToList();
                        if (loadOrderValidationIssues.Count != 0)
                        {
                            HintManager.ShowHint($"Cancelled Import!\n\nLoad Order is not correct! Reason:\n{string.Join("\n", loadOrderValidationIssues.Select(x => x.Reason))}");
                            return;
                        }

                        var moduleIds = modules.Select(x => x.Id).ToHashSet();
                        var orderIssues = mixin.TryOrderByLoadOrder(modules.Select(x => x.Id), x => moduleIds.Contains(x)).ToList();
                        if (orderIssues.Count != 0)
                        {
                            HintManager.ShowHint($"Cancelled Import!\n\nLoad Order is not correct! Reason:\n{string.Join("\n", orderIssues)}");
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

        private void SaveBMList(Stream stream, IReadOnlyList<ModuleInfoExtendedWithMetadata> modules)
        {
            using var writer = new StreamWriter(stream);
            var content = Serialize(modules.Select(x => new ModuleListEntry(x.Id, x.Version)).ToArray());
            writer.Write(content);
        }
        private void SaveNovusPreset(Stream stream, IReadOnlyList<ModuleInfoExtendedWithMetadata> modules)
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
            var thread = new Thread(() =>
            {
                if (_launcherVM.ModsData.GetMixin<LauncherModsVMMixin, LauncherModsVM>() is not { } mixin)
                {
                    HintManager.ShowHint(@$"Cancelled Export!

Internal BUTRLoader error: GetMixin() null");
                    return;
                }

                var dialog = new VistaSaveFileDialog
                {
                    FileName = "MyList.bmlist",
                    Filter = "Bannerlord Module List (*.bmlist)|*.bmlist|Novus Preset (*.xml)|*.xml",
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
                            .ToArray();

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
                    catch (Exception) { }
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}