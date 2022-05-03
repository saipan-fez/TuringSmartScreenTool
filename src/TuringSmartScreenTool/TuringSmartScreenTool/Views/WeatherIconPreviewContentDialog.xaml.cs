using System.Threading.Tasks;
using ModernWpf.Controls;

namespace TuringSmartScreenTool.Views
{
    public partial class WeatherIconPreviewContentDialog : ContentDialog, IWeatherIconPreviewContentDialog
    {
        public WeatherIconPreviewContentDialog()
        {
            InitializeComponent();
        }
    }

    public interface IWeatherIconPreviewContentDialog
    {
        Task<ContentDialogResult> ShowAsync();
    }
}
