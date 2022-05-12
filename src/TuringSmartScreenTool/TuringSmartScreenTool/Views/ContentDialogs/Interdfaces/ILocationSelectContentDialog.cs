using System.Threading.Tasks;
using ModernWpf.Controls;

namespace TuringSmartScreenTool.Views.ContentDialogs.Interdfaces
{
    public interface ILocationSelectContentDialog
    {
        double? Latitude { get; }
        double? Longitude { get; }

        Task<ContentDialogResult> ShowAsync();
    }
}
