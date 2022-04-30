using Microsoft.Extensions.DependencyInjection;
using System;
using TuringSmartScreenTool.Controllers;

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
                .AddServicesForApp();

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
