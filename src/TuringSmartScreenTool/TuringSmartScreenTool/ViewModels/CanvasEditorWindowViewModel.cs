using System;
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
using TuringSmartScreenTool.Controllers;
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
        }

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private readonly ILogger<CanvasEditorWindowViewModel> _logger;
        private readonly IHardwareSelectContentDialog _hardwareSelectContentDialog;
        private readonly IHardwareFinder _hardwareFinder;
        private readonly ISensorFinder _sensorFinder;
        private readonly ITimeManager _timeManager;

        public ObservableCollection<BaseEditorViewModel> EditorViewModels { get; } = new() { new StaticTextBlockEditorViewModel(), new ImageEditorViewModel() };
        public ReactiveProperty<int> SelectedEditorViewModelIndex { get; } = new(-1);
        public ReadOnlyReactiveProperty<BaseEditorViewModel> SelectedEditorViewModel { get; }

        public ObservableCollection<EditorType> EditorCollection { get; } = new(GetEditorCollection());
        public ReactiveProperty<EditorType> SelectedEditor { get; } = new(GetEditorCollection().FirstOrDefault());

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
            // TODO: usecase
            IHardwareFinder hardwareFinder,
            ISensorFinder sensorFinder,
            ITimeManager timeManager)
        {
            _logger = logger;
            _hardwareSelectContentDialog = hardwareSelectContentDialog;
            _hardwareFinder = hardwareFinder;
            _sensorFinder = sensorFinder;
            _timeManager = timeManager;

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
            BaseEditorViewModel editorViewModel;
            switch (SelectedEditor.Value)
            {
                case EditorType.Text:
                    editorViewModel = new StaticTextBlockEditorViewModel();
                    break;
                case EditorType.Image:
                    editorViewModel = new ImageEditorViewModel();
                    break;
                case EditorType.HardwareName:
                    editorViewModel = new HardwareNameTextBlockEditorViewModel(
                        _hardwareSelectContentDialog,
                        _hardwareFinder);
                    break;
                case EditorType.HardwareValueText:
                    editorViewModel = new HardwareSensorTextBlockEditorViewModel(
                        _hardwareSelectContentDialog,
                        _sensorFinder);
                    break;
                case EditorType.HardwareValueIndicator:
                    editorViewModel = new HardwareSensorIndicatorEditorViewModel(
                        _hardwareSelectContentDialog,
                        _sensorFinder);
                    break;
                case EditorType.DateTime:
                    editorViewModel = new DateTimeTextEditorViewModel(_timeManager);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (editorViewModel is null)
                return;

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

        private static ObservableCollection<EditorType> GetEditorCollection()
        {
            return new(Enum.GetValues(typeof(EditorType)).Cast<EditorType>());
        }
    }
}
