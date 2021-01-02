﻿using Bannerlord.BUTRLoader.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.ModuleInfoExtended
{
    /// <summary>
    /// https://github.com/BUTR/Bannerlord.ButterLib/blob/dev/src/Bannerlord.ButterLib/Helpers/ModuleInfo/ExtendedModuleInfo.cs
    /// </summary>
    internal sealed class ModuleInfo2 : IEquatable<ModuleInfo2>, IEquatable<ModuleInfo>
    {
        public static string PathPrefix => Path.Combine(BasePath.Name, "Modules");

        public string Id { get; internal set; } = string.Empty;
        public string Name { get; internal set; } = string.Empty;
        public bool IsOfficial { get; internal set; }
        public ApplicationVersion Version { get; internal set; }
        public string Alias { get; internal set; } = string.Empty;
        public bool IsSingleplayerModule { get; internal set; }
        public bool IsMultiplayerModule { get; internal set; }
        public bool IsSelected { get; set; }
        public List<SubModuleInfo> SubModules { get; internal set; } = new();
        public List<DependedModule> DependedModules { get; internal set; } = new();

        public string Url { get; internal set; } = string.Empty;

        public List<DependedModuleMetadata> DependedModuleMetadatas { get; internal set; }  = new();

        public void Load(string alias)
        {
            Alias = alias;
            SubModules.Clear();
            DependedModules.Clear();
            DependedModuleMetadatas.Clear();

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(ModuleInfo.GetPath(alias));

            var moduleNode = xmlDocument.SelectSingleNode("Module");

            Name = moduleNode?.SelectSingleNode("Name")?.Attributes?["value"]?.InnerText ?? string.Empty;
            Id = moduleNode?.SelectSingleNode("Id")?.Attributes?["value"]?.InnerText ?? string.Empty;
            ApplicationVersionUtils.TryParse(moduleNode?.SelectSingleNode("Version")?.Attributes?["value"]?.InnerText, out var parsedVersion);
            Version = parsedVersion;

            IsOfficial = moduleNode?.SelectSingleNode("Official")?.Attributes?["value"]?.InnerText?.Equals("true") == true;
            IsSelected = moduleNode?.SelectSingleNode("DefaultModule")?.Attributes?["value"]?.InnerText?.Equals("true") == true || IsNative();
            IsSingleplayerModule = moduleNode?.SelectSingleNode("SingleplayerModule")?.Attributes?["value"]?.InnerText?.Equals("true") == true;
            IsMultiplayerModule = moduleNode?.SelectSingleNode("MultiplayerModule")?.Attributes?["value"]?.InnerText?.Equals("true") == true;

            var dependedModules = moduleNode?.SelectSingleNode("DependedModules");
            var dependedModulesList = dependedModules?.SelectNodes("DependedModule");
            for (var i = 0; i < dependedModulesList?.Count; i++)
            {
                if (dependedModulesList[i]?.Attributes["Id"] is { } idAttr)
                {
                    ApplicationVersionUtils.TryParse(dependedModulesList[i]?.Attributes?["DependentVersion"]?.InnerText, out var version);
                    DependedModules.Add(new DependedModule
                    {
                        ModuleId = idAttr.InnerText,
                        Version = version
                    });
                }
            }

            var subModules = moduleNode?.SelectSingleNode("SubModules");
            var subModuleList = subModules?.SelectNodes("SubModule");
            for (var i = 0; i < subModuleList?.Count; i++)
            {
                var subModuleInfo = new SubModuleInfo();
                try
                {
                    subModuleInfo.LoadFrom(subModuleList[i], Path.Combine(PathPrefix, alias));
                    SubModules.Add(subModuleInfo);
                }
                catch { }
            }

            // Custom data
            Url = moduleNode?.SelectSingleNode("Url")?.Attributes?["value"]?.InnerText ?? string.Empty;

            var dependedModuleMetadatas = moduleNode?.SelectSingleNode("DependedModuleMetadatas");
            var dependedModuleMetadatasList = dependedModuleMetadatas?.SelectNodes("DependedModuleMetadata");
            for (var i = 0; i < dependedModuleMetadatasList?.Count; i++)
            {
                if (dependedModuleMetadatasList[i]?.Attributes["id"] is { } idAttr)
                {
                    var incompatible = dependedModuleMetadatasList[i]?.Attributes["incompatible"]?.InnerText.Equals("true") ?? false;
                    if (incompatible)
                    {
                        DependedModuleMetadatas.Add(new DependedModuleMetadata
                        {
                            Id = idAttr.InnerText,
                            LoadType = LoadType.NONE,
                            IsOptional = false,
                            IsIncompatible = incompatible,
                            Version = ApplicationVersion.Empty
                        });
                    }
                    else if (dependedModuleMetadatasList[i]?.Attributes["order"] is { } orderAttr && Enum.TryParse<LoadTypeParse>(orderAttr.InnerText, out var order))
                    {
                        var optional = dependedModuleMetadatasList[i]?.Attributes["optional"]?.InnerText.Equals("true") ?? false;
                        var version = ApplicationVersionUtils.TryParse(dependedModuleMetadatasList[i]?.Attributes["version"]?.InnerText, out var v) ? v : ApplicationVersion.Empty;
                        DependedModuleMetadatas.Add(new DependedModuleMetadata
                        {
                            Id = idAttr.InnerText,
                            LoadType = (LoadType) order,
                            IsOptional = optional,
                            IsIncompatible = incompatible,
                            Version = version
                        });
                    }
                }
            }

            // Fixed Launcher supported optional tag
            var loadAfterModules = moduleNode?.SelectSingleNode("LoadAfterModules");
            var loadAfterModuleList = loadAfterModules?.SelectNodes("LoadAfterModule");
            for (var i = 0; i < loadAfterModuleList?.Count; i++)
            {
                if (loadAfterModuleList[i]?.Attributes["Id"] is { } idAttr)
                {
                    DependedModuleMetadatas.Add(new DependedModuleMetadata
                    {
                        Id = idAttr.InnerText,
                        LoadType = LoadType.NONE,
                        IsOptional = true,
                        IsIncompatible = false,
                        Version = ApplicationVersion.Empty
                    });
                }
            }

            // Bannerlord Launcher supported optional tag
            var optionalDependModules = moduleNode?.SelectSingleNode("OptionalDependModules");
            var optionalDependModuleList =
                (dependedModules?.SelectNodes("OptionalDependModule")?.Cast<XmlNode>() ?? Enumerable.Empty<XmlNode>())
                .Concat(optionalDependModules?.SelectNodes("OptionalDependModule")?.Cast<XmlNode>() ?? Enumerable.Empty<XmlNode>())
                .Concat(optionalDependModules?.SelectNodes("DependModule")?.Cast<XmlNode>() ?? Enumerable.Empty<XmlNode>()).ToList();
            for (var i = 0; i < optionalDependModuleList.Count; i++)
            {
                if (optionalDependModuleList[i]?.Attributes["Id"] is { } idAttr)
                {
                    DependedModuleMetadatas.Add(new DependedModuleMetadata
                    {
                        Id = idAttr.InnerText,
                        LoadType = LoadType.NONE,
                        IsOptional = true,
                        IsIncompatible = false,
                        Version = ApplicationVersion.Empty
                    });
                }
            }
        }

        public bool IsNative() => Id.Equals("native", StringComparison.OrdinalIgnoreCase);

        public override string ToString() => $"{Id} - {Version}";

        public bool Equals(ModuleInfo2? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }
        public bool Equals(ModuleInfo? other)
        {
            if (other is null) return false;
            return Id == other.Id;
        }
        public override bool Equals(object? obj) =>
            ReferenceEquals(this, obj) || (obj is ModuleInfo2 other && Equals(other)) || (obj is ModuleInfo other2 && Equals(other2));

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(ModuleInfo2? left, ModuleInfo2? right) => Equals(left, right);
        public static bool operator !=(ModuleInfo2? left, ModuleInfo2? right) => !Equals(left, right);
    }
}