using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.UseCases.Interfaces;
using TuringSmartScreenTool.ViewModels.Controls;

namespace TuringSmartScreenTool.ViewModels.Pages
{
    public class CanvasEditorPageViewModel : INavigationAware, IDisposable
    {
        private static readonly IEnumerable<CanvasSize> s_canvasTypeCollection = new[] { CanvasSize._320x480, CanvasSize._480x320 };
        private static readonly IEnumerable<EditorType> s_editorTypeCollection = Enum.GetValues(typeof(EditorType)).Cast<EditorType>();

        private readonly CompositeDisposable _disposables = new();

        private readonly ILogger<CanvasEditorPageViewModel> _logger;
        private readonly CanvasEditorListViewModel _listVM;
        private readonly IEditCanvasUseCase _editCanvasUseCase;

        public IEnumerable<CanvasSize> CanvasSizeCollection { get; } = s_canvasTypeCollection;

        public IEnumerable<EditorType> EditorCollection { get; } = s_editorTypeCollection;
        public ReactiveProperty<EditorType> SelectedEditor { get; } = new(s_editorTypeCollection.FirstOrDefault());

        public CanvasEditorListViewModel ListVM => _listVM;

        public ICommand SelectBackgroundColorCommand { get; }
        public ICommand SelectBackgroundImageCommand { get; }

        public ICommand AddEditorCommand { get; }
        public ICommand DuplicateCommand { get; }
        public ICommand MoveUpEditorCommand { get; }
        public ICommand MoveDownEditorCommand { get; }
        public ICommand DeleteSelectedEditorCommand { get; }
        public ICommand SaveAsFileCommand { get; }
        public ICommand LoadFromFileCommand { get; }
        public ICommand ClearCommand { get; }

        public CanvasEditorPageViewModel(
            ILogger<CanvasEditorPageViewModel> logger,
            CanvasEditorListViewModel canvasEditorListViewModel,
            IEditCanvasUseCase editCanvasUseCase)
        {
            _logger = logger;
            _listVM = canvasEditorListViewModel;
            _editCanvasUseCase = editCanvasUseCase;

            SelectBackgroundColorCommand = _listVM.BackgroundType
                .Select(x => x == CanvasBackgroundType.SolidColor)
                .ToReactiveCommand()
                .WithSubscribe(_ => SelectColor(_listVM.BackgroundColor))
                .AddTo(_disposables);
            SelectBackgroundImageCommand = _listVM.BackgroundType
                .Select(x => x == CanvasBackgroundType.Image)
                .ToReactiveCommand()
                .WithSubscribe(_ => SelectImageFilePath(_listVM.BackgroundImagePath))
                .AddTo(_disposables);
            AddEditorCommand = new RelayCommand(() => _listVM.AddEditor(SelectedEditor.Value));
            DuplicateCommand = _listVM.SelectedEditorViewModel
                .Select(x => x != null)
                .ToReactiveCommand()
                .WithSubscribe(_ => _listVM.DupilicateSelectedEditor())
                .AddTo(_disposables);
            MoveUpEditorCommand = _listVM.SelectedEditorViewModelIndex
                .Select(idx => idx != -1 && idx > 0)
                .ToReactiveCommand()
                .WithSubscribe(_ => _listVM.MoveUpSelectedEditor())
                .AddTo(_disposables);
            MoveDownEditorCommand = _listVM.SelectedEditorViewModelIndex
                .Select(idx => idx != -1 && idx < _listVM.EditorViewModels.Count - 1)
                .ToReactiveCommand()
                .WithSubscribe(_ => _listVM.MoveDownSelectedEditor())
                .AddTo(_disposables);
            DeleteSelectedEditorCommand = _listVM.SelectedEditorViewModel
                .Select(x => x != null)
                .ToReactiveCommand()
                .WithSubscribe(_ => _listVM.DeleteSelectedEditor())
                .AddTo(_disposables);
            SaveAsFileCommand = new AsyncReactiveCommand()
                .WithSubscribe(x => SaveAsFileEditorAsync())
                .AddTo(_disposables);
            LoadFromFileCommand = new AsyncReactiveCommand()
                .WithSubscribe(x => LoadFromFileAsync())
                .AddTo(_disposables);
            ClearCommand = new ReactiveCommand()
                .WithSubscribe(x => ClearEditor())
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _listVM.Dispose();
            _disposables.Dispose();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private static void SelectColor(ReactiveProperty<Color> target)
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

        private static void SelectImageFilePath(ReactiveProperty<string> target)
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

        private async Task LoadFromFileAsync()
        {
            var fileDialog = new OpenFileDialog()
            {
                Filter = "TSS Files(*.tss)|*.tss" + "|All Files(*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Multiselect = false
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            await _listVM.LoadFromFileAsync(fileDialog.FileName);
        }

        private async Task SaveAsFileEditorAsync()
        {
            var saveFileDialog = new SaveFileDialog()
            {
                DefaultExt = _editCanvasUseCase.GetFileExtension(),
                FileName = DateTime.Now.ToString("yyyyMMddhhmm"),
                RestoreDirectory = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "TSS Files(*.tss)|*.tss" + "|All Files(*.*)|*.*",
                OverwritePrompt = true
            };

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            await _listVM.SaveAsFileEditorAsync(saveFileDialog.FileName);
        }

        private void ClearEditor()
        {
            // TODO: localize
            if (MessageBox.Show("Dou you clear canvas?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                _listVM.ClearEditors();
            }
        }
    }
}
