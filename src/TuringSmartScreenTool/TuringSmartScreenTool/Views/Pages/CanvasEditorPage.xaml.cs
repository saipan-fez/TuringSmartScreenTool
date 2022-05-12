using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using ModernWpf.Controls;
using TuringSmartScreenTool.ViewModels.Editors;
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

        // Any mouse events handled by Thumb, so ListBox selected item not changed.
        // For that reason, change selected item by myself when raise PreviewMouseDown event.
        private void Editor_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement elem)
                return;
            if (elem.DataContext is not BaseEditorViewModel vm)
                return;

            var index = _viewModel.EditorViewModels.IndexOf(vm);
            if (index != -1)
                _viewModel.SelectedEditorViewModelIndex.Value = index;
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // FIXME: System.Runtime.InteropServices.COMException
            // Right数値をキーボードで変更してこのイベントを発生させると例外が発生する（原因不明）
            _viewModel.SelectedEditorViewModelIndex.Value = -1;
        }
    }
}
