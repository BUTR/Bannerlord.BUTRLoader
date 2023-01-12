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
            Path.Combine($@"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}", "Mount and Blade II Bannerlord", "Configs", "engine_config.txt");

        public static Dictionary<string, string> GetGameOptions()
        {
            var dict = new Dictionary<string, string>();
            try
            {
                var content = File.ReadAllText(GameConfigPath);
                foreach (var keyValue in content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var split = keyValue.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length != 2) continue;
                    var key = split[0].Trim();
                    var value = split[1].Trim();
                    dict.Add(key, value);
                }
            }
            catch (Exception) { }
            return dict;
        }
        public static Dictionary<string, string> GetEngineOptions()
        {
            var dict = new Dictionary<string, string>();
            try
            {
                var content = File.ReadAllText(EngineConfigPath);
                foreach (var keyValue in content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var split = keyValue.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length != 2) continue;
                    var key = split[0].Trim();
                    var value = split[1].Trim();
                    dict.Add(key, value);
                }
            }
            catch (Exception) { }
            return dict;
        }
    }
}