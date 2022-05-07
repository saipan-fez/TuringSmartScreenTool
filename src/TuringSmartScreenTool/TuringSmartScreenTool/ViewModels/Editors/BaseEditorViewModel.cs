using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.ViewModels.Editors
{
    public class SaveAccessory
    {
        private readonly DirectoryInfo _assetsDirectory;

        public SaveAccessory(DirectoryInfo assetsDirectory)
        {
            _assetsDirectory = assetsDirectory;
        }

        public string SaveAssetFile(string srcFilePath)
        {
            if (string.IsNullOrEmpty(srcFilePath) || !File.Exists(srcFilePath))
                return null;

            var destFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(srcFilePath);
            var destFileFullPath = Path.Combine(_assetsDirectory.FullName, destFileName);

            File.Copy(srcFilePath, destFileFullPath);

            return $"/{_assetsDirectory.Name}/{destFileName}";
        }
    }

    public class LoadAccessory
    {
        private readonly DirectoryInfo _directory;

        public LoadAccessory(DirectoryInfo directory)
        {
            _directory = directory;
        }

        public string GetFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            var replacedFilePath = filePath.Replace('/', Path.DirectorySeparatorChar);
            var fileFullPath = _directory.FullName + replacedFilePath;
            if (string.IsNullOrEmpty(fileFullPath) || !File.Exists(fileFullPath))
                return null;

            return fileFullPath;
        }
    }

    public interface IEditor
    {
        Task<JObject> SaveAsync(SaveAccessory accessory);
        Task LoadAsync(LoadAccessory accessory, JObject jobject);
    }

    public abstract class BaseEditorViewModel : IDisposable, IEditor
    {
        protected readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ReactiveProperty<string> Id { get; } = new(Guid.NewGuid().ToString());

        public abstract ReactiveProperty<string> Name { get; }
        public virtual bool IsAutoSizeSupported => true;

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

        public ReactiveProperty<bool> IsAutoSize { get; }
        public ReactiveProperty<double?> InputWidth { get; } = new(100d);
        public ReactiveProperty<double?> InputHeight { get; } = new(100d);
        public ReadOnlyReactiveProperty<double?> Width { get; }
        public ReadOnlyReactiveProperty<double?> Height { get; }

        public ReactiveCommand<DragStartedEventArgs> DragStartedCommand { get; }
        public ReactiveCommand<DragCompletedEventArgs> DragCompletedCommand { get; }
        public ReactiveCommand<DragDeltaEventArgs> DragDeltaCommand { get; }

        public BaseEditorViewModel()
        {
            CanvasLeft = Observable
                .CombineLatest(
                    CanvasHorizontalAlignment,
                    InputCanvasLeft,
                    (a, v) => a == HorizontalAlignment.Left ? v : null)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            CanvasRight = Observable
                .CombineLatest(
                    CanvasHorizontalAlignment,
                    InputCanvasRight,
                    (a, v) => a == HorizontalAlignment.Right ? v : null)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            CanvasTop = Observable
                .CombineLatest(
                    CanvasVerticalAlignment,
                    InputCanvasTop,
                    (a, v) => a == VerticalAlignment.Top ? v : null)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            CanvasBottom = Observable
                .CombineLatest(
                    CanvasVerticalAlignment,
                    InputCanvasBottom,
                    (a, v) => a == VerticalAlignment.Bottom ? v : null)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            IsAutoSize = new (IsAutoSizeSupported);
            Width = Observable
                .CombineLatest(
                    IsAutoSize,
                    InputWidth,
                    (auto, v) => auto ? null : v)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            Height = Observable
                .CombineLatest(
                    IsAutoSize,
                    InputHeight,
                    (auto, v) => auto ? null : v)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

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

        protected static void SelectColor(ReactiveProperty<Color> target)
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

        protected static void SelectImageFilePath(ReactiveProperty<string> target)
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

        #region IEditor
        public class BaseEditorViewModelParameter
        {
            public static readonly string Key = "Base";

            [JsonProperty]
            public string Id { get; init; } = "";
            [JsonProperty]
            public string Name { get; init; } = "";
            [JsonProperty]
            [JsonConverter(typeof(StringEnumConverter))]
            public HorizontalAlignment CanvasHorizontalAlignment { get; init; } = HorizontalAlignment.Left;
            [JsonProperty]
            [JsonConverter(typeof(StringEnumConverter))]
            public VerticalAlignment CanvasVerticalAlignment { get; init; } = VerticalAlignment.Top;
            [JsonProperty]
            public double? InputCanvasLeft { get; init; } = 0d;
            [JsonProperty]
            public double? InputCanvasRight { get; init; } = 0d;
            [JsonProperty]
            public double? InputCanvasTop { get; init; } = 0d;
            [JsonProperty]
            public double? InputCanvasBottom { get; init; } = 0d;
            [JsonProperty]
            public bool IsAutoSize { get; init; } = true;
            [JsonProperty]
            public double? InputWidth { get; init; } = 0d;
            [JsonProperty]
            public double? InputHeight { get; init; } = 0d;
        }

        public virtual Task<JObject> SaveAsync(SaveAccessory accessory)
        {
            var vmParam = new BaseEditorViewModelParameter()
            {
                Id                        = Id.Value,
                Name                      = Name.Value,
                CanvasHorizontalAlignment = CanvasHorizontalAlignment.Value,
                CanvasVerticalAlignment   = CanvasVerticalAlignment.Value,
                InputCanvasLeft           = InputCanvasLeft.Value,
                InputCanvasRight          = InputCanvasRight.Value,
                InputCanvasTop            = InputCanvasTop.Value,
                InputCanvasBottom         = InputCanvasBottom.Value,
                IsAutoSize                = IsAutoSize.Value,
                InputWidth                = InputWidth.Value,
                InputHeight               = InputHeight.Value,
            };

            var jobject = new JObject
            {
                [BaseEditorViewModelParameter.Key] = JToken.FromObject(vmParam)
            };
            return Task.FromResult(jobject);
        }

        public virtual Task LoadAsync(LoadAccessory accessory, JObject jobject)
        {
            if (!jobject.TryGetValue(BaseEditorViewModelParameter.Key, out var val))
                return Task.CompletedTask;

            var param = val.ToObject<BaseEditorViewModelParameter>();
            if (param is null)
                return Task.CompletedTask;

            Id.Value                        = param.Id;
            Name.Value                      = param.Name;
            CanvasHorizontalAlignment.Value = param.CanvasHorizontalAlignment;
            CanvasVerticalAlignment.Value   = param.CanvasVerticalAlignment;
            InputCanvasLeft.Value           = param.InputCanvasLeft;
            InputCanvasRight.Value          = param.InputCanvasRight;
            InputCanvasTop.Value            = param.InputCanvasTop;
            InputCanvasBottom.Value         = param.InputCanvasBottom;
            IsAutoSize.Value                = param.IsAutoSize;
            InputWidth.Value                = param.InputWidth;
            InputHeight.Value               = param.InputHeight;

            return Task.CompletedTask;
        }
        #endregion
    }
}
