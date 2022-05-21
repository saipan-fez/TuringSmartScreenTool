using System.Windows.Navigation;
using Microsoft.Extensions.Logging;
using ModernWpf.Controls;
using TuringSmartScreenTool.ViewModels.Pages;

namespace TuringSmartScreenTool.Views.Pages
{
    public partial class DeviceControlPage : Page
    {
        private readonly ILogger<DeviceControlPage> _logger;
        private readonly DeviceControlPageViewModel _viewModel;

        public DeviceControlPage(
            ILogger<DeviceControlPage> logger,
            DeviceControlPageViewModel viewModel)
        {
            _logger = logger;
            _viewModel = viewModel;

            InitializeComponent();
            DataContext = _viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _viewModel.OnNavigatedTo(default);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _viewModel.OnNavigatedFrom(default);
        }
    }
}
