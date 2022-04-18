using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;

namespace TuringSmartScreenTool.Views.Converters
{
    public class ObjectToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is Color color)
                {
                    return new SolidColorBrush(color);
                }
                else if (value is string str)
                {
                    if (!File.Exists(str))
                        return DependencyProperty.UnsetValue;

                    var extension = Path.GetExtension(str).ToLower();
                    if (extension == ".gif")
                    {
                        var image = new Image();
                        AnimationBehavior.SetSourceUri(image, new Uri(str));
                        AnimationBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                        AnimationBehavior.SetAutoStart(image, true);
                        return new VisualBrush(image);
                    }
                    else
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(str);
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return new ImageBrush(bitmap)
                        {
                            Stretch = Stretch.None
                        };
                    }
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
