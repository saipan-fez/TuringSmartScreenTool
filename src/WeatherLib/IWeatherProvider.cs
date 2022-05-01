using System.Threading.Tasks;
using WeatherLib.Entities;

namespace WeatherLib
{
    public interface IWeatherProvider
    {
        Task<WeatherData> GetWeatherAsync(Geocode geocode);
    }
}
