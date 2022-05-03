using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.ViewModels.Editors;
using TuringSmartScreenTool.Views;

namespace TuringSmartScreenTool.ViewModels
{
    public class CanvasEditorWindowViewModel : IDisposable
    {
        public enum EditorType
        {
            Text,
            Image,
            HardwareName,
            HardwareValueText,
            HardwareValueIndicator,
            DateTime,
            Weather,
        }

        private static readonly IEnumerable<EditorType> s_editorTypeCollection = Enum.GetValues(typeof(EditorType)).Cast<EditorType>();

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private readonly ILogger<CanvasEditorWindowViewModel> _logger;
        private readonly IHardwareSelectContentDialog _hardwareSelectContentDialog;
        private readonly ILocationSelectContentDialog _locationSelectContentDialog;
        private readonly IWeatherIconPreviewContentDialog _weatherIconPreviewContentDialog;
        private readonly IHardwareFinder _hardwareFinder;
        private readonly ISensorFinder _sensorFinder;
        private readonly ITimeManager _timeManager;
        private readonly IWeatherManager _weatherManager;

        public ObservableCollection<BaseEditorViewModel> EditorViewModels { get; } = new();
        public ReactiveProperty<int> SelectedEditorViewModelIndex { get; } = new(-1);
        public ReadOnlyReactiveProperty<BaseEditorViewModel> SelectedEditorViewModel { get; }

        public IEnumerable<EditorType> EditorCollection { get; } = s_editorTypeCollection;
        public ReactiveProperty<EditorType> SelectedEditor { get; } = new(s_editorTypeCollection.FirstOrDefault());

        public ReactiveProperty<int> CanvasWidth { get; } = new(0);
        public ReactiveProperty<int> CanvasHeight { get; } = new(0);

        public ReactiveProperty<CanvasBackgroundType> InputCanvasBackgroundType { get; } = new(CanvasBackgroundType.SolidColor);
        public ReactiveProperty<Color> InputCanvasBackgroundColor { get; } = new(Colors.Black);
        public ReactiveProperty<string> InputCanvasBackgroundImagePath { get; } = new();
        public ReadOnlyReactiveProperty<object> CanvasBackground { get; }

        public ICommand SelectBackgroundColorCommand { get; }
        public ICommand SelectBackgroundImageCommand { get; }

        public ICommand AddEditorCommand { get; }
        public ICommand MoveUpEditorCommand { get; }
        public ICommand MoveDownEditorCommand { get; }
        public ICommand DeleteSelectedEditorCommand { get; }

