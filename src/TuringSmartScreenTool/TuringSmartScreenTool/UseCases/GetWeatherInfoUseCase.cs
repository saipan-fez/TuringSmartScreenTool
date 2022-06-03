using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.UseCases.Interfaces;
using WeatherLib.Entities;

namespace TuringSmartScreenTool.UseCases
{
    public class GetWeatherInfoUseCase : IGetWeatherInfoUseCase
    {
        private readonly ILogger<GetWeatherInfoUseCase> _logger;
        private readonly IWeatherManager _weatherManager;

        public GetWeatherInfoUseCase(
            ILogger<GetWeatherInfoUseCase> logger,
            IWeatherManager weatherManager)
        {
            _logger = logger;
            _weatherManager = weatherManager;
        }

        public IWeatherInfo Get(Geocode geocode)
        {
            return _weatherManager.Get(geocode);
        }
    }
}
