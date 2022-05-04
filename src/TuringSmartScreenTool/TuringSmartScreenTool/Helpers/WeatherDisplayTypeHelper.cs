using TuringSmartScreenTool.ViewModels.Editors;
using WeatherLib.Entities;

namespace TuringSmartScreenTool.Helpers
{
    public static class WeatherDisplayTypeHelper
    {
        public static string ToTextIconFont(this WeatherDisplayType displayType)
        {
            return displayType switch
            {
                WeatherDisplayType.TextIcon1 => "/Assets/erikflowers_weather-icons/icon.ttf #Weather Icons",
                WeatherDisplayType.TextIcon2 => "/Assets/qwd_icons/icon.ttf #qweather-icons",
                _ => null
            };
        }

        public static string ToText(this WeatherDisplayType displayType, WeatherType weatherType)
        {
            return displayType switch
            {
                WeatherDisplayType.Text => ToText(weatherType),
                WeatherDisplayType.TextIcon1 => ToTextIcon1(weatherType),
                WeatherDisplayType.TextIcon2 => ToTextIcon2(weatherType),
                _ => ""
            };
        }

        public static string ToSvgPath(this WeatherDisplayType displayType, WeatherType weatherType)
        {
            return displayType switch
            {
                WeatherDisplayType.ColorIcon1 => ToColorIcon1SvgPath(weatherType),
                WeatherDisplayType.ColorIcon2 => ToColorIcon2SvgPath(weatherType),
                _ => null
            };
        }

        private static string ToColorIcon1SvgPath(WeatherType type)
        {
            return type switch
            {
                WeatherType.Sunny         => "/Assets/nrkno_yr-weather-symbols/01d.svg",
                WeatherType.PartlyCloudy  => "/Assets/nrkno_yr-weather-symbols/03d.svg",
                WeatherType.Cloudy        => "/Assets/nrkno_yr-weather-symbols/04.svg",
                WeatherType.Foggy         => "/Assets/nrkno_yr-weather-symbols/15.svg",
                WeatherType.Drizzle       => "/Assets/nrkno_yr-weather-symbols/09.svg",
                WeatherType.HeavyDrizzle  => "/Assets/nrkno_yr-weather-symbols/10.svg",
                WeatherType.Rain          => "/Assets/nrkno_yr-weather-symbols/09.svg",
                WeatherType.HeavyRain     => "/Assets/nrkno_yr-weather-symbols/10.svg",
                WeatherType.SnowFall      => "/Assets/nrkno_yr-weather-symbols/49.svg",
                WeatherType.HeavySnowFall => "/Assets/nrkno_yr-weather-symbols/50.svg",
                WeatherType.Thunderstorm  => "/Assets/nrkno_yr-weather-symbols/11.svg",
                WeatherType.Unknown       => null,
                _                         => null,
            };
        }

        private static string ToColorIcon2SvgPath(WeatherType type)
        {
            return type switch
            {
                WeatherType.Sunny         => "/Assets/basmilius_weather-icons/clear-day.svg",
                WeatherType.PartlyCloudy  => "/Assets/basmilius_weather-icons/partly-cloudy-day.svg",
                WeatherType.Cloudy        => "/Assets/basmilius_weather-icons/cloudy.svg",
                WeatherType.Foggy         => "/Assets/basmilius_weather-icons/fog.svg",
                WeatherType.Drizzle       => "/Assets/basmilius_weather-icons/drizzle.svg",
                WeatherType.HeavyDrizzle  => "/Assets/basmilius_weather-icons/extreme-drizzle.svg",
                WeatherType.Rain          => "/Assets/basmilius_weather-icons/rain.svg",
                WeatherType.HeavyRain     => "/Assets/basmilius_weather-icons/extreme-rain.svg",
                WeatherType.SnowFall      => "/Assets/basmilius_weather-icons/snow.svg",
                WeatherType.HeavySnowFall => "/Assets/basmilius_weather-icons/extreme-snow.svg",
                WeatherType.Thunderstorm  => "/Assets/basmilius_weather-icons/thunderstorms.svg",
                WeatherType.Unknown       => "/Assets/basmilius_weather-icons/not-available.svg",
                _                         => "/Assets/basmilius_weather-icons/not-available.svg",
            };
        }

        private static string ToText(WeatherType type)
        {
            // TODO: localize
            return type switch
            {
                WeatherType.Sunny         => "Sunny",
                WeatherType.PartlyCloudy  => "PartlyCloudy",
                WeatherType.Cloudy        => "Cloudy",
                WeatherType.Foggy         => "Foggy",
                WeatherType.Drizzle       => "Drizzle",
                WeatherType.HeavyDrizzle  => "HeavyDrizzle",
                WeatherType.Rain          => "Rain",
                WeatherType.HeavyRain     => "HeavyRain",
                WeatherType.SnowFall      => "SnowFall",
                WeatherType.HeavySnowFall => "HeavySnowFall",
                WeatherType.Thunderstorm  => "Thunderstorm",
                WeatherType.Unknown       => "N/A",
                _                         => "N/A",
            };
        }

        private static string ToTextIcon1(WeatherType type)
        {
            return type switch
            {
                WeatherType.Sunny         => "\U0000f00d",
                WeatherType.PartlyCloudy  => "\U0000f002",
                WeatherType.Cloudy        => "\U0000f013",
                WeatherType.Foggy         => "\U0000f014",
                WeatherType.Drizzle       => "\U0000f01a",
                WeatherType.HeavyDrizzle  => "\U0000f01a",
                WeatherType.Rain          => "\U0000f019",
                WeatherType.HeavyRain     => "\U0000f019",
                WeatherType.SnowFall      => "\U0000f01b",
                WeatherType.HeavySnowFall => "\U0000f01b",
                WeatherType.Thunderstorm  => "\U0000f01e",
                WeatherType.Unknown       => "\U0000f07b",
                _                         => "\U0000f07b",
            };
        }

        private static string ToTextIcon2(WeatherType type)
        {
            return type switch
            {
                WeatherType.Sunny         => "\U0000f101",
                WeatherType.PartlyCloudy  => "\U0000f102",
                WeatherType.Cloudy        => "\U0000f105",
                WeatherType.Foggy         => "\U0000f135",
                WeatherType.Drizzle       => "\U0000f113",
                WeatherType.HeavyDrizzle  => "\U0000f113",
                WeatherType.Rain          => "\U0000f110",
                WeatherType.HeavyRain     => "\U0000f111",
                WeatherType.SnowFall      => "\U0000f121",
                WeatherType.HeavySnowFall => "\U0000f122",
                WeatherType.Thunderstorm  => "\U0000f10d",
                WeatherType.Unknown       => "\U0000f146",
                _                         => "\U0000f146",
            };
        }
    }
}
