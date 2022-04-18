using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TuringSmartScreenTool.Views.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; } = Visibility.Visible;
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool v)
                return v ? TrueValue : FalseValue;
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v == TrueValue;
            else
                return value;
        }
    }
}
