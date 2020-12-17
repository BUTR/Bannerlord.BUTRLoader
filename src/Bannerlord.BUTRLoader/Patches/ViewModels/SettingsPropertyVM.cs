using Bannerlord.BUTRLoader.Helpers;

using System.ComponentModel;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Patches.ViewModels
{
    internal sealed class SettingsPropertyVM : ViewModel
    {
        private ISettingsPropertyDefinition SettingPropertyDefinition { get; }
        private IRef PropertyReference => SettingPropertyDefinition.PropertyReference;
        private SettingType SettingType => SettingPropertyDefinition.SettingType;

        [DataSourceProperty]
        public string Name => SettingPropertyDefinition.DisplayName;

        [DataSourceProperty]
        public bool IsIntVisible { get; }
        [DataSourceProperty]
        public bool IsFloatVisible { get; }
        [DataSourceProperty]
        public bool IsBoolVisible { get; }
        [DataSourceProperty]
        public bool IsStringVisible { get; }

        [DataSourceProperty]
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
        [DataSourceProperty]
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
        [DataSourceProperty]
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
        [DataSourceProperty]
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

        [DataSourceProperty]
        public float MaxValue => (float) SettingPropertyDefinition.MaxValue;
        [DataSourceProperty]
        public float MinValue => (float) SettingPropertyDefinition.MinValue;
        [DataSourceProperty]
        public string TextBoxValue => SettingType switch
        {
            SettingType.Int when PropertyReference.Value is int val => val.ToString(),
            SettingType.Float when PropertyReference.Value is float val => val.ToString(),
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