﻿using System;
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
        Icon1,
        Icon2
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

        public WeatherTextEditorViewModel(
            // TODO: usecase
            IWeatherManager weatherManager,
            ILocationSelectContentDialog locationSelectContentDialog)
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

            Text = Observable.CombineLatest(
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

                // TODO: localize
                return target.Value switch
                {
                    WeatherType.Unknown => "Unknown",
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
                    _ => "Unknown",
                };
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
