using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TuringSmartScreenTool.ViewModels.Editors;
using TuringSmartScreenTool.ViewModels.Pages;

namespace TuringSmartScreenTool.Views.Controls
{
    public partial class CanvasEditorListBox : ListBox
    {
        public CanvasEditorListBox()
        {
            InitializeComponent();
        }

        // Any mouse events handled by Thumb, so ListBox selected item not changed.
        // For that reason, change selected item by myself when raise PreviewMouseDown event.
        private void Editor_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement elem)
                return;
            if (elem.DataContext is not BaseEditorViewModel vm)
                return;

            if (DataContext is not CanvasEditorPageViewModel viewModel)
                return;

            var index = viewModel.EditorViewModels.IndexOf(vm);
            if (index != -1)
                viewModel.SelectedEditorViewModelIndex.Value = index;
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not CanvasEditorPageViewModel viewModel)
                return;

            // TODO: System.Runtime.InteropServices.COMException
            // Right数値をキーボードで変更してこのイベントを発生させると例外が発生する（原因不明）
            viewModel.SelectedEditorViewModelIndex.Value = -1;
        }
    }
}
