using System.Collections.Generic;

namespace Bannerlord.BUTRLoader.Localization
{
    /// <summary>
    /// Storage of all localized strings
    /// </summary>
    internal class BUTRLocalizedText
    {
        private readonly string _stringId;
        private readonly Dictionary<string, string> _localizedTextDictionary = new();

        public BUTRLocalizedText(string stringId)
        {
            _stringId = stringId;
        }

        public BUTRLocalizedText AddTranslation(string language, string translation)
        {
            if (!_localizedTextDictionary.ContainsKey(language))
            {
                _localizedTextDictionary.Add(language, translation);
            }
            return this;
        }

        public string? GetTranslatedText(string languageId)
        {
            if (_localizedTextDictionary.TryGetValue(languageId, out var text))
            {
                return text;
            }

            if (_localizedTextDictionary.TryGetValue("English", out text))
            {
                return text;
            }

            return null;
        }
    }
}