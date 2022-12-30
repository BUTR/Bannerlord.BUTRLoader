using System;
using System.Collections.Generic;
using System.Xml;

namespace Bannerlord.BUTRLoader.Localization
{
    internal static partial class BUTRLocalizationManager
    {
        private static readonly Dictionary<string, BUTRLocalizedText> _localizationStorage = new();
        public static string? GetTranslatedText(string languageId, string id) =>
            _localizationStorage.TryGetValue(id, out var localizedText) ? localizedText.GetTranslatedText(languageId) : null;

        public static void DeserializeStrings(XmlNode node, string languageId)
        {
            if (node.Attributes == null)
            {
                throw new InvalidOperationException("Node attributes are null!");
            }

            var id = node.Attributes["id"].Value;
            var text = node.Attributes["text"].Value;
            if (!_localizationStorage.ContainsKey(id))
            {
                _localizationStorage.Add(id, new BUTRLocalizedText(id));
            }

            _localizationStorage[id].AddTranslation(languageId, text);
        }

        public static void Clear()
        {
            _localizationStorage.Clear();
            BUTRLanguageData.Clear();
        }
    }
}