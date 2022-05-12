using System;
using System.Threading.Tasks;
using ModernWpf.Controls;
using TuringSmartScreenTool.ViewModels.ContentDialogs;
using TuringSmartScreenTool.Views.ContentDialogs.Interdfaces;

namespace TuringSmartScreenTool.Views.ContentDialogs
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
}
