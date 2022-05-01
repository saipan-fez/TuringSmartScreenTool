using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime.TimeZones;
using Nominatim.API.Geocoders;
using Nominatim.API.Models;
using OpenMeteoLib;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TuringSmartScreenTool.Controllers;
using WeatherLib;

namespace TuringSmartScreenTool
{
    /// <summary>
    /// Program class
    /// </summary>
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                .AddServicesForApp()
                .AddWeatherServices();

            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                // TODO: async
                var hardwareMonitor = serviceProvider.GetService<IHardwareMonitorController>();
                hardwareMonitor.InitializeAsync().AsTask().Wait();

                var app = new App(serviceProvider);
                app.Run();
            }
        }
    }
}
