using ModernWpf.Controls;
using TuringSmartScreenTool.ViewModels.ContentDialogs;
using TuringSmartScreenTool.Views.ContentDialogs.Interdfaces;

namespace TuringSmartScreenTool.Views.ContentDialogs
{
    public partial class LocationSelectContentDialog : ContentDialog, ILocationSelectContentDialog
    {
        private readonly LocationSelectContentDialogViewModel _viewModel;

        public double? Latitude => _viewModel.Latitude.Value;
        public double? Longitude => _viewModel.Longitude.Value;

        public LocationSelectContentDialog(
            LocationSelectContentDialogViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            _viewModel = viewModel;
        }
    }
}
