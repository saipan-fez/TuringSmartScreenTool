using System.Collections.Generic;
using System.Windows.Controls;
using ModernWpf.Controls;
using SharpVectors.Converters;
using TuringSmartScreenTool.Helpers;
using TuringSmartScreenTool.ViewModels.Editors;
using TuringSmartScreenTool.Views.ContentDialogs.Interdfaces;
using WeatherLib.Entities;

namespace TuringSmartScreenTool.Views.ContentDialogs
{
    public partial class WeatherIconPreviewContentDialog : ContentDialog, IWeatherIconPreviewContentDialog
    {
        public WeatherIconPreviewContentDialog()
        {
            InitializeComponent();
            SetValues();
        }

        private void SetValues()
        {
            var textDict = new Dictionary<WeatherType, TextBlock>()
            {
                { WeatherType.Sunny,         Text_Sunny },              { WeatherType.PartlyCloudy,  Text_PartlyCloudy },
                { WeatherType.Cloudy,        Text_Cloudy },             { WeatherType.Foggy,         Text_Foggy },
                { WeatherType.Drizzle,       Text_Drizzle },            { WeatherType.HeavyDrizzle,  Text_HeavyDrizzle },
                { WeatherType.Rain,          Text_Rain },               { WeatherType.HeavyRain,     Text_HeavyRain },
                { WeatherType.SnowFall,      Text_SnowFall },           { WeatherType.HeavySnowFall, Text_HeavySnowFall },
                { WeatherType.Thunderstorm,  Text_Thunderstorm },       { WeatherType.Unknown,       Text_Unknown },
            };
            var colorIcon1Dict = new Dictionary<WeatherType, SvgViewbox>()
            {
                { WeatherType.Sunny,         ColorIcon1_Sunny },        { WeatherType.PartlyCloudy,  ColorIcon1_PartlyCloudy },
                { WeatherType.Cloudy,        ColorIcon1_Cloudy },       { WeatherType.Foggy,         ColorIcon1_Foggy },
                { WeatherType.Drizzle,       ColorIcon1_Drizzle },      { WeatherType.HeavyDrizzle,  ColorIcon1_HeavyDrizzle },
                { WeatherType.Rain,          ColorIcon1_Rain },         { WeatherType.HeavyRain,     ColorIcon1_HeavyRain },
                { WeatherType.SnowFall,      ColorIcon1_SnowFall },     { WeatherType.HeavySnowFall, ColorIcon1_HeavySnowFall },
                { WeatherType.Thunderstorm,  ColorIcon1_Thunderstorm }, { WeatherType.Unknown,       ColorIcon1_Unknown },
            };
            var colorIcon2Dict = new Dictionary<WeatherType, SvgViewbox>()
            {
                { WeatherType.Sunny,         ColorIcon2_Sunny },        { WeatherType.PartlyCloudy,  ColorIcon2_PartlyCloudy },
                { WeatherType.Cloudy,        ColorIcon2_Cloudy },       { WeatherType.Foggy,         ColorIcon2_Foggy },
                { WeatherType.Drizzle,       ColorIcon2_Drizzle },      { WeatherType.HeavyDrizzle,  ColorIcon2_HeavyDrizzle },
                { WeatherType.Rain,          ColorIcon2_Rain },         { WeatherType.HeavyRain,     ColorIcon2_HeavyRain },
                { WeatherType.SnowFall,      ColorIcon2_SnowFall },     { WeatherType.HeavySnowFall, ColorIcon2_HeavySnowFall },
                { WeatherType.Thunderstorm,  ColorIcon2_Thunderstorm }, { WeatherType.Unknown,       ColorIcon2_Unknown },
            };
            var textIcon1Dict = new Dictionary<WeatherType, FontIcon>()
            {
                { WeatherType.Sunny,         TextIcon1_Sunny },        { WeatherType.PartlyCloudy,  TextIcon1_PartlyCloudy },
                { WeatherType.Cloudy,        TextIcon1_Cloudy },       { WeatherType.Foggy,         TextIcon1_Foggy },
                { WeatherType.Drizzle,       TextIcon1_Drizzle },      { WeatherType.HeavyDrizzle,  TextIcon1_HeavyDrizzle },
                { WeatherType.Rain,          TextIcon1_Rain },         { WeatherType.HeavyRain,     TextIcon1_HeavyRain },
                { WeatherType.SnowFall,      TextIcon1_SnowFall },     { WeatherType.HeavySnowFall, TextIcon1_HeavySnowFall },
                { WeatherType.Thunderstorm,  TextIcon1_Thunderstorm }, { WeatherType.Unknown,       TextIcon1_Unknown },
            };
            var textIcon2Dict = new Dictionary<WeatherType, FontIcon>()
            {
                { WeatherType.Sunny,         TextIcon2_Sunny },        { WeatherType.PartlyCloudy,  TextIcon2_PartlyCloudy },
                { WeatherType.Cloudy,        TextIcon2_Cloudy },       { WeatherType.Foggy,         TextIcon2_Foggy },
                { WeatherType.Drizzle,       TextIcon2_Drizzle },      { WeatherType.HeavyDrizzle,  TextIcon2_HeavyDrizzle },
                { WeatherType.Rain,          TextIcon2_Rain },         { WeatherType.HeavyRain,     TextIcon2_HeavyRain },
                { WeatherType.SnowFall,      TextIcon2_SnowFall },     { WeatherType.HeavySnowFall, TextIcon2_HeavySnowFall },
                { WeatherType.Thunderstorm,  TextIcon2_Thunderstorm }, { WeatherType.Unknown,       TextIcon2_Unknown },
            };

            foreach (var (type, ctrl) in textDict)
            {
                var d = WeatherDisplayType.Text;
                ctrl.SetBinding(TextBlock.TextProperty, "Text");
                ctrl.DataContext = new TextData(d.ToText(type));
            }
            foreach (var (type, ctrl) in colorIcon1Dict)
            {
                var d = WeatherDisplayType.ColorIcon1;
                ctrl.SetBinding(SvgViewbox.SourceProperty, "Source");
                ctrl.DataContext = new SvgData(d.ToSvgPath(type));
            }
            foreach (var (type, ctrl) in colorIcon2Dict)
            {
                var d = WeatherDisplayType.ColorIcon2;
                ctrl.SetBinding(SvgViewbox.SourceProperty, "Source");
                ctrl.DataContext = new SvgData(d.ToSvgPath(type));
            }
            foreach (var (type, ctrl) in textIcon1Dict)
            {
                var d = WeatherDisplayType.TextIcon1;
                ctrl.SetBinding(FontIcon.GlyphProperty, "Glyph");
                ctrl.SetBinding(FontIcon.FontFamilyProperty, "FontFamily");
                ctrl.DataContext = new FontIconData(d.ToText(type), d.ToTextIconFont());
            }
            foreach (var (type, ctrl) in textIcon2Dict)
            {
                var d = WeatherDisplayType.TextIcon2;
                ctrl.SetBinding(FontIcon.GlyphProperty, "Glyph");
                ctrl.SetBinding(FontIcon.FontFamilyProperty, "FontFamily");
                ctrl.DataContext = new FontIconData(d.ToText(type), d.ToTextIconFont());
            }
        }

        public record TextData(string Text);
        public record SvgData(string Source);
        public record FontIconData(string Glyph, string FontFamily);
    }
}
