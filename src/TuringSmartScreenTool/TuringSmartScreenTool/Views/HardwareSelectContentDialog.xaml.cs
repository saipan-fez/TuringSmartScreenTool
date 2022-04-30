using System;
using System.Threading.Tasks;
using ModernWpf.Controls;
using TuringSmartScreenTool.ViewModels;

namespace TuringSmartScreenTool.Views
{
    public partial class HardwareSelectContentDialog : ContentDialog, IHardwareSelectContentDialog
    {
        private readonly HardwareSelectContentDialogViewModel _viewModel;

        public string SelectedId => _viewModel.SelectedMonitorTarget.Value?.Id ?? null;

        public HardwareSelectContentDialog(
            HardwareSelectContentDialogViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            _viewModel = viewModel;
        }

        public async Task<ContentDialogResult> ShowAsync(HardwareSelectType type)
        {
            _viewModel.Mode.Value = type switch
            {
                HardwareSelectType.Sensor => HardwareSelectContentDialogViewModel.SelectionMode.Sensor,
                HardwareSelectType.Hardware => HardwareSelectContentDialogViewModel.SelectionMode.Hardware,
                _ => throw new InvalidOperationException()
            };
            return await ShowAsync();
        }
    }

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
