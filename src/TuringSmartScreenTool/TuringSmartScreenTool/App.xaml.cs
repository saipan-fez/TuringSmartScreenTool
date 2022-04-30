using System;
using System.Reactive.Concurrency;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using TuringSmartScreenTool.Views;

namespace TuringSmartScreenTool
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<App> _logger;

        public App(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = serviceProvider.GetService<ILogger<App>>();

            RegisterUnhandledException();

            InitializeComponent();
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            ReactivePropertyScheduler.SetDefaultSchedulerFactory(() => new DispatcherScheduler(Dispatcher));

            var mainWindow = _serviceProvider.GetService<CanvasEditorWindow>();
            mainWindow.Show();
        }

        private void RegisterUnhandledException()
        {
            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            {
                _logger.LogError(e.Exception, "");
            };
            DispatcherUnhandledException += (s, e) =>
            {
                _logger.LogError(e.Exception, "");
            };
        }
    }
}
