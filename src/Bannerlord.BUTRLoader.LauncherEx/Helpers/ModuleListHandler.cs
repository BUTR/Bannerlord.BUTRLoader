﻿using Bannerlord.BUTRLoader.Patches;

using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal class ModuleListHandler
    {
        private record ModuleListEntry(string Id, string Version);
        private record ModuleMismatch(string Id, string OriginalVersion, string CurrentVersion)
        {
            public override string ToString() => $"{Id} - Expected: {OriginalVersion}, Installed: {CurrentVersion}";
        }

        private delegate void UpdateAndSaveUserModsDataDelegate(LauncherVM instance, bool isMultiplayer);
        private static readonly UpdateAndSaveUserModsDataDelegate? UpdateAndSaveUserModsDataMethod =
            AccessTools2.GetDelegate<UpdateAndSaveUserModsDataDelegate>(typeof(LauncherVM), "UpdateAndSaveUserModsData");

        private static readonly int DefaultChangeSet = typeof(ApplicationVersion).GetField("DefaultChangeSet")?.GetValue(null) as int? ?? 0;

        private static string ToString(ApplicationVersion version)
        {
            var str = version.ToString();
            if (version.ChangeSet == DefaultChangeSet)
            {
                var idx = str.LastIndexOf('.');
                return str.Substring(0, idx);
            }
            return str;
        }

        private static string Serialize(IEnumerable<ModuleListEntry> modules)
        {
            var sb = new StringBuilder();
            foreach (var (id, version) in modules)
            {
                sb.AppendLine($"{id}: {version}");
            }
            return sb.ToString();
        }
        private static IEnumerable<ModuleListEntry> Deserialize(IEnumerable<string> content)
        {
            foreach (var line in content)
            {
                if (line.Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries) is { Length: 2 } split)
                {
                    yield return new ModuleListEntry(split[0], split[1]);
                }
            }
        }

        private readonly LauncherVM _launcherVM;
        private readonly UserDataManager _userDataManager;

        public ModuleListHandler(LauncherVM launcherVM, UserDataManager userDataManager)
        {
            _launcherVM = launcherVM;
            _userDataManager = userDataManager;
        }

        public void Import()
        {
            static IEnumerable<string> ReadAllLines(TextReader reader)
            {
                while (reader.ReadLine() is { } line)
                {
                    yield return line;
                }
            }

            var thread = new Thread(() =>
            {
                var dialog = new OpenFileDialog
                {
                    FileName = "MyList.bmlist",
                    Filter = "Bannerlord Module List (*.bmlist)|*.bmlist",
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
                        using var reader = new StreamReader(fs);
                        var modules = Deserialize(ReadAllLines(reader)).ToArray();
                        var moduleIds = modules.Select(x => x.Id).ToHashSet();
                        var wrappedModules = _launcherVM.ModsData.Modules
                            .Where(x => moduleIds.Contains(x.Info.Id))
                            .ToArray();
                        var wrappedModuleIds = wrappedModules.Select(x => x.Info?.Id).ToHashSet();
                        if (modules.Length != wrappedModules.Length)
                        {
                            var missingModules = moduleIds.Except(wrappedModuleIds);
                            HintManager.ShowHint(@$"Cancelled Import!

Missing modules:
{string.Join(Environment.NewLine, missingModules)}");
                            return;
                        }

                        foreach (var launcherModuleVM in _launcherVM.ModsData.Modules)
                        {
                            launcherModuleVM.IsSelected = false;
                        }
                        var mismatchedVersions = new List<ModuleMismatch>();
                        foreach (var (id, version) in modules)
                        {
                            var module = _launcherVM.ModsData.Modules
                                .FirstOrDefault(x => x.Info.Id == id);
                            if (module is not null)
                            {
                                var launcherModuleVersion = ToString(module.Info.Version);
                                if (launcherModuleVersion == version)
                                {
                                    module.IsSelected = true;
                                    continue;
                                }
                                else
                                {
                                    mismatchedVersions.Add(new ModuleMismatch(id, version, launcherModuleVersion));
                                }
                            }
                        }
                        if (mismatchedVersions.Count > 0)
                        {
                            HintManager.ShowHint(@$"Warning!

Mismatched module versions:
{string.Join(Environment.NewLine, mismatchedVersions)}");
                        }
                        else
                        {
                            HintManager.ShowHint("Successfully imported list!");
                        }

                        UpdateAndSaveUserModsDataMethod?.Invoke(_launcherVM, false);
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
                        var moduleIds = _userDataManager.UserData?.SingleplayerData?.ModDatas
                            .Where(x => x.IsSelected)
                            .Select(x => x.Id)
                            .ToHashSet();
                        var modules = _launcherVM.ModsData.Modules
                            .Select(x => x.Info)
                            .Where(x => moduleIds.Contains(x.Id))
                            .Select(x => new ModuleListEntry(x!.Id, ToString(x.Version)))
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