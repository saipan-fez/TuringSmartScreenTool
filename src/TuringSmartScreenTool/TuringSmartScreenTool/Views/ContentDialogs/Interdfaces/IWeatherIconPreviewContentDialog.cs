using System.Threading.Tasks;
using ModernWpf.Controls;

namespace TuringSmartScreenTool.Views.ContentDialogs.Interdfaces
{
    public interface IWeatherIconPreviewContentDialog
    {
        Task<ContentDialogResult> ShowAsync();
    }
}
