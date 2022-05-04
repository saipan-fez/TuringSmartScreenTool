using System.Threading.Tasks;
using ModernWpf.Controls;
using TuringSmartScreenTool.ViewModels;

namespace TuringSmartScreenTool.Views
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

    public interface ILocationSelectContentDialog
    {
        double? Latitude { get; }
        double? Longitude { get; }

        Task<ContentDialogResult> ShowAsync();
    }
}
