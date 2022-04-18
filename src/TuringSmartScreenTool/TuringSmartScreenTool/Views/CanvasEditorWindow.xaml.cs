using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.ViewModels;

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

        // Any mouse events handled by Thumb, so ListBox selected item not changed.
        // For that reason, change selected item by myself when raise PreviewMouseDown event.
        private void Editor_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement elem)
                return;
            if (elem.DataContext is not CommonEditorViewModel vm)
                return;

            var index = _mainWindowViewModel.EditorViewModels.IndexOf(vm);
            if (index != -1)
                _mainWindowViewModel.SelectedEditorViewModelIndex.Value = index;
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mainWindowViewModel.SelectedEditorViewModelIndex.Value = -1;
        }
    }
}
