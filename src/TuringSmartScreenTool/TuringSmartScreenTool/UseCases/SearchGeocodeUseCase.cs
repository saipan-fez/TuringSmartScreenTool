using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.UseCases.Interfaces;
using WeatherLib;
using WeatherLib.Entities;

namespace TuringSmartScreenTool.UseCases
{
    public class SearchGeocodeUseCase : ISearchGeocodeUseCase
    {
        private readonly ILogger<SearchGeocodeUseCase> _logger;
        private readonly IGeocoder _geocoder;

        public SearchGeocodeUseCase(
            ILogger<SearchGeocodeUseCase> logger,
            IGeocoder geocoder)
        {
            _logger = logger;
            _geocoder = geocoder;
        }

        public async Task<Geocode> SearchAsync(RegionInfo country, string state, string city)
        {
            return await _geocoder.SearchAsync(country, state, city);
        }
    }
}
