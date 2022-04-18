using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.ViewModels
{
    public abstract class CommonEditorViewModel : IDisposable
    {
        protected readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ReactiveProperty<string> Id { get; } = new(Guid.NewGuid().ToString());
        public abstract ReactiveProperty<string> Name { get; }

        public ReactiveProperty<HorizontalAlignment> CanvasHorizontalAlignment { get; } = new(HorizontalAlignment.Left);
        public ReactiveProperty<VerticalAlignment> CanvasVerticalAlignment { get; } = new(VerticalAlignment.Top);

        public ReactiveProperty<double?> InputCanvasLeft { get; } = new(0d);
        public ReactiveProperty<double?> InputCanvasRight { get; } = new(0d);
        public ReactiveProperty<double?> InputCanvasTop { get; } = new(0d);
        public ReactiveProperty<double?> InputCanvasBottom { get; } = new(0d);

        public ReadOnlyReactiveProperty<double?> CanvasLeft { get; }
        public ReadOnlyReactiveProperty<double?> CanvasRight { get; }
        public ReadOnlyReactiveProperty<double?> CanvasTop { get; }
        public ReadOnlyReactiveProperty<double?> CanvasBottom { get; }

        public ReactiveProperty<bool> IsAutoSize { get; } = new(true);
        public ReactiveProperty<double?> InputWidth { get; } = new(100d);
        public ReactiveProperty<double?> InputHeight { get; } = new(100d);
        public ReadOnlyReactiveProperty<double?> Width { get; }
        public ReadOnlyReactiveProperty<double?> Height { get; }

        public ReactiveCommand<DragStartedEventArgs> DragStartedCommand { get; }
        public ReactiveCommand<DragCompletedEventArgs> DragCompletedCommand { get; }
        public ReactiveCommand<DragDeltaEventArgs> DragDeltaCommand { get; }

        public CommonEditorViewModel()
        {
            CanvasLeft = Observable
                .CombineLatest(
                    CanvasHorizontalAlignment,
                    InputCanvasLeft,
                    (a, v) => a == HorizontalAlignment.Left ? v : null)
                .ToReadOnlyReactiveProperty();
            CanvasRight = Observable
                .CombineLatest(
                    CanvasHorizontalAlignment,
                    InputCanvasRight,
                    (a, v) => a == HorizontalAlignment.Right ? v : null)
                .ToReadOnlyReactiveProperty();
            CanvasTop = Observable
                .CombineLatest(
                    CanvasVerticalAlignment,
                    InputCanvasTop,
                    (a, v) => a == VerticalAlignment.Top ? v : null)
                .ToReadOnlyReactiveProperty();
            CanvasBottom = Observable
                .CombineLatest(
                    CanvasVerticalAlignment,
                    InputCanvasBottom,
                    (a, v) => a == VerticalAlignment.Bottom ? v : null)
                .ToReadOnlyReactiveProperty();

            Width = Observable
                .CombineLatest(
                    IsAutoSize,
                    InputWidth,
                    (auto, v) => auto ? null : v)
                .ToReadOnlyReactiveProperty();
            Height = Observable
                .CombineLatest(
                    IsAutoSize,
                    InputHeight,
                    (auto, v) => auto ? null : v)
                .ToReadOnlyReactiveProperty();

            DragStartedCommand = new ReactiveCommand<DragStartedEventArgs>()
                .WithSubscribe(e => e.Handled = false)
                .AddTo(_disposables);
            DragCompletedCommand = new ReactiveCommand<DragCompletedEventArgs>()
                .WithSubscribe(e => e.Handled = false)
                .AddTo(_disposables);
            DragDeltaCommand = new ReactiveCommand<DragDeltaEventArgs>()
                .WithSubscribe(e =>
                {
                    if (e is null)
                        return;

                    if (CanvasHorizontalAlignment.Value == HorizontalAlignment.Left)
                    {
                        if (InputCanvasLeft.Value.HasValue)
                            InputCanvasLeft.Value += e.HorizontalChange;
                    }
                    else
                    {
                        if (InputCanvasRight.Value.HasValue)
                            InputCanvasRight.Value -= e.HorizontalChange;
                    }

                    if (CanvasVerticalAlignment.Value == VerticalAlignment.Top)
                    {
                        if (InputCanvasTop.Value.HasValue)
                            InputCanvasTop.Value += e.VerticalChange;
                    }
                    else
                    {
                        if (InputCanvasBottom.Value.HasValue)
                            InputCanvasBottom.Value -= e.VerticalChange;
                    }

                    // not handled for listbox selected event
                    e.Handled = false;
                })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        protected void SelectColor(ReactiveProperty<Color> target)
        {
            var c = target.Value;
            var colorDialog = new System.Windows.Forms.ColorDialog()
            {
                Color = System.Drawing.Color.FromArgb(c.R, c.G, c.B),
                FullOpen = true,
                AnyColor = false
            };
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var newColor = colorDialog.Color;
                target.Value = Color.FromRgb(newColor.R, newColor.G, newColor.B);
            }
        }

        protected void SelectImageFilePath(ReactiveProperty<string> target)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "Image|*.png;*.jpg;*.jpeg;*.bmp;*.gif" + "|All Files|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Multiselect = false
            };
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                File.Exists(fileDialog.FileName))
            {
                target.Value = fileDialog.FileName;
            }
        }
    }
}
