using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.ViewModels;
using TuringSmartScreenTool.ViewModels.Editors;

namespace TuringSmartScreenTool.Views
{
    public partial class CanvasEditorWindow : Window
    {
        private readonly ILogger<CanvasEditorWindow> _logger;
        private readonly CanvasEditorWindowViewModel _mainWindowViewModel;

        public CanvasEditorWindow(
            ILogger<CanvasEditorWindow> logger,
            CanvasEditorWindowViewModel mainWindowViewModel)
        {
            _logger = logger;
            _mainWindowViewModel = mainWindowViewModel;
            InitializeComponent();

            DataContext = mainWindowViewModel;
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        // Any mouse events handled by Thumb, so ListBox selected item not changed.
        // For that reason, change selected item by myself when raise PreviewMouseDown event.
        private void Editor_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement elem)
                return;
            if (elem.DataContext is not BaseEditorViewModel vm)
                return;

            var index = _mainWindowViewModel.EditorViewModels.IndexOf(vm);
            if (index != -1)
                _mainWindowViewModel.SelectedEditorViewModelIndex.Value = index;
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: System.Runtime.InteropServices.COMException
            //       Right数値をキーボードで変更してこのイベントを発生させると例外が発生する（原因不明）
            _mainWindowViewModel.SelectedEditorViewModelIndex.Value = -1;
        }
    }
}
