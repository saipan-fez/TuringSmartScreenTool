using TuringSmartScreenTool.Controllers.Interfaces;
using WeatherLib.Entities;

namespace TuringSmartScreenTool.UseCases.Interfaces
{
    public interface IGetWeatherInfoUseCase
    {
        IWeatherInfo Get(Geocode geocode);
    }
}
