using Microsoft.Extensions.DependencyInjection;
using OpenMeteoLib;

namespace WeatherLib
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWeatherServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient<IOpenMeteoClient, OpenMeteoClient>(httpClient =>
            {
                return new OpenMeteoClient("https://api.open-meteo.com/", httpClient);
            });

            return serviceCollection
                .AddTransient<IWeatherProvider, OpenMeteoWeatherProvider>()
                .AddTransient<IGeocoder, Geocoder>();
        }
    }
}
