using WeatherLib.Entities;

namespace TuringSmartScreenTool.Controllers.Interfaces
{

    public interface IWeatherManager
    {
        IWeatherInfo Get(Geocode geocode);
    }
}
