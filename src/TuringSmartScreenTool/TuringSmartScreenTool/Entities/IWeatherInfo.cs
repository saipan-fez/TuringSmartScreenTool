using System;
using WeatherLib.Entities;

namespace TuringSmartScreenTool.Controllers.Interfaces
{
    public interface IWeatherInfo
    {
        public Geocode GeoCode { get; }
        public IObservable<WeatherType?> TodayWeather { get; }
        public IObservable<WeatherType?> TommorowWeather { get; }
        public IObservable<double?> TodayMinCelsiusTemp { get; }
        public IObservable<double?> TodayMaxCelsiusTemp { get; }
        public IObservable<double?> TommorowMinCelsiusTemp { get; }
        public IObservable<double?> TommorowMaxCelsiusTemp { get; }
        public IObservable<double?> CurrentCelsiusTemp { get; }
    }
}
