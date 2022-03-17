using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Helpers
{
    /// <summary>
    /// https://github.com/Aragas/Bannerlord.MBOptionScreen/blob/dev/src/MCM/Abstractions/Settings/SettingType.cs
    /// </summary>
    internal enum SettingType
    {
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        NONE = -1,
        Bool,
        Int,
        Float,
        String
    }

    /// <summary>
    /// https://github.com/Aragas/Bannerlord.MBOptionScreen/blob/dev/src/MCM/Abstractions/Settings/Models/ISettingsPropertyDefinition.cs
    /// </summary>
    internal interface ISettingsPropertyDefinition
    {
        IRef PropertyReference { get; }

        SettingType SettingType { get; }

        string DisplayName { get; }
        decimal MinValue { get; }
        decimal MaxValue { get; }
    }

    /// <summary>
    /// https://github.com/Aragas/Bannerlord.MBOptionScreen/blob/dev/src/MCM/Abstractions/Settings/Models/SettingsPropertyDefinition.cs
    /// </summary>
    internal sealed class SettingsPropertyDefinition : ISettingsPropertyDefinition
    {
        public string DisplayName { get; init; } = default!;
        public IRef PropertyReference { get; init; } = default!;
        public SettingType SettingType { get; init; } = default!;

        public decimal MinValue { get; init; } = default!;
        public decimal MaxValue { get; init; } = default!;
    }
}