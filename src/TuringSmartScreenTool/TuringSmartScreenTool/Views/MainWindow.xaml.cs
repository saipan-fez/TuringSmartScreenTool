using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernWpf.Controls;
using TuringSmartScreenTool.Views.Pages;

namespace TuringSmartScreenTool.Views
{
    public sealed partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(
            ILogger<MainWindow> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            InitializeComponent();

            RootNavigationView.SelectedItem = RootNavigationView.MenuItems.OfType<NavigationViewItem>().First();
        }

        private void NavigationView_SelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem is not FrameworkElement elem)
                return;

            Page navigatePage = elem.Tag switch
            {
                "DeviceControl" => _serviceProvider.GetService<DeviceControlPage>(),
                "CanvasEditor"  => _serviceProvider.GetService<CanvasEditorPage>(),
                "Setting"       => _serviceProvider.GetService<DeviceControlPage>(),
                _               => throw new InvalidOperationException()
            };

            ContentFrame.Navigate(navigatePage);
        }
    }
}
