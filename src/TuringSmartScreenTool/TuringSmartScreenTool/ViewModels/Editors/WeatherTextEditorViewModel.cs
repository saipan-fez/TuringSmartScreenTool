using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Helpers;
using TuringSmartScreenTool.Views;
using WeatherLib.Entities;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public enum WeatherInfoType
    {
        TodayWeather,
        TomorrowWeather,
        CurrentTemperature,
        TodayMaxTemperature,
        TodayMinTemperature,
        TomorrowMaxTemperature,
        TomorrowMinTemperature,
    }

    public enum WeatherDisplayType
    {
        Text,
        TextIcon1,
        TextIcon2,
        ColorIcon1,
        ColorIcon2,
    }

    public enum TemperatureUnit
    {
        Celsius,
        CelsiusWithUnit,
        Fahrenheit,
        FahrenheitWithUnit,
    }

    public class WeatherTextEditorViewModel : BaseTextBlockEditorViewModel
    {
        private readonly ReactiveProperty<IWeatherInfo> _weatherInfo = new();
        private readonly ReadOnlyReactiveProperty<WeatherType?> _todayWeather;
        private readonly ReadOnlyReactiveProperty<WeatherType?> _tommorowWeather;
        private readonly ReadOnlyReactiveProperty<double?> _todayMinCelsiusTemp;
        private readonly ReadOnlyReactiveProperty<double?> _todayMaxCelsiusTemp;
        private readonly ReadOnlyReactiveProperty<double?> _tommorowMinCelsiusTemp;
        private readonly ReadOnlyReactiveProperty<double?> _tommorowMaxCelsiusTemp;
        private readonly ReadOnlyReactiveProperty<double?> _currentCelsiusTemp;

        public override ReactiveProperty<string> Name { get; } = new("Weather");
        public override ReadOnlyReactiveProperty<string> Text { get; }

        public ReadOnlyReactiveProperty<bool> IsText { get; }
        public ReadOnlyReactiveProperty<bool> IsTextIcon { get; }
        public ReadOnlyReactiveProperty<bool> IsSvgIcon { get; }
        public ReadOnlyReactiveProperty<string> TextIconFont { get; }
        public ReadOnlyReactiveProperty<string> SvgPath { get; }

        public ReactiveProperty<double?> Latitude { get; } = new();
        public ReactiveProperty<double?> Longitude { get; } = new();
        public ReadOnlyReactiveProperty<bool> IsLocationInputed { get; }

        public IEnumerable<WeatherInfoType> DisplayWeatherInfoTypeCollection { get; } = Enum.GetValues(typeof(WeatherInfoType)).Cast<WeatherInfoType>();
        public ReactiveProperty<WeatherInfoType> DisplayWeatherInfoType { get; } = new(WeatherInfoType.TodayWeather);

        public ReadOnlyReactiveProperty<bool> CanSelectWeatherType { get; }
        public IEnumerable<WeatherDisplayType> WeatherDisplayTypeCollection { get; } = Enum.GetValues(typeof(WeatherDisplayType)).Cast<WeatherDisplayType>();
        public ReactiveProperty<WeatherDisplayType> SelectedWeatherDisplayType { get; } = new(WeatherDisplayType.Text);

        public ReadOnlyReactiveProperty<bool> CanSelectTemperatureUnit { get; }
        public IEnumerable<TemperatureUnit> TemperatureUnitCollection { get; } = Enum.GetValues(typeof(TemperatureUnit)).Cast<TemperatureUnit>();
        public ReactiveProperty<TemperatureUnit> SelectedTemperatureUnit { get; } = new(TemperatureUnit.CelsiusWithUnit);

        public ICommand SelectGeolocationCommand { get; }
        public ICommand ShowWeatherIconPreviewCommand { get; }

        public WeatherTextEditorViewModel(
            // TODO: usecase
            IWeatherManager weatherManager,
            ILocationSelectContentDialog locationSelectContentDialog,
            IWeatherIconPreviewContentDialog weatherIconPreviewContentDialog)
        {
            _todayWeather = _weatherInfo
                .Select(x => x?.TodayWeather ?? Observable.Empty<WeatherType?>())
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            _tommorowWeather = _weatherInfo
                .Select(x => x?.TommorowWeather ?? Observable.Empty<WeatherType?>())
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            _todayMinCelsiusTemp = _weatherInfo
                .Select(x => x?.TodayMinCelsiusTemp ?? Observable.Empty<double?>())
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            _todayMaxCelsiusTemp = _weatherInfo
                .Select(x => x?.TodayMaxCelsiusTemp ?? Observable.Empty<double?>())
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            _tommorowMinCelsiusTemp = _weatherInfo
                .Select(x => x?.TommorowMinCelsiusTemp ?? Observable.Empty<double?>())
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            _tommorowMaxCelsiusTemp = _weatherInfo
                .Select(x => x?.TommorowMaxCelsiusTemp ?? Observable.Empty<double?>())
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            _currentCelsiusTemp = _weatherInfo
                .Select(x => x?.CurrentCelsiusTemp ?? Observable.Empty<double?>())
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsLocationInputed = Latitude
                .CombineLatest(Longitude, (la, lo) => la.HasValue && lo.HasValue)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            CanSelectWeatherType =
                Observable.CombineLatest(
                    IsLocationInputed,
                    DisplayWeatherInfoType,
                    (isLocationInputed, weatherInfoType) => isLocationInputed && IsWeather(weatherInfoType))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            CanSelectTemperatureUnit =
                Observable.CombineLatest(
                    IsLocationInputed,
                    DisplayWeatherInfoType,
                    (isLocationInputed, weatherInfoType) => isLocationInputed && IsTemperature(weatherInfoType))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsSvgIcon =
                Observable.CombineLatest(
                    DisplayWeatherInfoType,
                    SelectedWeatherDisplayType,
                    (wetherInfoType, weatherDisplayType) =>
                    {
                        return
                            (wetherInfoType == WeatherInfoType.TodayWeather || wetherInfoType == WeatherInfoType.TomorrowWeather) &&
                            (weatherDisplayType == WeatherDisplayType.ColorIcon1 || weatherDisplayType == WeatherDisplayType.ColorIcon2);
                    })
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsTextIcon =
                Observable.CombineLatest(
                    DisplayWeatherInfoType,
                    SelectedWeatherDisplayType,
                    (wetherInfoType, weatherDisplayType) =>
                    {
                        return
                            (wetherInfoType == WeatherInfoType.TodayWeather || wetherInfoType == WeatherInfoType.TomorrowWeather) &&
                            (weatherDisplayType == WeatherDisplayType.TextIcon1 || weatherDisplayType == WeatherDisplayType.TextIcon2);
                    })
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsText =
                Observable.CombineLatest(
                    IsSvgIcon,
                    IsTextIcon,
                    (s, t) => !s && !t)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            Text =
                Observable.CombineLatest(
                    DisplayWeatherInfoType,
                    SelectedWeatherDisplayType,
                    SelectedTemperatureUnit,
                    _todayWeather,
                    _tommorowWeather,
                    _todayMinCelsiusTemp,
                    _todayMaxCelsiusTemp,
                    _tommorowMinCelsiusTemp,
                    _tommorowMaxCelsiusTemp,
                    _currentCelsiusTemp,
                    ConvertToText)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            TextIconFont = SelectedWeatherDisplayType
                .Select(x => ConvertToTextIconFont(x))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            SvgPath =
                Observable.CombineLatest(
                    DisplayWeatherInfoType,
                    SelectedWeatherDisplayType,
                    _todayWeather,
                    _tommorowWeather,
                    ConvertToSvgPath)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            SelectGeolocationCommand = new RelayCommand(async () =>
            {
                var result = await locationSelectContentDialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                    return;

                var latitude = locationSelectContentDialog.Latitude;
                var longitude = locationSelectContentDialog.Longitude;
                if (latitude.HasValue && longitude.HasValue)
                {
                    Latitude.Value  = latitude;
                    Longitude.Value = longitude;
                    _weatherInfo.Value = weatherManager.Get(new Geocode(latitude.Value, longitude.Value));
                }
            });

            ShowWeatherIconPreviewCommand = new RelayCommand(() =>
            {
                weatherIconPreviewContentDialog.ShowAsync();
            });
        }

        string ConvertToTextIconFont(WeatherDisplayType d)
        {
            if (d == WeatherDisplayType.TextIcon1)
            {
                return "/Assets/erikflowers_weather-icons/icon.ttf #Weather Icons";
            }
            else if (d == WeatherDisplayType.TextIcon2)
            {
                return "/Assets/qwd_icons/icon.ttf #qweather-icons";
            }
            else
            {
                return null;
            }
        }

        string ConvertToSvgPath(
            WeatherInfoType w, WeatherDisplayType d,
            WeatherType? todayWeather, WeatherType? tommorowWeather)
        {
            if (w == WeatherInfoType.TodayWeather)
            {
                if (d == WeatherDisplayType.ColorIcon1)
                {
                    return ToColorIcon1SvgPath(todayWeather);
                }
                else if (d == WeatherDisplayType.ColorIcon2)
                {
                    return ToColorIcon2SvgPath(todayWeather);
                }
            }
            else if (w == WeatherInfoType.TomorrowWeather)
            {
                if (d == WeatherDisplayType.ColorIcon1)
                {
                    return ToColorIcon1SvgPath(tommorowWeather);
                }
                else if (d == WeatherDisplayType.ColorIcon2)
                {
                    return ToColorIcon2SvgPath(tommorowWeather);
                }
            }
            return null;

            string ToColorIcon1SvgPath(WeatherType? type)
            {
                if (!type.HasValue)
                    return null;

                return type.Value switch
                {
                    WeatherType.Sunny => "/Assets/nrkno_yr-weather-symbols/01d.svg",
                    WeatherType.PartlyCloudy => "/Assets/nrkno_yr-weather-symbols/03d.svg",
                    WeatherType.Cloudy => "/Assets/nrkno_yr-weather-symbols/04.svg",
                    WeatherType.Foggy => "/Assets/nrkno_yr-weather-symbols/15.svg",
                    WeatherType.Drizzle => "/Assets/nrkno_yr-weather-symbols/09.svg",
                    WeatherType.HeavyDrizzle => "/Assets/nrkno_yr-weather-symbols/10.svg",
                    WeatherType.Rain => "/Assets/nrkno_yr-weather-symbols/09.svg",
                    WeatherType.HeavyRain => "/Assets/nrkno_yr-weather-symbols/10.svg",
                    WeatherType.SnowFall => "/Assets/nrkno_yr-weather-symbols/49.svg",
                    WeatherType.HeavySnowFall => "/Assets/nrkno_yr-weather-symbols/50.svg",
                    WeatherType.Thunderstorm => "/Assets/nrkno_yr-weather-symbols/11.svg",
                    WeatherType.Unknown => null,
                    _ => null,
                };
            }

            string ToColorIcon2SvgPath(WeatherType? type)
            {
                if (!type.HasValue)
                    return null;

                return type.Value switch
                {
                    WeatherType.Sunny => "/Assets/basmilius_weather-icons/clear-day.svg",
                    WeatherType.PartlyCloudy => "/Assets/basmilius_weather-icons/partly-cloudy-day.svg",
                    WeatherType.Cloudy => "/Assets/basmilius_weather-icons/cloudy.svg",
                    WeatherType.Foggy => "/Assets/basmilius_weather-icons/fog.svg",
                    WeatherType.Drizzle => "/Assets/basmilius_weather-icons/drizzle.svg",
                    WeatherType.HeavyDrizzle => "/Assets/basmilius_weather-icons/extreme-drizzle.svg",
                    WeatherType.Rain => "/Assets/basmilius_weather-icons/rain.svg",
                    WeatherType.HeavyRain => "/Assets/basmilius_weather-icons/extreme-rain.svg",
                    WeatherType.SnowFall => "/Assets/basmilius_weather-icons/snow.svg",
                    WeatherType.HeavySnowFall => "/Assets/basmilius_weather-icons/extreme-snow.svg",
                    WeatherType.Thunderstorm => "/Assets/basmilius_weather-icons/thunderstorms.svg",
                    WeatherType.Unknown => "/Assets/basmilius_weather-icons/not-available.svg",
                    _ => "/Assets/basmilius_weather-icons/not-available.svg",
                };
            }
        }

        string ConvertToText(
            WeatherInfoType w, WeatherDisplayType d, TemperatureUnit t,
            WeatherType? todayWeather, WeatherType? tommorowWeather,
            double? todayMinCelsiusTemp, double? todayMaxCelsiusTemp,
            double? tommorowMinCelsiusTemp, double? tommorowMaxCelsiusTemp,
            double? currentCelsiusTemp)
        {
            string ToWeatherText(WeatherType? target)
            {
                if (!target.HasValue)
                    return "";

                if (d == WeatherDisplayType.ColorIcon1 ||
                    d == WeatherDisplayType.ColorIcon2)
                    return "";

                return d switch
                {
                    WeatherDisplayType.Text => ToText(),
                    WeatherDisplayType.TextIcon1 => ToTextIcon1(),
                    WeatherDisplayType.TextIcon2 => ToTextIcon2(),
                    _ => ""
                };

                string ToText()
                {
                    // TODO: localize
                    return target.Value switch
                    {
                        WeatherType.Sunny => "Sunny",
                        WeatherType.PartlyCloudy => "PartlyCloudy",
                        WeatherType.Cloudy => "Cloudy",
                        WeatherType.Foggy => "Foggy",
                        WeatherType.Drizzle => "Drizzle",
                        WeatherType.HeavyDrizzle => "HeavyDrizzle",
                        WeatherType.Rain => "Rain",
                        WeatherType.HeavyRain => "HeavyRain",
                        WeatherType.SnowFall => "SnowFall",
                        WeatherType.HeavySnowFall => "HeavySnowFall",
                        WeatherType.Thunderstorm => "Thunderstorm",
                        WeatherType.Unknown => "N/A",
                        _ => "N/A",
                    };
                }
                string ToTextIcon1()
                {
                    return target.Value switch
                    {
                        WeatherType.Sunny => "\U0000f00d",
                        WeatherType.PartlyCloudy => "\U0000f002",
                        WeatherType.Cloudy => "\U0000f013",
                        WeatherType.Foggy => "\U0000f014",
                        WeatherType.Drizzle => "\U0000f01a",
                        WeatherType.HeavyDrizzle => "\U0000f01a",
                        WeatherType.Rain => "\U0000f019",
                        WeatherType.HeavyRain => "\U0000f019",
                        WeatherType.SnowFall => "\U0000f01b",
                        WeatherType.HeavySnowFall => "\U0000f01b",
                        WeatherType.Thunderstorm => "\U0000f01e",
                        WeatherType.Unknown => "\U0000f07b",
                        _ => "\U0000f07b",
                    };
                }
                string ToTextIcon2()
                {
                    return target.Value switch
                    {
                        WeatherType.Sunny => "\U0000f101",
                        WeatherType.PartlyCloudy => "\U0000f102",
                        WeatherType.Cloudy => "\U0000f105",
                        WeatherType.Foggy => "\U0000f135",
                        WeatherType.Drizzle => "\U0000f113",
                        WeatherType.HeavyDrizzle => "\U0000f113",
                        WeatherType.Rain => "\U0000f110",
                        WeatherType.HeavyRain => "\U0000f111",
                        WeatherType.SnowFall => "\U0000f121",
                        WeatherType.HeavySnowFall => "\U0000f122",
                        WeatherType.Thunderstorm => "\U0000f10d",
                        WeatherType.Unknown => "\U0000f146",
                        _ => "\U0000f146",
                    };
                }
            }

            string ToTemperatureText(double? target)
            {
                if (!target.HasValue)
                    return "";

                var (unit, includeUnit) = t switch
                {
                    TemperatureUnit.Celsius => (ThermalUnit.Celsius, false),
                    TemperatureUnit.CelsiusWithUnit => (ThermalUnit.Celsius, true),
                    TemperatureUnit.Fahrenheit => (ThermalUnit.Fahrenheit, false),
                    TemperatureUnit.FahrenheitWithUnit => (ThermalUnit.Fahrenheit, true),
                    _ => throw new InvalidOperationException()
                };

                return ValueToStringHelper.CelsiusToThermalString(target.Value, unit, "F0", includeUnit);
            }

            return w switch
            {
                WeatherInfoType.TodayWeather => ToWeatherText(todayWeather),
                WeatherInfoType.TomorrowWeather => ToWeatherText(tommorowWeather),
                WeatherInfoType.TodayMinTemperature => ToTemperatureText(todayMinCelsiusTemp),
                WeatherInfoType.TodayMaxTemperature => ToTemperatureText(todayMaxCelsiusTemp),
                WeatherInfoType.TomorrowMinTemperature => ToTemperatureText(tommorowMinCelsiusTemp),
                WeatherInfoType.TomorrowMaxTemperature => ToTemperatureText(tommorowMaxCelsiusTemp),
                WeatherInfoType.CurrentTemperature => ToTemperatureText(currentCelsiusTemp),
                _ => throw new InvalidOperationException(),
            };
        }

        private static bool IsWeather(WeatherInfoType type)
        {
            return type switch
            {
                WeatherInfoType.TodayWeather => true,
                WeatherInfoType.TomorrowWeather => true,
                _ => false
            };
        }

        private static bool IsTemperature(WeatherInfoType type)
        {
            return type switch
            {
                WeatherInfoType.CurrentTemperature => true,
                WeatherInfoType.TodayMinTemperature => true,
                WeatherInfoType.TodayMaxTemperature => true,
                WeatherInfoType.TomorrowMinTemperature => true,
                WeatherInfoType.TomorrowMaxTemperature => true,
                _ => false
            };
        }
    }
}
