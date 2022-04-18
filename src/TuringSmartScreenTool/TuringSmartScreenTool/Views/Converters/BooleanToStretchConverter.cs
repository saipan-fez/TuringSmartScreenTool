using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TuringSmartScreenTool.Views.Converters
{
    public class BooleanToStretchConverter : IValueConverter
    {
        public Stretch TrueValue { get; set; } = Stretch.Uniform;
        public Stretch FalseValue { get; set; } = Stretch.None;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool v)
                return v ? TrueValue : FalseValue;
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Stretch v)
                return v == TrueValue;
            else
                return value;
        }
    }
}
