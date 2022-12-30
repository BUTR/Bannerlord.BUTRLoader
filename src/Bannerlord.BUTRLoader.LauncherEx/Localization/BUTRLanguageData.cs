using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Bannerlord.BUTRLoader.Localization
{
    /// <summary>
    /// Storage of all serialized languages
    /// </summary>
    internal partial class BUTRLanguageData
    {
        public string StringId { get; }
        public string Title { get; private set; } = string.Empty;
        public string[] SupportedIsoCodes { get; private set; } = Array.Empty<string>();
        public bool IsUnderDevelopment { get; private set; } = true;
        public bool IsValid { get; private set; } = false;

        public BUTRLanguageData(string stringId) => StringId = stringId;

        private void Deserialize(XmlNode node)
        {
            if (node.Attributes == null)
            {
                throw new InvalidOperationException("LanguageData node does not have any Attributes!");
            }

            if (node.Attributes["name"]?.Value is { } title && !string.IsNullOrEmpty(title))
            {
                Title = title!;
            }

            if (node.Attributes["supported_iso"]?.Value is { } supportedIso)
            {
                SupportedIsoCodes = new List<string>(SupportedIsoCodes).Union(supportedIso.Split(',')).ToArray<string>();
            }

            if (node.Attributes["under_development"].Value is { } underDevelopment)
            {
                IsUnderDevelopment = bool.TryParse(underDevelopment, out var flag) && flag;
            }

            IsValid = SupportedIsoCodes.Length != 0;

            foreach (var childNode in node.ChildNodes.OfType<XmlNode>())
            {
                if (childNode is not { Name: "strings", HasChildNodes: true }) continue;
                if (string.Equals(StringId, BUTRLocalizationManager.DefaultLanguage, StringComparison.OrdinalIgnoreCase)) continue;
                foreach (var stringNode in childNode.ChildNodes.OfType<XmlNode>())
                {
                    if (stringNode.Name == "string" && stringNode.NodeType != XmlNodeType.Comment)
                    {
                        BUTRLocalizationManager.DeserializeStrings(stringNode, StringId);
                    }
                }
            }
        }
    }
}