using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TuringSmartScreenLibrary;
using TuringSmartScreenTool.Controllers;
using TuringSmartScreenTool.Controllers.Interfaces;
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
            services.AddTransient<HardwareSelectContentDialogViewModel>();
            services.AddTransient<LocationSelectContentDialogViewModel>();
            services.AddTransient<IHardwareSelectContentDialog, HardwareSelectContentDialog>();
            services.AddTransient<ILocationSelectContentDialog, LocationSelectContentDialog>();
            services.AddTransient<IWeatherIconPreviewContentDialog, WeatherIconPreviewContentDialog>();

            // UseCase
            services.AddTransient<IFindScreenDeviceUseCase, FindScreenDeviceUseCase>();
            services.AddTransient<IControlScreenDeviceUseCase, ControlScreenDeviceUseCase>();
            services.AddTransient<IUpdateScreenUseCase, UpdateScreenUseCase>();

            // Registered as Singleton
            services.AddSingleton<HardwareMonitor>();
            services.AddSingleton<IHardwareMonitorController>(s => s.GetService<HardwareMonitor>());
            services.AddSingleton<ISensorFinder>(s => s.GetService<HardwareMonitor>());
            services.AddSingleton<IHardwareFinder>(s => s.GetService<HardwareMonitor>());
            services.AddSingleton<IValueUpdateManager, ValueUpdateManager>();
            services.AddSingleton<IScreenDeviceManager, ScreenDeviceManager>();
            services.AddSingleton<ITimeManager, TimeManager>();
            services.AddSingleton<IWeatherManager, WeatherManager>();
            services.AddTransient<IEditorFileManager, EditorFileManager>();

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
