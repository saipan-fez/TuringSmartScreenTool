using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nominatim.API.Geocoders;
using Nominatim.API.Models;
using WeatherLib.Entities;

namespace WeatherLib
{
    public class Geocoder : IGeocoder
    {
        private readonly ILogger<Geocoder> _logger;

        public Geocoder(ILogger<Geocoder> logger)
        {
            _logger = logger;
        }

        public async Task<Geocode> SearchAsync(RegionInfo country, string state, string city)
        {
            try
            {
                var x = new ForwardGeocoder();
                var req = new ForwardGeocodeRequest()
                {
                    CountryCodeSearch = country.TwoLetterISORegionName,
                    Country = country.DisplayName,
                    State = state,
                    City = city,
                    ShowGeoJSON = true
                };

                _logger.LogTrace("geocode requesting. CountryCodeSearch:{CountryCodeSearch} Country:{Country} State:{State} City:{City}",
                    req.CountryCodeSearch,
                    req.Country,
                    req.State,
                    req.City);

                var res = await x.Geocode(req);
                if (res.Length > 0)
                {
                    var latitude = res[0].Latitude;
                    var longitude = res[0].Longitude;
                    _logger.LogTrace("geocode request succeed. latitude:{latitude} longitude:{longitude}", latitude, longitude);

                    return new(latitude, longitude);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "geocoging error.");
                return null;
            }
        }
    }
}
