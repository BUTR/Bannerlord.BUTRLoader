using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader
{
    internal sealed class ExtendedModuleInfo : ModuleInfo
    {
        public string Url { get; private set; } = string.Empty;

        public readonly List<DependedModuleMetadata> DependedModuleMetadatas = new List<DependedModuleMetadata>();

        public new void Load(string alias)
        {
            base.Load(alias);

            Url = string.Empty;

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(GetPath(alias));

            var xmlNodeModule = xmlDocument.SelectSingleNode("Module");

            Url = xmlNodeModule?.SelectSingleNode("Url")?.Attributes?["value"]?.InnerText ?? string.Empty;

            DependedModuleMetadatas.Clear();
            var xmlNodeDependedModuleMetadatas = xmlNodeModule?.SelectSingleNode("DependedModuleMetadatas");
            foreach (var xmlElement in xmlNodeDependedModuleMetadatas?.OfType<XmlElement>() ?? Enumerable.Empty<XmlElement>())
            {
                if (xmlElement.Attributes["id"] is { } idAttr && xmlElement.Attributes["order"] is { } orderAttr && Enum.TryParse<LoadType>(orderAttr.InnerText, out var order))
                {
                    var optional = xmlElement.Attributes["optional"]?.InnerText.Equals("true") ?? false;
                    var version = ApplicationVersionUtils.TryParse(xmlElement.Attributes["version"]?.InnerText, out var v) ? v : ApplicationVersion.Empty;
                    DependedModuleMetadatas.Add(new DependedModuleMetadata(idAttr.InnerText, order, optional, version));
                }
            }
        }

        public override string ToString() => $"{Id} - {Version}";
    }
}