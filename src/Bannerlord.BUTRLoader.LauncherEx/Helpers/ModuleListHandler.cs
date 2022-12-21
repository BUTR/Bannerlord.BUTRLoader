using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.ModuleManager;

using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using TaleWorlds.MountAndBlade.Launcher.Library;

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

        private static readonly int DefaultChangeSet = typeof(TaleWorlds.Library.ApplicationVersion).GetField("DefaultChangeSet")?.GetValue(null) as int? ?? 0;

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

        public ModuleListHandler(LauncherVM launcherVM)
        {
            _launcherVM = launcherVM;
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
                if (_launcherVM.ModsData.GetModules() is not { } moduleVMs)
                {
                    HintManager.ShowHint(@$"Cancelled Import!

Internal BUTRLoader error: GetModules() null");
                    return;
                }

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
                        var wrappedModules = moduleVMs.Where(x => moduleIds.Contains(x.ModuleInfoExtended.Id)).ToArray();
                        var wrappedModuleIds = wrappedModules.Select(x => x.ModuleInfoExtended.Id).ToHashSet();
                        if (modules.Length != wrappedModules.Length)
                        {
                            var missingModules = moduleIds.Except(wrappedModuleIds);
                            HintManager.ShowHint(@$"Cancelled Import!

Missing modules:
{string.Join(Environment.NewLine, missingModules)}");
                            return;
                        }

                        foreach (var moduleVM in moduleVMs)
                        {
                            if (moduleVM.IsSelected)
                                moduleVM.ExecuteSelect();
                        }

                        var mismatchedVersions = new List<ModuleMismatch>();
                        foreach (var (id, version) in modules)
                        {
                            if (moduleVMs.FirstOrDefault(x => x.ModuleInfoExtended.Id == id) is { } module)
                            {
                                var launcherModuleVersion = ToString(module.ModuleInfoExtended.Version);
                                if (launcherModuleVersion == version)
                                {
                                    if (!module.IsSelected)
                                        module.ExecuteSelect();
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
                if (_launcherVM.ModsData.GetModules() is not { } moduleVMs)
                {
                    HintManager.ShowHint(@$"Cancelled Export!

Internal BUTRLoader error: GetModules() null");
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
                        var modules = moduleVMs
                            .Where(x => x.IsSelected)
                            .Select(x => x.ModuleInfoExtended)
                            .Select(x => new ModuleListEntry(x.Id, ToString(x.Version)))
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