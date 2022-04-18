using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TuringSmartScreenTool.Views;

namespace TuringSmartScreenTool
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            InitializeComponent();
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<CanvasEditorWindow>();
            mainWindow.Show();
        }
    }
}
