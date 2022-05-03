using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reactive.Bindings;
using TuringSmartScreenTool.Controllers.Interfaces;
using WeatherLib;
using WeatherLib.Entities;

namespace TuringSmartScreenTool.Controllers
{
    public class WeatherParameter
    {
        public TimeSpan Interval { get; init; } = TimeSpan.FromHours(3);
    }

    public class WeatherManager : IDisposable, IWeatherManager
    {
        private class WeatherInfo : IWeatherInfo
        {
            private readonly ReactiveProperty<WeatherType?> _todayWeather;
            private readonly ReactiveProperty<WeatherType?> _tommorowWeather;
            private readonly ReactiveProperty<double?> _todayMinCelsiusTemp;
            private readonly ReactiveProperty<double?> _todayMaxCelsiusTemp;
            private readonly ReactiveProperty<double?> _tommorowMinCelsiusTemp;
            private readonly ReactiveProperty<double?> _tommorowMaxCelsiusTemp;
            private readonly ReactiveProperty<double?> _currentCelsiusTemp;

            public Geocode GeoCode { get; }
            public IObservable<WeatherType?> TodayWeather => _todayWeather;
            public IObservable<WeatherType?> TommorowWeather => _tommorowWeather;
            public IObservable<double?> TodayMinCelsiusTemp => _todayMinCelsiusTemp;
            public IObservable<double?> TodayMaxCelsiusTemp => _todayMaxCelsiusTemp;
            public IObservable<double?> TommorowMinCelsiusTemp => _tommorowMinCelsiusTemp;
            public IObservable<double?> TommorowMaxCelsiusTemp => _tommorowMaxCelsiusTemp;
            public IObservable<double?> CurrentCelsiusTemp => _currentCelsiusTemp;

            public WeatherInfo(Geocode g)
            {
                GeoCode = g;
                _todayWeather = new((WeatherType?)null);
                _tommorowWeather = new((WeatherType?)null);
                _todayMinCelsiusTemp = new((double?)null);
                _todayMaxCelsiusTemp = new((double?)null);
                _tommorowMinCelsiusTemp = new((double?)null);
                _tommorowMaxCelsiusTemp = new((double?)null);
                _currentCelsiusTemp = new((double?)null);
            }

            public void Update(WeatherData w)
            {
                _todayWeather.Value = w?.TodayWeather;
                _tommorowWeather.Value = w?.TommorowWeather;
                _todayMinCelsiusTemp.Value = w?.TodayMinCelsiusTemp;
                _todayMaxCelsiusTemp.Value = w?.TodayMaxCelsiusTemp;
                _tommorowMinCelsiusTemp.Value = w?.TommorowMinCelsiusTemp;
                _tommorowMaxCelsiusTemp.Value = w?.TommorowMaxCelsiusTemp;
                _currentCelsiusTemp.Value = w?.CurrentCelsiusTemp;
            }
        }

        private readonly ILogger<WeatherManager> _logger;
        private readonly IWeatherProvider _weatherProvider;
        private readonly IValueUpdateManager _valueUpdateManager;
        private readonly WeatherParameter _weatherParameter;

        private readonly Dictionary<Geocode, (string id, WeatherInfo info)> _idDictionary = new();
        private readonly Stopwatch _stopwatch = new();

        public WeatherManager(
            ILogger<WeatherManager> logger,
            IWeatherProvider weatherProvider,
            IValueUpdateManager valueUpdateManager,
            IOptions<WeatherParameter> weatherParameter)
        {
            _logger = logger;
            _weatherProvider = weatherProvider;
            _valueUpdateManager = valueUpdateManager;
            _weatherParameter = weatherParameter.Value;
        }

        public void Dispose()
        {
            foreach (var pair in _idDictionary)
            {
                _valueUpdateManager.Unregister(pair.Value.id);
            }
        }

        public IWeatherInfo Get(Geocode geocode)
        {
            if (_idDictionary.TryGetValue(geocode, out var val))
            {
                return val.info;
            }

            var weatherInfo = new WeatherInfo(geocode);

            var id = _valueUpdateManager.Register(
                nameof(WeatherManager),
                _weatherParameter.Interval,
                UpdateWeatherAsync);

            _idDictionary.Add(geocode, (id, weatherInfo));

            return weatherInfo;
        }

        private async Task UpdateWeatherAsync(CancellationToken token)
        {
            _stopwatch.Restart();
            var start = _stopwatch.Elapsed;

            await Parallel.ForEachAsync(_idDictionary, token, async (pair, t) =>
            {
                var geocode = pair.Key;
                var weatherData = await _weatherProvider.GetWeatherAsync(geocode, t);

                pair.Value.info.Update(weatherData);
            });

            var elapsed = _stopwatch.Elapsed - start;
            _logger.LogTrace("Weather updated. time:{ms}ms", elapsed.TotalMilliseconds);
        }
    }
}
