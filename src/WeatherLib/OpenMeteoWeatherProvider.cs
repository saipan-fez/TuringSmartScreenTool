using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NodaTime.TimeZones;
using OpenMeteoLib;
using WeatherLib.Entities;

namespace WeatherLib
{
    public class OpenMeteoWeatherProvider : IWeatherProvider
    {
        private static readonly Anonymous2[] s_anonymous2Parameter = new Anonymous2[]
        {
            Anonymous2.Temperature_2m_max,
            Anonymous2.Temperature_2m_min,
            Anonymous2.Weathercode,
        };

        private readonly ILogger<OpenMeteoWeatherProvider> _logger;
        private readonly IOpenMeteoClient _openMeteoClient;

        public OpenMeteoWeatherProvider(
            ILogger<OpenMeteoWeatherProvider> logger,
            IOpenMeteoClient openMeteoClient)
        {
            _logger = logger;
            _openMeteoClient = openMeteoClient;
        }

        public async Task<WeatherData> GetWeatherAsync(Geocode geocode)
        {
            try
            {
                // get current timezone
                var timeZoneCollection = TzdbDateTimeZoneSource.Default.ZoneLocations;
                var timeZone = TzdbDateTimeZoneSource.Default.ZoneLocations
                    .FirstOrDefault(t => t.CountryCode == RegionInfo.CurrentRegion.TwoLetterISORegionName);

                _logger.LogDebug("weather requesting. Latitude:{Latitude} Longitude:{Longitude} ZoneId:{ZoneId}",
                    (float)geocode.Latitude,
                    (float)geocode.Longitude,
                    timeZone.ZoneId);

                // send request
                var response = await _openMeteoClient.ForecastAsync(
                    Array.Empty<Anonymous>(),
                    s_anonymous2Parameter,
                    (float)geocode.Latitude,
                    (float)geocode.Longitude,
                    true,
                    Temperature_unit.Celsius,
                    null,
                    Timeformat.Iso8601,
                    timeZone.ZoneId,
                    null);

                /*
                 * [Samples of response]
                 * == Daily ==
                 * {
                 *    "time": ["2022-05-02", "2022-05-03", "2022-05-04", "2022-05-05", "2022-05-06", "2022-05-07", "2022-05-08" ],
                 *    "weathercode": [80, 2, 3, 3, 3, 61 ],
                 *    "temperature_2m_max": [18.9, 18.1, 21.1, 22.2, 24.4, 22 ],
                 *    "temperature_2m_min": [ 8.5, 8.5, 9.3, 12.2, 12.9, 15.7 ]
                 * }
                 * == Current_weather ==
                 * {
                 *    "weathercode": 3,
                 *    "time": "2022-05-02T01:00",
                 *    "windspeed": 3,
                 *    "temperature": 10.6,
                 *    "winddirection": 5
                 *  }
                 */

                if (response.Daily is not JObject dailyJson)
                {
                    return null;
                }
                if (response.Current_weather is not JObject currentJson)
                {
                    return null;
                }

                var weatherCodes       = dailyJson["weathercode"].Values<int>();
                var minCelsiusTemps    = dailyJson["temperature_2m_min"].Values<double>();
                var maxCelsiusTemps    = dailyJson["temperature_2m_max"].Values<double>();
                var currentCelsiusTemp = currentJson["temperature"].Value<double>();

                return new()
                {
                    GeoCode                = new Geocode(response.Latitude, response.Longitude),
                    TodayWeather           = WeatherCodeToWeatherType(weatherCodes.ElementAt(0)),
                    TommorowWeather        = WeatherCodeToWeatherType(weatherCodes.ElementAt(1)),
                    TodayMinCelsiusTemp    = minCelsiusTemps.ElementAt(0),
                    TodayMaxCelsiusTemp    = maxCelsiusTemps.ElementAt(0),
                    TommorowMinCelsiusTemp = minCelsiusTemps.ElementAt(1),
                    TommorowMaxCelsiusTemp = maxCelsiusTemps.ElementAt(1),
                    CurrentCelsiusTemp     = currentCelsiusTemp
                };
            }
            catch (ApiException<Response2> ex)
            {
                _logger.LogError(ex, "open-meteo request error. Error:{Error} Reason:{Reason} Message:{Message}", ex.Result.Error, ex.Result.Reason, ex.Message);
                throw new WeatherException("open-meteo request error.", ex);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "open-meteo request error. {Message}", ex.Message);
                throw new WeatherException("open-meteo request error.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "invalid error.");
                throw new WeatherException("invalid error.", ex);
            }
        }

        private WeatherType WeatherCodeToWeatherType(int weatherCode)
        {
            /*
             * [Weather Codes]
             * | Code       | Description                                      |
             * | :--------- | :----------------------------------------------- |
             * | 0          | Clear sky                                        |
             * | 1, 2, 3    | Mainly clear, partly cloudy, and overcast        |
             * | 45, 48     | Fog and depositing rime fog                      |
             * | 51, 53, 55 | Drizzle: Light, moderate, and dense intensity    |
             * | 56, 57     | Freezing Drizzle: Light and dense intensity      |
             * | 61, 63, 65 | Rain: Slight, moderate and heavy intensity       |
             * | 66, 67     | Freezing Rain: Light and heavy intensity         |
             * | 71, 73, 75 | Snow fall: Slight, moderate, and heavy intensity |
             * | 77         | Snow grains                                      |
             * | 80, 81, 82 | Rain showers: Slight, moderate, and violent      |
             * | 85, 86     | Snow showers slight and heavy                    |
             * | 95 (*)     | Thunderstorm: Slight or moderate                 |
             * | 96, 99 (*) | Thunderstorm with slight and heavy hail          |
             * (*) Thunderstorm forecast with hail is only available in Central Europe
             *
             * ref: https://open-meteo.com/en/docs#weathervariables
             */

            return weatherCode switch
            {
                0 or 1                     => WeatherType.Sunny,
                2                          => WeatherType.PartlyCloudy,
                3                          => WeatherType.Cloudy,
                45 or 48                   => WeatherType.Fog,
                51 or 53 or 56             => WeatherType.Drizzle,
                55 or 57                   => WeatherType.HeavyDrizzle,
                61 or 63 or 66 or 80 or 81 => WeatherType.Rain,
                65 or 67 or 82             => WeatherType.HeavyRain,
                71 or 73 or 77 or 85       => WeatherType.SnowFall,
                75 or 86                   => WeatherType.HeavySnowFall,
                95 or 96 or 99             => WeatherType.Thunderstorm,
                _                          => WeatherType.Unknown,
            };
        }
    }
}
