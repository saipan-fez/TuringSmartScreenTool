using System.Threading.Tasks;
using ModernWpf.Controls;

namespace TuringSmartScreenTool.Views.ContentDialogs.Interdfaces
{
    public enum HardwareSelectType
    {
        Hardware,
        Sensor
    }

    public interface IHardwareSelectContentDialog
    {
        string SelectedId { get; }

        Task<ContentDialogResult> ShowAsync(HardwareSelectType type);
    }
}
