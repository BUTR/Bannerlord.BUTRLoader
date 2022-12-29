using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.BUTRLoader.Options;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.ViewModels
{
    internal enum OptionsType
    {
        Launcher, Game, Engine
    }

    internal sealed class BUTRLauncherOptionsVM : BUTRViewModel
    {
        private static readonly string GameConfigPath =
            Path.Combine($@"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}", "Mount and Blade II Bannerlord", "Configs", "BannerlordConfig.txt");
        private static readonly string EngineConfigPath =
            Path.Combine($@"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}", "Mount and Blade II Bannerlord","Configs", "engine_config.txt");

        private readonly OptionsType _optionsType;
        private LauncherExData? _launcherExData;
        private readonly Action _saveUserData;
        private readonly Action _refreshOptions;

        [BUTRDataSourceProperty]
        public bool IsDisabled { get => _isDisabled; set => SetField(ref _isDisabled, value, nameof(IsDisabled)); }
        private bool _isDisabled;

        [BUTRDataSourceProperty]
        public MBBindingList<SettingsPropertyVM> SettingProperties { get => _settingProperties; set => SetField(ref _settingProperties, value, nameof(SettingProperties)); }
        private MBBindingList<SettingsPropertyVM> _settingProperties = new();


        public BUTRLauncherOptionsVM(OptionsType optionsType, Action saveUserData, Action refreshOptions)
        {
            _optionsType = optionsType;
            _saveUserData = saveUserData;
            _refreshOptions = refreshOptions;
        }

        public void Refresh()
        {
            SettingProperties.Clear();
            switch (_optionsType)
            {
                case OptionsType.Launcher:
                    RefreshLauncherOptions();
                    break;
                case OptionsType.Game:
                    RefreshGameOptions();
                    break;
                case OptionsType.Engine:
                    RefreshEngineOptions();
                    break;
            }
        }
        private void RefreshLauncherOptions()
        {
            _launcherExData = new LauncherExData(
                LauncherSettings.AutomaticallyCheckForUpdates,
                LauncherSettings.UnblockFiles,
                LauncherSettings.FixCommonIssues,
                LauncherSettings.CompactModuleList,
                LauncherSettings.HideRandomImage,
                LauncherSettings.DisableBinaryCheck,
                LauncherSettings.BetaSorting,
                LauncherSettings.BigMode);

            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Unblock Files on Start",
                HintText = "Automatically unblock's .dll files on start",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.UnblockFiles))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Fix Common Issues",
                HintText = "Fixes issues like 0Harmony.dll bein in the /bin folder",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.FixCommonIssues))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Compact Module List",
                HintText = "Requires restart! Makes the Mods tab content smaller",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.CompactModuleList))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Disable Binary Compatibility Check",
                HintText = "DISABLED! Requires restart! Disables Launcher's own check for binary compatibility of mods",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.DisableBinaryCheck))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Hide Random Image",
                HintText = "Hide's the Rider image so the launcher loosk more compact",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.HideRandomImage))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Unblock Files",
                HintText = "Unblock all .dll files on /Modules folder",
                SettingType = SettingType.Button,
                PropertyReference = new ProxyRef<string>(() => "Unblock", _ =>
                {
                    NtfsUnblocker.UnblockPath(Path.Combine(BasePath.Name, ModuleInfoHelper.ModulesFolder));
                })
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Beta Sorting",
                HintText = "Uses the new sorting algorithm after v1.12.x. Disable to use the old algorithm",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.BetaSorting))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Big Mode",
                HintText = "Makes the launcher bigger in height",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.BigMode))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Restore Game Options Backup",
                HintText = "BUTRLoader always makes a backup before saving the first time. This will restore the original files",
                SettingType = SettingType.Button,
                PropertyReference = new ProxyRef<string>(() => "Restore", _ =>
                {
                    var backupPath = $"{GameConfigPath}.bak";
                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, GameConfigPath, true);
                        File.Delete(backupPath);
                        _refreshOptions();
                    }
                })
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Restore Engine Options Backup",
                HintText = "BUTRLoader always makes a backup before saving the first time. This will restore the original files",
                SettingType = SettingType.Button,
                PropertyReference = new ProxyRef<string>(() => "Restore", _ =>
                {
                    var backupPath = $"{EngineConfigPath}.bak";
                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, EngineConfigPath, true);
                        File.Delete(backupPath);
                        _refreshOptions();
                    }
                })
            }));
            /*
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Automatically Check for Updates",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(BUTRLoaderAppDomainManager).GetProperty(nameof(BUTRLoaderAppDomainManager.AutomaticallyCheckForUpdates))!, this)
            }));
            */
        }
        private void RefreshGameOptions()
        {
            var content = File.ReadAllText(GameConfigPath);
            foreach (var keyValue in content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var split = keyValue.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length != 2) continue;
                var key = split[0].Trim();
                var value = split[1].Trim();
                SettingProperties.Add(CreateSettingsPropertyVM(key, value, ToSeparateWords));
            }
        }
        private void RefreshEngineOptions()
        {
            var content = File.ReadAllText(EngineConfigPath);
            foreach (var keyValue in content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var split = keyValue.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length != 2) continue;
                var key = split[0].Trim();
                var value = split[1].Trim();
                SettingProperties.Add(CreateSettingsPropertyVM(key, value, x => ToTitleCase(x.Replace("_", " "))));
            }
        }

        public void Save()
        {
            switch (_optionsType)
            {
                case OptionsType.Launcher:
                    SaveLauncherOptions();
                    break;
                case OptionsType.Game:
                    SaveGameOptions();
                    break;
                case OptionsType.Engine:
                    SaveEngineOptions();
                    break;
            }
        }
        private void SaveLauncherOptions()
        {
            if (_launcherExData is null)
                return;

            if (_launcherExData.AutomaticallyCheckForUpdates != LauncherSettings.AutomaticallyCheckForUpdates)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.UnblockFiles != LauncherSettings.UnblockFiles)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.FixCommonIssues != LauncherSettings.FixCommonIssues)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.CompactModuleList != LauncherSettings.CompactModuleList)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.HideRandomImage != LauncherSettings.HideRandomImage)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.DisableBinaryCheck != LauncherSettings.DisableBinaryCheck)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.BetaSorting != LauncherSettings.BetaSorting)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.BigMode != LauncherSettings.BigMode)
            {
                _saveUserData();
                return;
            }
        }
        private void SaveGameOptions()
        {
            var backupPath = $"{GameConfigPath}.bak";
            if (!File.Exists(backupPath))
                File.Copy(GameConfigPath, backupPath);

            var hasChanges = false;
            var sb = new StringBuilder();
            foreach (var settingProperty in SettingProperties)
            {
                if (settingProperty.SettingPropertyDefinition is not ConfigSettingsPropertyDefinition propertyDefinition)
                    continue;

                if (!string.Equals(propertyDefinition.OriginalValue, settingProperty.ValueAsString, StringComparison.Ordinal))
                    hasChanges = true;

                sb.AppendLine($"{propertyDefinition.ConfigKey}={settingProperty.ValueAsString}");
            }
            if (hasChanges)
                File.WriteAllText(GameConfigPath, sb.ToString());
        }
        private void SaveEngineOptions()
        {
            var backupPath = $"{EngineConfigPath}.bak";
            if (!File.Exists(backupPath))
                File.Copy(EngineConfigPath, backupPath);

            var hasChanges = false;
            var sb = new StringBuilder();
            foreach (var settingProperty in SettingProperties)
            {
                if (settingProperty.SettingPropertyDefinition is not ConfigSettingsPropertyDefinition propertyDefinition)
                    continue;

                if (!string.Equals(propertyDefinition.OriginalValue, settingProperty.ValueAsString, StringComparison.Ordinal))
                    hasChanges = true;

                sb.AppendLine($"{propertyDefinition.ConfigKey} = {settingProperty.ValueAsString}");
            }
            if (hasChanges)
                File.WriteAllText(EngineConfigPath, sb.ToString());
        }

        private static SettingsPropertyVM CreateSettingsPropertyVM(string key, string value, Func<string, string> keyProcessor)
        {
            var settingsType = bool.TryParse(value, out _) ? SettingType.Bool
                : int.TryParse(value, out _) ? SettingType.Int
                : float.TryParse(value, out _) ? SettingType.Float
                : SettingType.String;
            var storage = settingsType switch
            {
                SettingType.Bool => (IRef) new StorageRef<bool>(bool.Parse(value)),
                SettingType.Int => (IRef) new StorageRef<int>(int.Parse(value)),
                SettingType.Float => (IRef) new StorageRef<float>(float.Parse(value)),
                SettingType.String => (IRef) new StorageRef<string>(value),
                _ => throw new ArgumentOutOfRangeException()
            };
            var propertyRef = settingsType switch
            {
                SettingType.Bool => (IRef) new ProxyRef<bool>(() => (bool) storage.Value!, val => { storage.Value = val; }),
                SettingType.Int => (IRef) new ProxyRef<int>(() => (int) storage.Value!, val => { storage.Value = val; }),
                SettingType.Float => (IRef) new ProxyRef<float>(() => (float) storage.Value!, val => { storage.Value = val; }),
                SettingType.String => (IRef) new ProxyRef<string>(() => (string) storage.Value!, val => { storage.Value = val; }),
                _ => throw new ArgumentOutOfRangeException()
            };
            return new SettingsPropertyVM(new ConfigSettingsPropertyDefinition
            {
                ConfigKey = key,
                OriginalValue = value,
                DisplayName = keyProcessor(key),
                SettingType = settingsType,
                PropertyReference = propertyRef
            });
        }

        [return: NotNullIfNotNull("value")]
        private static string? ToSeparateWords(string? value)
        {
            if (value == null) return null;
            if (value.Length <= 1) return value;

            var inChars = value.ToCharArray();
            var uCWithAnyLC = new List<int>();
            var i = 0;
            while (i < inChars.Length && char.IsUpper(inChars[i])) { ++i; }

            for (; i < inChars.Length; i++)
            {
                if (!char.IsUpper(inChars[i])) continue;
                uCWithAnyLC.Add(i);
                if (++i >= inChars.Length || !char.IsUpper(inChars[i])) continue;
                while (++i < inChars.Length)
                {
                    if (char.IsUpper(inChars[i])) continue;
                    uCWithAnyLC.Add(i - 1);
                    break;
                }
            }

            var outChars = new char[inChars.Length + uCWithAnyLC.Count];
            var lastIndex = 0;
            for (i = 0; i < uCWithAnyLC.Count; i++)
            {
                var currentIndex = uCWithAnyLC[i];
                Array.Copy(inChars, lastIndex, outChars, lastIndex + i, currentIndex - lastIndex);
                outChars[currentIndex + i] = ' ';
                lastIndex = currentIndex;
            }

            var lastPos = lastIndex + uCWithAnyLC.Count;
            Array.Copy(inChars, lastIndex, outChars, lastPos, outChars.Length - lastPos);
            return new string(outChars);
        }

        [return: NotNullIfNotNull("value")]
        private static string? ToTitleCase(string? value) => value is null ? null : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value);
    }
}