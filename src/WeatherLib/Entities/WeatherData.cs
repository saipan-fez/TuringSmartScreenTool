namespace WeatherLib.Entities
{
    public class WeatherData
    {
        public Geocode GeoCode { get; init; }
        public WeatherType TodayWeather { get; init; }
        public WeatherType TommorowWeather { get; init; }
        public double TodayMinCelsiusTemp { get; init; }
        public double TodayMaxCelsiusTemp { get; init; }
        public double TommorowMinCelsiusTemp { get; init; }
        public double TommorowMaxCelsiusTemp { get; init; }
        public double CurrentCelsiusTemp { get; init; }
    }
}
