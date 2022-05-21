using Microsoft.Extensions.DependencyInjection;
using System;
using TuringSmartScreenTool.Controllers.Interfaces;
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
                var hardwareMonitor = serviceProvider.GetService<IHardwareMonitorController>();
                hardwareMonitor.StartToInitialize();

                var app = new App(serviceProvider);
                app.Run();
            }
        }
    }
}
