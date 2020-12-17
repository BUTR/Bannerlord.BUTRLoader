using System.Diagnostics.CodeAnalysis;

namespace Bannerlord.BUTRLoader.Helpers
{
    public enum SettingType
    {
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        NONE = -1,
        Bool,
        Int,
        Float,
        String
    }

    public interface ISettingsPropertyDefinition
    {
        IRef PropertyReference { get; }

        SettingType SettingType { get; }

        string DisplayName { get; }
        decimal MinValue { get; }
        decimal MaxValue { get; }
    }

    public class SettingsPropertyDefinition : ISettingsPropertyDefinition
    {
        public string DisplayName { get; init; } = default!;
        public IRef PropertyReference { get; init; } = default!;
        public SettingType SettingType { get; init; } = default!;

        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
    }
}