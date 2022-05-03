using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TuringSmartScreenTool.Views.Converters
{
    public class NullToNotDependencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return DependencyProperty.UnsetValue;
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
