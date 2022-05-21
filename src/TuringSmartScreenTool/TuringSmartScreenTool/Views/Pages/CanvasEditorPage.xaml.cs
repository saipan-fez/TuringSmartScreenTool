using System.Windows.Navigation;
using Microsoft.Extensions.Logging;
using ModernWpf.Controls;
using TuringSmartScreenTool.ViewModels.Pages;

namespace TuringSmartScreenTool.Views.Pages
{
    public partial class CanvasEditorPage : Page
    {
        private readonly ILogger<CanvasEditorPage> _logger;
        private readonly CanvasEditorPageViewModel _viewModel;

        public CanvasEditorPage(
            ILogger<CanvasEditorPage> logger,
            CanvasEditorPageViewModel viewModel)
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
