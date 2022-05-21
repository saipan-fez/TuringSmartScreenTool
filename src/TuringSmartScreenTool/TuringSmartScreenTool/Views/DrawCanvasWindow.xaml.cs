using System.Windows;
using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.ViewModels.Pages;

namespace TuringSmartScreenTool.Views
{
    public partial class DrawCanvasWindow : Window
    {
        public DrawCanvasWindow(
            ILogger<DrawCanvasWindow> logger,
            CanvasEditorPageViewModel viewModel)
        {
            InitializeComponent();

            // TODO: fix
#if DEBUG

#endif

            ShowInTaskbar = false;
            Hide();

            DataContext = viewModel;
        }
    }
}
