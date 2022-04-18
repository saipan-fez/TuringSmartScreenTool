using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TuringSmartScreenTool.Views.Converters
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; } = Visibility.Visible;
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterString = parameter as string;
            if (parameterString is null)
                return DependencyProperty.UnsetValue;

            if (!Enum.IsDefined(value.GetType(), value))
                return DependencyProperty.UnsetValue;

            var parameterValue = Enum.Parse(value.GetType(), parameterString);
            return parameterValue.Equals(value) ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterString = parameter as string;
            if (parameterString is null)
                return DependencyProperty.UnsetValue;

            if (TrueValue.Equals(value))
                return Enum.Parse(targetType, parameterString);
            else
                return DependencyProperty.UnsetValue;
        }
    }
}
