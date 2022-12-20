using Bannerlord.BUTRLoader.Helpers;

using System.ComponentModel;
using System.Globalization;

namespace Bannerlord.BUTRLoader.Patches.ViewModels
{
    internal sealed class SettingsPropertyVM : BUTRViewModel
    {
        private ISettingsPropertyDefinition SettingPropertyDefinition { get; }
        private IRef PropertyReference => SettingPropertyDefinition.PropertyReference;
        private SettingType SettingType => SettingPropertyDefinition.SettingType;

        [BUTRDataSourceProperty]
        public string Name => SettingPropertyDefinition.DisplayName;

        [BUTRDataSourceProperty]
        public bool IsIntVisible { get; }
        [BUTRDataSourceProperty]
        public bool IsFloatVisible { get; }
        [BUTRDataSourceProperty]
        public bool IsBoolVisible { get; }
        [BUTRDataSourceProperty]
        public bool IsStringVisible { get; }

        [BUTRDataSourceProperty]
        public float FloatValue
        {
            get => IsFloatVisible ? PropertyReference.Value is float val ? val : float.MinValue : 0f;
            set
            {
                if (IsFloatVisible && FloatValue != value)
                {
                    PropertyReference.Value = value;
                    OnPropertyChanged(nameof(FloatValue));
                    OnPropertyChanged(nameof(TextBoxValue));
                }
            }
        }
        [BUTRDataSourceProperty]
        public int IntValue
        {
            get => IsIntVisible ? PropertyReference.Value is int val ? val : int.MinValue : 0;
            set
            {
                if (IsIntVisible && IntValue != value)
                {
                    PropertyReference.Value = value;
                    OnPropertyChanged(nameof(IntValue));
                    OnPropertyChanged(nameof(TextBoxValue));
                }
            }
        }
        [BUTRDataSourceProperty]
        public bool BoolValue
        {
            get => IsBoolVisible && PropertyReference.Value is bool val ? val : false;
            set
            {
                if (IsBoolVisible && BoolValue != value)
                {
                    PropertyReference.Value = value;
                    OnPropertyChanged(nameof(BoolValue));
                }
            }
        }
        [BUTRDataSourceProperty]
        public string StringValue
        {
            get => IsStringVisible ? PropertyReference.Value is string val ? val : "ERROR" : string.Empty;
            set
            {
                if (IsStringVisible && StringValue != value)
                {
                    PropertyReference.Value = value;
                    OnPropertyChanged(nameof(StringValue));
                }
            }
        }

        [BUTRDataSourceProperty]
        public float MaxValue => (float) SettingPropertyDefinition.MaxValue;
        [BUTRDataSourceProperty]
        public float MinValue => (float) SettingPropertyDefinition.MinValue;
        [BUTRDataSourceProperty]
        public string TextBoxValue => SettingType switch
        {
            SettingType.Int when PropertyReference.Value is int val => val.ToString(),
            SettingType.Float when PropertyReference.Value is float val => val.ToString(CultureInfo.InvariantCulture),
            _ => string.Empty
        };

        public SettingsPropertyVM(ISettingsPropertyDefinition definition)
        {
            SettingPropertyDefinition = definition;

            // Moved to constructor
            IsIntVisible = SettingType == SettingType.Int;
            IsFloatVisible = SettingType == SettingType.Float;
            IsBoolVisible = SettingType == SettingType.Bool;
            IsStringVisible = SettingType == SettingType.String;
            // Moved to constructor

            PropertyReference.PropertyChanged += PropertyReference_OnPropertyChanged;

            RefreshValues();
        }
        public override void OnFinalize()
        {
            PropertyReference.PropertyChanged -= PropertyReference_OnPropertyChanged;

            base.OnFinalize();
        }
        private void PropertyReference_OnPropertyChanged(object? obj, PropertyChangedEventArgs args)
        {
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            switch (SettingType)
            {
                case SettingType.Bool:
                    OnPropertyChanged(nameof(BoolValue));
                    break;
                case SettingType.Int:
                    OnPropertyChanged(nameof(IntValue));
                    break;
                case SettingType.Float:
                    OnPropertyChanged(nameof(FloatValue));
                    break;
                case SettingType.String:
                    OnPropertyChanged(nameof(StringValue));
                    break;
            }
            OnPropertyChanged(nameof(TextBoxValue));
        }

        public override string ToString() => Name;
    }
}