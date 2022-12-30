using System;
using System.Collections.Generic;
using System.IO;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal static class ConfigReader
    {
        public static readonly string GameConfigPath =
            Path.Combine($@"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}", "Mount and Blade II Bannerlord", "Configs", "BannerlordConfig.txt");
        public static readonly string EngineConfigPath =
            Path.Combine($@"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}", "Mount and Blade II Bannerlord","Configs", "engine_config.txt");

        public static IEnumerable<KeyValuePair<string, string>> GetGameOptions()
        {
            var content = File.ReadAllText(GameConfigPath);
            foreach (var keyValue in content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var split = keyValue.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length != 2) continue;
                var key = split[0].Trim();
                var value = split[1].Trim();
                yield return new KeyValuePair<string, string>(key, value);
            }
        }
        public static IEnumerable<KeyValuePair<string, string>> GetEngineOptions()
        {
            var content = File.ReadAllText(EngineConfigPath);
            foreach (var keyValue in content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var split = keyValue.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length != 2) continue;
                var key = split[0].Trim();
                var value = split[1].Trim();
                yield return new KeyValuePair<string, string>(key, value);
            }
        }
    }
}