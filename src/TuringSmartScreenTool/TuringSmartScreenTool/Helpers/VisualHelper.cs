using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;

namespace TuringSmartScreenTool.Helpers
{
    internal static class VisualHelper
    {
        public static void CopyVisualToBgraMat(Visual visual, Mat<Vec4b> bgraMat)
        {
            visual.Dispatcher.Invoke(() =>
            {
                var bounds = VisualTreeHelper.GetDescendantBounds(visual);
                var render = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);
                var darwingVisual = new DrawingVisual();
                using (var context = darwingVisual.RenderOpen())
                {
                    var brush = new VisualBrush(visual);
                    context.DrawRectangle(brush, null, bounds);
                }
                render.Render(darwingVisual);
                render.Freeze();

                var rect = new Int32Rect()
                {
                    X = 0,
                    Y = 0,
                    Width = render.PixelWidth,
                    Height = render.PixelHeight
                };
                render.CopyPixels(rect, bgraMat.Data, bgraMat.Width * bgraMat.Height * 4, bgraMat.Width * 4);
            });
        }
    }
}
