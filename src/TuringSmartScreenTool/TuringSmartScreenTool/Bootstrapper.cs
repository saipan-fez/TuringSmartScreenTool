using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TuringSmartScreenLibrary;
using TuringSmartScreenTool.Controllers;
using TuringSmartScreenTool.UseCases;
using TuringSmartScreenTool.ViewModels;
using TuringSmartScreenTool.Views;

namespace TuringSmartScreenTool
{
    internal static class Bootstrapper
    {
        public static IServiceCollection AddServicesForApp(this IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddNLog(GetNLogConfig().FullName);
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            services.AddTuringSmartScreenServices();

            services.AddTransient<MainWindow>();
            services.AddTransient<CanvasEditorWindow>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<CanvasEditorWindowViewModel>();
            services.AddTransient<IFindScreenDeviceUseCase, FindScreenDeviceUseCase>();
            services.AddTransient<IControlScreenDeviceUseCase, ControlScreenDeviceUseCase>();
            services.AddTransient<IUpdateScreenUseCase, UpdateScreenUseCase>();
            services.AddSingleton<IScreenDeviceManager, ScreenDeviceManager>();

            return services;
        }

        private static FileInfo GetNLogConfig()
        {
            var exeFullPath = Assembly.GetExecutingAssembly().Location;
            var exeDir = Path.GetDirectoryName(exeFullPath);

            var nlogFileName =
#if DEBUG
                "NLog.Debug.config";
#else
                "NLog.Release.config";
#endif

            return new FileInfo(Path.Combine(exeDir, nlogFileName));
        }
    }
}
