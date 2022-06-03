using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.Helpers;
using TuringSmartScreenTool.UseCases.Interfaces;
using TuringSmartScreenTool.ViewModels.Editors;

namespace TuringSmartScreenTool.ViewModels.Controls
{
    public class CanvasEditorListViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        private readonly IServiceProvider _serviceProvider;
        private readonly IEditCanvasUseCase _editCanvasUseCase;

        public ReactiveProperty<CanvasSize> SelectedCanvasSize { get; } = new();
        public ReadOnlyReactiveProperty<int> CanvasWidth { get; }
        public ReadOnlyReactiveProperty<int> CanvasHeight { get; }

        public ReactiveProperty<CanvasBackgroundType> BackgroundType { get; } = new();
        public ReactiveProperty<Color> BackgroundColor { get; } = new();
        public ReactiveProperty<string> BackgroundImagePath { get; } = new();
        public ReadOnlyReactiveProperty<object> Background { get; }

        public ObservableCollection<BaseEditorViewModel> EditorViewModels { get; } = new();
        public ReactiveProperty<int> SelectedEditorViewModelIndex { get; } = new();
        public ReadOnlyReactiveProperty<BaseEditorViewModel> SelectedEditorViewModel { get; }
        public ReadOnlyReactiveProperty<bool> IsEditorViewModelSelected { get; }

        public CanvasEditorListViewModel(
            IServiceProvider serviceProvider,
            IEditCanvasUseCase editCanvasUseCase)
        {
            _serviceProvider = serviceProvider;
            _editCanvasUseCase = editCanvasUseCase;

            Background =
                Observable.CombineLatest(
                    BackgroundType,
                    BackgroundColor,
                    BackgroundImagePath,
                    (type, color, path) => (object)(type switch
                    {
                        CanvasBackgroundType.SolidColor => color,
                        CanvasBackgroundType.Image => path,
                        _ => throw new InvalidOperationException(),
                    }))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            SelectedEditorViewModel = SelectedEditorViewModelIndex
                .Select(idx => EditorViewModels.ElementAtOrDefault(idx))
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            IsEditorViewModelSelected = SelectedEditorViewModelIndex
                .Select(idx => idx == -1)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            CanvasWidth = SelectedCanvasSize
                .Select(x => ConvertToSize(x).width)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            CanvasHeight = SelectedCanvasSize
                .Select(x => ConvertToSize(x).height)
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            // init values
            ClearEditors();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void ClearEditors()
        {
            SelectedEditorViewModelIndex.Value = -1;
            EditorViewModels.Clear();

            SelectedCanvasSize.Value  = CanvasSize._480x320;
            BackgroundType.Value      = CanvasBackgroundType.SolidColor;
            BackgroundColor.Value     = Colors.Black;
            BackgroundImagePath.Value = null;
        }

        public void AddEditor(EditorType type)
        {
            var editorViewModel = CreateEditorViewModel(type);
            EditorViewModels.Add(editorViewModel);
            SelectedEditorViewModelIndex.Value = EditorViewModels.Count - 1;
        }

        public async void DupilicateSelectedEditor()
        {
            var vm = SelectedEditorViewModel.Value;
            if (vm is null)
                return;

            var jobject = await vm.SaveAsync(null);
            var copiedVm = CreateEditorViewModel(vm.EditorType);
            await copiedVm.LoadAsync(null, jobject);
            copiedVm.Name.Value = copiedVm.Name.Value + " - Copy";

            EditorViewModels.Add(copiedVm);
            SelectedEditorViewModelIndex.Value = EditorViewModels.Count - 1;
        }

        public void MoveUpSelectedEditor()
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

        public void MoveDownSelectedEditor()
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

        public void DeleteSelectedEditor()
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

        public async Task LoadFromFileAsync(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var tempDirectory = DirectoryInfoHelper.GetTempDirectory();
            var editorFileData = await _editCanvasUseCase.LoadFromFileAsync(fileInfo, tempDirectory, CreateEditorViewModel);
            ClearEditors();
            foreach (var (_, editor) in editorFileData.Editors)
            {
                if (editor is BaseEditorViewModel vm)
                {
                    EditorViewModels.Add(vm);
                }
            }
            SelectedCanvasSize.Value           = editorFileData.CanvasSize;
            BackgroundType.Value               = editorFileData.CanvasBackgroundType;
            BackgroundColor.Value              = ColorHelper.FromString(editorFileData.CanvasBackgroundColor);
            BackgroundImagePath.Value          = editorFileData.CanvasBackgroundImagePath;
            SelectedEditorViewModelIndex.Value = EditorViewModels.Count > 0 ? 0 : -1;
        }

        public async Task SaveAsFileEditorAsync(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            var editors = EditorViewModels
                .Select(x => (x.EditorType, (IEditor)x))
                .ToList();
            var editorFileData = new EditorFileData()
            {
                Editors                   = editors,
                CanvasSize                = SelectedCanvasSize.Value,
                CanvasBackgroundType      = BackgroundType.Value,
                CanvasBackgroundColor     = ColorHelper.ToString(BackgroundColor.Value),
                CanvasBackgroundImagePath = BackgroundImagePath.Value
            };
            await _editCanvasUseCase.SaveEditorAsFileAsync(fileInfo, editorFileData);
        }

        private static (int width, int height) ConvertToSize(CanvasSize canvasSize)
        {
            return canvasSize switch
            {
                CanvasSize._320x480 => (320, 480),
                CanvasSize._480x320 => (480, 320),
                _ => throw new InvalidOperationException()
            };
        }

        private BaseEditorViewModel CreateEditorViewModel(EditorType type)
        {
            return type switch
            {
                EditorType.Text                   => _serviceProvider.GetService<StaticTextBlockEditorViewModel>(),
                EditorType.Image                  => _serviceProvider.GetService<ImageEditorViewModel>(),
                EditorType.HardwareName           => _serviceProvider.GetService<HardwareNameTextBlockEditorViewModel>(),
                EditorType.HardwareValueText      => _serviceProvider.GetService<HardwareSensorTextBlockEditorViewModel>(),
                EditorType.HardwareValueIndicator => _serviceProvider.GetService<HardwareSensorIndicatorEditorViewModel>(),
                EditorType.DateTime               => _serviceProvider.GetService<DateTimeTextEditorViewModel>(),
                EditorType.Weather                => _serviceProvider.GetService<WeatherTextEditorViewModel>(),
                _                                 => throw new InvalidOperationException(),
            };
        }
    }
}
