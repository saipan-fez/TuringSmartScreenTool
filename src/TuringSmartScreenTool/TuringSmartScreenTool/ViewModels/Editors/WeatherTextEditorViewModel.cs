using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.Helpers;
using TuringSmartScreenTool.Views;
using WeatherLib.Entities;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public enum WeatherInfoType
    {
        // TODO: DisplayAttribute
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
        // TODO: DisplayAttribute
        Text,
        TextIcon1,
        TextIcon2,
        ColorIcon1,
        ColorIcon2,
    }

    public enum TemperatureUnit
    {
        // TODO: DisplayAttribute
        Celsius,
        CelsiusWithUnit,
        Fahrenheit,
        FahrenheitWithUnit,
    }

    public class WeatherTextEditorViewModel : BaseTextBlockEditorViewModel
    {
        private readonly ReadOnlyReactiveProperty<IWeatherInfo> _weatherInfo;
        private readonly ReadOnlyReactiveProperty<WeatherType?> _todayWeather;
        private readonly ReadOnlyReactiveProperty<WeatherType?> _tommorowWeather;
        private readonly ReadOnlyReactiveProperty<double?> _todayMinCelsiusTemp;
        private readonly ReadOnlyReactiveProperty<double?> _todayMaxCelsiusTemp;
        private readonly ReadOnlyReactiveProperty<double?> _tommorowMinCelsiusTemp;
        private readonly ReadOnlyReactiveProperty<double?> _tommorowMaxCelsiusTemp;
        private readonly ReadOnlyReactiveProperty<double?> _currentCelsiusTemp;

        public override ReactiveProperty<string> Name { get; } = new("Weather");
        public override ReadOnlyReactiveProperty<string> Text { get; }
        public override IReadOnlyReactiveProperty<bool> CanSelectFontFamily { get; }
        public override IReadOnlyReactiveProperty<bool> CanSelectForeground { get; }
        public override IReadOnlyReactiveProperty<bool> CanSelectFontSize { get; }
        public override IReadOnlyReactiveProperty<bool> CanSelectFontWeight { get; }

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
            _weatherInfo =
                Observable.CombineLatest(
                    Latitude,
                    Longitude,
                    (latitude, longitude) =>
                    {
                        if (latitude.HasValue && longitude.HasValue)
                            return weatherManager.Get(new Geocode(latitude.Value, longitude.Value));
                        else
                            return null;
                    })
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

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

            CanSelectFontFamily =
                Observable.CombineLatest(
                    IsTextIcon,
                    IsSvgIcon,
                    (t, s) => !t && !s)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            CanSelectFontSize = IsSvgIcon
                .Select(x => !x)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            CanSelectForeground = IsSvgIcon
                .Select(x => !x)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            CanSelectFontWeight = IsSvgIcon
                .Select(x => !x)
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
                    Latitude.Value  = null;
                    Longitude.Value = null;

                    Latitude.Value  = latitude;
                    Longitude.Value = longitude;
                }
            });

            ShowWeatherIconPreviewCommand = new RelayCommand(() =>
            {
                weatherIconPreviewContentDialog.ShowAsync();
            });
        }

        private string ConvertToTextIconFont(WeatherDisplayType d)
        {
            return d.ToTextIconFont();
        }

        private string ConvertToSvgPath(
            WeatherInfoType w, WeatherDisplayType d,
            WeatherType? todayWeather, WeatherType? tommorowWeather)
        {
            if (w == WeatherInfoType.TodayWeather)
            {
                if (todayWeather.HasValue)
                    return d.ToSvgPath(todayWeather.Value);
            }
            else if (w == WeatherInfoType.TomorrowWeather)
            {
                if (tommorowWeather.HasValue)
                    return d.ToSvgPath(tommorowWeather.Value);
            }
            return null;
        }

        private string ConvertToText(
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

                return d.ToText(target.Value);
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

        #region IEditor
        public class WeatherTextEditorViewModelParameter
        {
            public static readonly string Key = "WeatherText";

            [JsonProperty]
            public double? Latitude { get; init; } = null;
            [JsonProperty]
            public double? Longitude { get; init; } = null;
            [JsonProperty]
            [JsonConverter(typeof(StringEnumConverter))]
            public WeatherInfoType DisplayWeatherInfoType { get; init; } = WeatherInfoType.TodayWeather;
            [JsonProperty]
            [JsonConverter(typeof(StringEnumConverter))]
            public WeatherDisplayType SelectedWeatherDisplayType { get; init; } = WeatherDisplayType.Text;
            [JsonProperty]
            [JsonConverter(typeof(StringEnumConverter))]
            public TemperatureUnit SelectedTemperatureUnit { get; init; } = TemperatureUnit.CelsiusWithUnit;
        }

        public override async Task<JObject> SaveAsync(SaveAccessory accessory)
        {
            var jobject = await base.SaveAsync(accessory);
            var param = new WeatherTextEditorViewModelParameter()
            {
                Latitude                   = Latitude.Value,
                Longitude                  = Longitude.Value,
                DisplayWeatherInfoType     = DisplayWeatherInfoType.Value,
                SelectedWeatherDisplayType = SelectedWeatherDisplayType.Value,
                SelectedTemperatureUnit    = SelectedTemperatureUnit.Value,
            };
            jobject[WeatherTextEditorViewModelParameter.Key] = JToken.FromObject(param);

            return jobject;
        }

        public override async Task LoadAsync(LoadAccessory accessory, JObject jobject)
        {
            await base.LoadAsync(accessory, jobject);

            if (!jobject.TryGetValue(WeatherTextEditorViewModelParameter.Key, out var val))
                return;

            var param = val.ToObject<WeatherTextEditorViewModelParameter>();
            if (param is null)
                return;

            Latitude.Value                   = param.Latitude;
            Longitude.Value                  = param.Longitude;
            DisplayWeatherInfoType.Value     = param.DisplayWeatherInfoType;
            SelectedWeatherDisplayType.Value = param.SelectedWeatherDisplayType;
            SelectedTemperatureUnit.Value    = param.SelectedTemperatureUnit;
        }
        #endregion
    }
}
