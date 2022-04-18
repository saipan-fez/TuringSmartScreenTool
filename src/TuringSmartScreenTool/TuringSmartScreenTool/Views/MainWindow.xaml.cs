using System.Windows;
using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.ViewModels;

namespace TuringSmartScreenTool.Views
{
    public sealed partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow> _logger;

        public MainWindow(
            ILogger<MainWindow> logger,
            MainWindowViewModel mainWindowViewModel)
        {
            _logger = logger;
            InitializeComponent();

            DataContext = mainWindowViewModel;

            //var timer = new System.Windows.Threading.DispatcherTimer()
            //{
            //    Interval = System.TimeSpan.FromSeconds(0.2),
            //    IsEnabled = true
            //};
            //timer.Tick += (s, e) =>
            //{
            //    TimeTextBlock.Text = System.DateTime.Now.ToString("HH:mm:ss");
            //};
            //timer.Start();
        }
    }
}
