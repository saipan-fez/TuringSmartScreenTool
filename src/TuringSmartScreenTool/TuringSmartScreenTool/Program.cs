using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var app = new App(serviceProvider);
                app.Run();
            }
        }
    }
}
