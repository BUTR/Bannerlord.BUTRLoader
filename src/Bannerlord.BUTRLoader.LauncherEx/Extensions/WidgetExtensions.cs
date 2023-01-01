using HarmonyLib.BUTR.Extensions;

using System.Collections.Generic;
using System.Runtime.CompilerServices;

using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace Bannerlord.BUTRLoader.Extensions
{
    internal static class WidgetExtensions
    {
        private delegate void OnPropertyChangedDelegate(PropertyOwnerObject instance, object value, [CallerMemberName] string? propertyName = null);
        private static readonly OnPropertyChangedDelegate? OnPropertyChangedMethod =
            AccessTools2.GetDelegate<OnPropertyChangedDelegate>(typeof(PropertyOwnerObject), "OnPropertyChanged");

        private delegate void SetIsPressedDelegate(Widget instance, bool value);
        private static readonly SetIsPressedDelegate? SetIsPressedMethod =
            AccessTools2.GetPropertySetterDelegate<SetIsPressedDelegate>(typeof(Widget), "IsPressed");

        public static bool IsPointInsideMeasuredArea(this Widget widget)
        {
            var method = AccessTools2.Method(typeof(Widget), "IsPointInsideMeasuredArea");
            var property = AccessTools2.Property(typeof(EventManager), "MousePosition");

            if (method is null || property is null)
                return false;

            if (method?.Invoke(widget, new object[] { property.GetValue(widget.EventManager) }) is not bool result)
                return false;

            return result;
        }

        public static void SetIsPressed(this Widget widget, bool value) => SetIsPressedMethod?.Invoke(widget, value);

        public static bool SetField<T>(this Widget widget, ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            OnPropertyChangedMethod?.Invoke(widget, value, propertyName);
            return true;
        }
    }
}