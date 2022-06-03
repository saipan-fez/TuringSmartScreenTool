using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TuringSmartScreenLibrary;
using TuringSmartScreenTool.Controllers;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Helpers;
using TuringSmartScreenTool.UseCases;
using TuringSmartScreenTool.UseCases.Interfaces;
using TuringSmartScreenTool.ViewModels.ContentDialogs;
using TuringSmartScreenTool.ViewModels.Controls;
using TuringSmartScreenTool.ViewModels.Editors;
using TuringSmartScreenTool.ViewModels.Pages;
using TuringSmartScreenTool.Views;
using TuringSmartScreenTool.Views.ContentDialogs;
using TuringSmartScreenTool.Views.ContentDialogs.Interdfaces;
using TuringSmartScreenTool.Views.Pages;

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

            // UI
            services.AddTransient<MainWindow>();
            services.AddSingleton<CanvasEditorPage>();
            services.AddSingleton<DeviceControlPage>();

            // ViewModel
            services.AddTransient<DeviceControlPageViewModel>();
            services.AddTransient<CanvasEditorPageViewModel>();
            services.AddTransient<HardwareSelectContentDialogViewModel>();
            services.AddTransient<LocationSelectContentDialogViewModel>();
            services.AddTransient<CanvasEditorListViewModel>();
            services.AddTransient<StaticTextBlockEditorViewModel>();
            services.AddTransient<ImageEditorViewModel>();
            services.AddTransient<HardwareNameTextBlockEditorViewModel>();
            services.AddTransient<HardwareSensorTextBlockEditorViewModel>();
            services.AddTransient<HardwareSensorIndicatorEditorViewModel>();
            services.AddTransient<DateTimeTextEditorViewModel>();
            services.AddTransient<WeatherTextEditorViewModel>();

            // ContentDialog
            services.AddTransient<IHardwareSelectContentDialog, HardwareSelectContentDialog>();
            services.AddTransient<ILocationSelectContentDialog, LocationSelectContentDialog>();
            services.AddTransient<IWeatherIconPreviewContentDialog, WeatherIconPreviewContentDialog>();

            // UseCase
            services.AddTransient<IControlScreenDeviceUseCase, ControlScreenDeviceUseCase>();
            services.AddTransient<IEditCanvasUseCase, EditCanvasUseCase>();
            services.AddTransient<IFindHardwareInfoUseCase, FindHardwareInfoUseCase>();
            services.AddTransient<IFindSensorInfoUseCase, FindSensorInfoUseCase>();
            services.AddTransient<IGetMonitorTargetsUseCase, GetMonitorTargetsUseCase>();
            services.AddTransient<IGetTimeDataUseCase, GetTimeDataUseCase>();
            services.AddTransient<IGetWeatherInfoUseCase, GetWeatherInfoUseCase>();
            services.AddTransient<ISearchGeocodeUseCase, SearchGeocodeUseCase>();

            // Controller
            //   Registered as Singleton
            services.AddSingleton<HardwareMonitor>();
            services.AddSingleton<IHardwareMonitorController>(s => s.GetService<HardwareMonitor>());
            services.AddSingleton<ISensorFinder>(s => s.GetService<HardwareMonitor>());
            services.AddSingleton<IHardwareFinder>(s => s.GetService<HardwareMonitor>());
            //   Registered as Singleton
            services.AddSingleton<IValueUpdateManager, ValueUpdateManager>();
            //   Registered as Singleton
            services.AddSingleton<IScreenDeviceManager, ScreenDeviceManager>();
            //   Registered as Singleton
            services.AddSingleton<ITimeManager, TimeManager>();
            //   Registered as Singleton
            services.AddSingleton<IWeatherManager, WeatherManager>();
            services.AddTransient<IEditorFileManager, EditorFileManager>();

            return services;
        }

        private static FileInfo GetNLogConfig()
        {
            var nlogFileName =
#if DEBUG
                "NLog.Debug.config";
#else
                "NLog.Release.config";
#endif
            // If a config file exists in the appdata folder, use it preferentially.
            var appDataDir = DirectoryInfoHelper.GetApplicationDataDirectory();
            var addtionalNlogFile = new FileInfo(Path.Combine(appDataDir.FullName, nlogFileName));
            if (addtionalNlogFile.Exists)
            {
                return addtionalNlogFile;
            }
            else
            {
                var assemblyDir = DirectoryInfoHelper.GetCurrentAssemblyDirectory();
                return new FileInfo(Path.Combine(assemblyDir.FullName, nlogFileName));
            }
        }
    }
}