        public CanvasEditorWindowViewModel(
            ILogger<CanvasEditorWindowViewModel> logger,
            IHardwareSelectContentDialog hardwareSelectContentDialog,
            ILocationSelectContentDialog locationSelectContentDialog,
            IWeatherIconPreviewContentDialog weatherIconPreviewContentDialog,
            // TODO: usecase
            IHardwareFinder hardwareFinder,
            ISensorFinder sensorFinder,
            ITimeManager timeManager,
            IWeatherManager weatherManager)
        {
            _logger = logger;
            _hardwareSelectContentDialog = hardwareSelectContentDialog;
            _locationSelectContentDialog = locationSelectContentDialog;
            _weatherIconPreviewContentDialog = weatherIconPreviewContentDialog;
            _hardwareFinder = hardwareFinder;
            _sensorFinder = sensorFinder;
            _timeManager = timeManager;
            _weatherManager = weatherManager;

            SelectedEditorViewModel = SelectedEditorViewModelIndex
                .Select(idx => EditorViewModels.ElementAtOrDefault(idx))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            CanvasBackground =
                Observable.CombineLatest(
                    InputCanvasBackgroundType,
                    InputCanvasBackgroundColor,
                    InputCanvasBackgroundImagePath,
                    (type, color, path) => (object)(type switch
                    {
                        CanvasBackgroundType.SolidColor => color,
                        CanvasBackgroundType.Image => path,
                        _ => throw new InvalidOperationException(),
                    }))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            SelectBackgroundColorCommand = InputCanvasBackgroundType
                .Select(x => x == CanvasBackgroundType.SolidColor)
                .ToReactiveCommand()
                .WithSubscribe(_ => SelectColor(InputCanvasBackgroundColor))
                .AddTo(_disposables);
            SelectBackgroundImageCommand = InputCanvasBackgroundType
                .Select(x => x == CanvasBackgroundType.Image)
                .ToReactiveCommand()
                .WithSubscribe(_ => SelectImageFilePath(InputCanvasBackgroundImagePath))
                .AddTo(_disposables);
            AddEditorCommand = new RelayCommand(() => AddEditor());
            MoveUpEditorCommand = SelectedEditorViewModelIndex
                .Select(idx => idx != -1 && idx > 0)
                .ToReactiveCommand()
                .WithSubscribe(_ => MoveUpSelectedEditor())
                .AddTo(_disposables);
            MoveDownEditorCommand = SelectedEditorViewModelIndex
                .Select(idx => idx != -1 && idx < EditorViewModels.Count - 1)
                .ToReactiveCommand()
                .WithSubscribe(_ => MoveDownSelectedEditor())
                .AddTo(_disposables);
            DeleteSelectedEditorCommand = SelectedEditorViewModel
                .Select(x => x != null)
                .ToReactiveCommand()
                .WithSubscribe(_ => DeleteSelectedEditor())
                .AddTo(_disposables);

            // TODO: delete
            CanvasWidth.Value = 320;
            CanvasHeight.Value = 480;
            SelectedEditorViewModelIndex.Value = EditorViewModels.Count > 0 ? 0 : -1;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void AddEditor()
        {
            BaseEditorViewModel editorViewModel = SelectedEditor.Value switch
            {
                EditorType.Text => new StaticTextBlockEditorViewModel(),
                EditorType.Image => new ImageEditorViewModel(),
                EditorType.HardwareName => new HardwareNameTextBlockEditorViewModel(_hardwareSelectContentDialog, _hardwareFinder),
                EditorType.HardwareValueText => new HardwareSensorTextBlockEditorViewModel(_hardwareSelectContentDialog, _sensorFinder),
                EditorType.HardwareValueIndicator => new HardwareSensorIndicatorEditorViewModel(_hardwareSelectContentDialog, _sensorFinder),
                EditorType.DateTime => new DateTimeTextEditorViewModel(_timeManager),
                EditorType.Weather => new WeatherTextEditorViewModel(_weatherManager, _locationSelectContentDialog, _weatherIconPreviewContentDialog),
                _ => throw new InvalidOperationException(),
            };

            EditorViewModels.Add(editorViewModel);
        }

        private void MoveUpSelectedEditor()
        {
            var index = SelectedEditorViewModelIndex.Value;

            // not found
            if (index == -1)
                return;

            if (index > 0)
            {
                var item = EditorViewModels[index];
                EditorViewModels.RemoveAt(index);
                EditorViewModels.Insert(index - 1, item);
                SelectedEditorViewModelIndex.Value = index - 1;
            }
        }

        private void MoveDownSelectedEditor()
        {
            var index = SelectedEditorViewModelIndex.Value;

            // not found
            if (index == -1)
                return;

            if (index < EditorViewModels.Count - 1)
            {
                var item = EditorViewModels[index];
                EditorViewModels.RemoveAt(index);
                EditorViewModels.Insert(index + 1, item);
                SelectedEditorViewModelIndex.Value = index + 1;
            }
        }

        private void DeleteSelectedEditor()
        {
            var index = SelectedEditorViewModelIndex.Value;

            // not found
            if (index == -1)
                return;

            if (index <= EditorViewModels.Count - 1)
            {
                EditorViewModels.RemoveAt(index);

                if (EditorViewModels.Count > 0)
                    // select previous element
                    SelectedEditorViewModelIndex.Value = index == 0 ? 0 : index - 1;
                else
                    SelectedEditorViewModelIndex.Value = -1;
            }
        }

        private void SelectColor(ReactiveProperty<Color> target)
        {
            var c = target.Value;
            var colorDialog = new ColorDialog()
            {
                Color = System.Drawing.Color.FromArgb(c.R, c.G, c.B),
                FullOpen = true,
                AnyColor = false
            };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                var newColor = colorDialog.Color;
                target.Value = Color.FromRgb(newColor.R, newColor.G, newColor.B);
            }
        }

        private void SelectImageFilePath(ReactiveProperty<string> target)
        {
            var fileDialog = new OpenFileDialog()
            {
                Filter = "Image|*.png;*.jpg;*.jpeg;*.bmp;*.gif" + "|All Files|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Multiselect = false
            };
            if (fileDialog.ShowDialog() == DialogResult.OK &&
                File.Exists(fileDialog.FileName))
            {
                target.Value = fileDialog.FileName;
            }
        }
    }
}
